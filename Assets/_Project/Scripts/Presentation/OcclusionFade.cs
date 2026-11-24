using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Turns a large enemy translucent while it stands between the camera and the player.</summary>
    public sealed class OcclusionFade : MonoBehaviour
    {
        private const float HoldTime = 0.18f;

        [SerializeField] private EnemyBrain brain;
        [SerializeField] private HitFlash flash;
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private float halfWidth = 1.7f;
        [SerializeField] private float height = 3.4f;

        private Camera viewCamera;
        private float holdUntil;

        public bool IsFaded { get; private set; }

        private void Awake() => viewCamera = Camera.main;

        private void LateUpdate()
        {
            var target = brain.CurrentTarget;
            if (target == null || viewCamera == null || brain.State == EnemyState.Dead)
            {
                SetFaded(false);
                return;
            }

            if (Covers(target.position + Vector3.up * 1.05f) || Covers(target.position + Vector3.up * 1.8f)) holdUntil = Time.unscaledTime + HoldTime;
            SetFaded(Time.unscaledTime < holdUntil);
        }

        private bool Covers(Vector3 point)
        {
            var bounds = new Bounds(transform.position + Vector3.up * (height * 0.5f), new Vector3(halfWidth * 2f, height, halfWidth * 2f));
            return bounds.IntersectRay(new Ray(point, -viewCamera.transform.forward));
        }

        private void SetFaded(bool faded)
        {
            if (faded == IsFaded) return;
            IsFaded = faded;
            flash.SetOverride(faded ? ghostMaterial : null);
        }
    }
}
