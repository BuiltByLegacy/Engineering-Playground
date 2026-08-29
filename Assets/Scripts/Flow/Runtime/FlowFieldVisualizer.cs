using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Runtime
{
    public enum FlowViewMode
    {
        Flow,
        Velocity,
        Pressure,
        Vorticity
    }

    public sealed class FlowFieldVisualizer : MonoBehaviour
    {
        [SerializeField] private FlowLabRuntimeController controller;
        [SerializeField] private RawImage target;
        [SerializeField] private FlowViewMode viewMode = FlowViewMode.Flow;

        private Texture2D _texture;
        private Color32[] _pixels;

        public void Configure(FlowLabRuntimeController runtimeController, RawImage rawImage)
        {
            controller = runtimeController;
            target = rawImage;
        }

        private void Start()
        {
            if (controller == null)
                controller = GetComponent<FlowLabRuntimeController>();
            BuildTexture();
            controller.SolverUpdated += Render;
            Render();
        }

        private void OnDestroy()
        {
            if (controller != null)
                controller.SolverUpdated -= Render;
        }

        public void SetViewMode(FlowViewMode mode)
        {
            viewMode = mode;
            Render();
        }

        private void BuildTexture()
        {
            var solver = controller.Solver;
            _texture = new Texture2D(solver.Width, solver.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _pixels = new Color32[solver.Width * solver.Height];
            if (target != null)
                target.texture = _texture;
        }

        private void Render()
        {
            if (controller == null || controller.Solver == null || _texture == null)
                return;

            var solver = controller.Solver;
            var rho = solver.Density;
            var ux = solver.VelocityX;
            var uy = solver.VelocityY;
            var solid = solver.Solid;

            for (var y = 0; y < solver.Height; y++)
            {
                for (var x = 0; x < solver.Width; x++)
                {
                    var i = y * solver.Width + x;
                    if (solid[i])
                    {
                        _pixels[i] = new Color32(25, 29, 38, 255);
                        continue;
                    }

                    var speed = Mathf.Clamp01((float)(System.Math.Sqrt(ux[i] * ux[i] + uy[i] * uy[i]) / 0.10));
                    var densityDelta = Mathf.Clamp((float)((rho[i] - 1.0) / 0.05), -1f, 1f);
                    var vorticity = Mathf.Clamp((float)(solver.VorticityAt(x, y) / 0.02), -1f, 1f);

                    switch (viewMode)
                    {
                        case FlowViewMode.Pressure:
                            _pixels[i] = Diverging(0.5f + 0.5f * densityDelta);
                            break;
                        case FlowViewMode.Vorticity:
                            _pixels[i] = Diverging(0.5f + 0.5f * vorticity);
                            break;
                        case FlowViewMode.Velocity:
                            _pixels[i] = Sequential(speed);
                            break;
                        default:
                            _pixels[i] = FlowColor(speed, (float)ux[i], (float)uy[i]);
                            break;
                    }
                }
            }

            _texture.SetPixels32(_pixels);
            _texture.Apply(false, false);
        }

        private static Color32 FlowColor(float speed, float ux, float uy)
        {
            var directionBias = Mathf.Clamp01(0.5f + 4f * uy);
            var baseColor = Color.Lerp(new Color(0.08f, 0.28f, 0.58f), new Color(0.10f, 0.75f, 0.88f), speed);
            var accent = Color.Lerp(baseColor, new Color(0.93f, 0.91f, 0.38f), Mathf.Clamp01(speed * directionBias * 0.45f));
            return accent;
        }

        private static Color32 Sequential(float t)
        {
            t = Mathf.Clamp01(t);
            return Color.Lerp(new Color(0.11f, 0.20f, 0.35f), new Color(0.92f, 0.89f, 0.33f), t);
        }

        private static Color32 Diverging(float t)
        {
            t = Mathf.Clamp01(t);
            if (t < 0.5f)
                return Color.Lerp(new Color(0.14f, 0.36f, 0.75f), new Color(0.90f, 0.92f, 0.96f), t * 2f);
            return Color.Lerp(new Color(0.90f, 0.92f, 0.96f), new Color(0.80f, 0.21f, 0.22f), (t - 0.5f) * 2f);
        }
    }
}
