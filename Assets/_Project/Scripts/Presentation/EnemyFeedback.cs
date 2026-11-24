using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Spawn, telegraph and death feedback. Every enemy attack is announced before it lands.</summary>
    public sealed class EnemyFeedback : MonoBehaviour
    {
        private const float SpawnTime = 0.6f;
        private const float LineWidth = 0.55f;

        [SerializeField] private EnemyBrain brain;
        [SerializeField] private Transform visual;
        [SerializeField] private Transform warningDisc;
        [SerializeField] private Transform aimLine;
        [SerializeField] private float aimLength = 8f;

        private float stateTime;

        private void Awake()
        {
            brain.StateChanged += OnStateChanged;
            visual.localScale = Vector3.one * 0.05f;
            warningDisc.gameObject.SetActive(true);
            if (aimLine != null) aimLine.gameObject.SetActive(false);
        }

        private void OnDestroy() => brain.StateChanged -= OnStateChanged;

        private void OnStateChanged(EnemyState state)
        {
            stateTime = 0f;
            // Bosses draw their real attack area with BossTelegraphView.
            var telegraphing = state == EnemyState.Telegraph && !brain.IsBoss;
            warningDisc.gameObject.SetActive(telegraphing);
            if (aimLine != null) aimLine.gameObject.SetActive(telegraphing);
        }

        private void Update()
        {
            stateTime += Time.deltaTime;
            Vector3 target;
            switch (brain.State)
            {
                case EnemyState.Dormant:
                    target = Vector3.one * Mathf.Clamp01(stateTime / SpawnTime);
                    SetDisc(Mathf.Clamp01(stateTime / SpawnTime));
                    break;
                case EnemyState.Telegraph:
                    var progress = Mathf.Clamp01(stateTime / brain.CurrentTelegraph);
                    target = new Vector3(1.18f, 0.78f, 1.18f);
                    SetDisc(progress);
                    SetLine(progress);
                    break;
                case EnemyState.Attack:
                    target = new Vector3(0.88f, 1.08f, 1.3f);
                    break;
                case EnemyState.Dead:
                    target = new Vector3(1.25f, 0.05f, 1.25f);
                    break;
                default:
                    target = Vector3.one;
                    break;
            }

            visual.localScale = Vector3.Lerp(visual.localScale, target, 1f - Mathf.Exp(-22f * Time.deltaTime));
        }

        private void SetDisc(float progress)
        {
            var size = Mathf.Lerp(0.5f, 2.6f, progress);
            warningDisc.localScale = new Vector3(size, 0.01f, size);
        }

        private void SetLine(float progress)
        {
            if (aimLine == null) return;
            var length = aimLength * progress;
            aimLine.localScale = new Vector3(LineWidth, 0.01f, length);
            aimLine.localPosition = new Vector3(0f, 0.05f, length * 0.5f);
        }
    }
}
