using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Shows the full danger area from the start, with a fill that grows until the attack lands.</summary>
    public sealed class BossTelegraphView : MonoBehaviour
    {
        private const int ConeSegments = 22;
        private const float FloorHeight = 0.07f;
        private const float BackHeight = 0.055f;

        [SerializeField] private EnemyBrain brain;
        [SerializeField] private Transform[] discs;
        [SerializeField] private Transform[] discBacks;
        [SerializeField] private Transform line;
        [SerializeField] private Transform lineBack;
        [SerializeField] private MeshFilter coneFilter;
        [SerializeField] private MeshRenderer coneRenderer;
        [SerializeField] private MeshFilter coneBackFilter;
        [SerializeField] private MeshRenderer coneBackRenderer;

        private TelegraphInfo info;
        private Mesh coneMesh;
        private float elapsed;

        private void Awake()
        {
            coneMesh = new Mesh { name = "BossCone" };
            coneFilter.sharedMesh = coneMesh;
            coneBackFilter.sharedMesh = coneMesh;
            brain.TelegraphStarted += OnTelegraphStarted;
            brain.StateChanged += OnStateChanged;
            Hide();
        }

        private void OnDestroy()
        {
            brain.TelegraphStarted -= OnTelegraphStarted;
            brain.StateChanged -= OnStateChanged;
            Destroy(coneMesh);
        }

        private void OnTelegraphStarted(TelegraphInfo telegraph)
        {
            Hide();
            info = telegraph;
            elapsed = 0f;
            var isCone = info.Shape == TelegraphShape.Cone;
            if (isCone) BuildCone(info.Radius, info.ArcDegrees);
            coneRenderer.enabled = isCone;
            coneBackRenderer.enabled = isCone;
            var isLine = info.Shape == TelegraphShape.Line;
            line.gameObject.SetActive(isLine);
            lineBack.gameObject.SetActive(isLine);
            if (isLine)
            {
                lineBack.localScale = new Vector3(info.Width, 0.01f, info.Length);
                lineBack.localPosition = new Vector3(0f, BackHeight, info.Length * 0.5f);
            }

            if (info.Shape != TelegraphShape.Circle && info.Shape != TelegraphShape.Bursts) return;
            for (var i = 0; i < discs.Length; i++)
            {
                var active = i < info.Points.Length;
                discs[i].gameObject.SetActive(active);
                discBacks[i].gameObject.SetActive(active);
            }
        }

        private void OnStateChanged(EnemyState state)
        {
            if (state != EnemyState.Telegraph) Hide();
        }

        private void LateUpdate()
        {
            if (info == null) return;
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, info.Duration));
            var grow = Mathf.Lerp(0.12f, 1f, progress);

            switch (info.Shape)
            {
                case TelegraphShape.Circle:
                case TelegraphShape.Bursts:
                    var diameter = info.Radius * 2f;
                    for (var i = 0; i < info.Points.Length && i < discs.Length; i++)
                    {
                        discBacks[i].position = info.Points[i] + Vector3.up * BackHeight;
                        discBacks[i].localScale = new Vector3(diameter, 0.01f, diameter);
                        discs[i].position = info.Points[i] + Vector3.up * FloorHeight;
                        discs[i].localScale = new Vector3(diameter * grow, 0.01f, diameter * grow);
                    }

                    break;
                case TelegraphShape.Line:
                    var length = info.Length * grow;
                    line.localScale = new Vector3(info.Width, 0.01f, length);
                    line.localPosition = new Vector3(0f, FloorHeight, length * 0.5f);
                    break;
                case TelegraphShape.Cone:
                    coneFilter.transform.localScale = new Vector3(grow, 1f, grow);
                    break;
            }
        }

        private void Hide()
        {
            info = null;
            foreach (var disc in discs) disc.gameObject.SetActive(false);
            foreach (var disc in discBacks) disc.gameObject.SetActive(false);
            line.gameObject.SetActive(false);
            lineBack.gameObject.SetActive(false);
            coneRenderer.enabled = false;
            coneBackRenderer.enabled = false;
        }

        private void BuildCone(float radius, float arcDegrees)
        {
            var vertices = new Vector3[ConeSegments + 2];
            var triangles = new int[ConeSegments * 3];
            vertices[0] = Vector3.zero;
            for (var i = 0; i <= ConeSegments; i++)
            {
                var angle = Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, i / (float)ConeSegments) * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
                if (i == ConeSegments) continue;
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            coneMesh.Clear();
            coneMesh.vertices = vertices;
            coneMesh.triangles = triangles;
            coneMesh.RecalculateNormals();
            coneMesh.RecalculateBounds();
        }
    }
}
