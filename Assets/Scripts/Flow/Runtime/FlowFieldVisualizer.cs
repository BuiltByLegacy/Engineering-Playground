using EngineeringPlayground.Flow.Simulation;
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
                filterMode = FilterMode.Point,
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
                    var value = viewMode switch
                    {
                        FlowViewMode.Pressure => 0.5f + 0.5f * densityDelta,
                        FlowViewMode.Velocity => speed,
                        _ => speed
                    };
                    _pixels[i] = Gradient(value, viewMode == FlowViewMode.Pressure);
                }
            }

            _texture.SetPixels32(_pixels);
            _texture.Apply(false, false);
        }

        private static Color32 Gradient(float t, bool diverging)
        {
            t = Mathf.Clamp01(t);
            if (diverging)
            {
                if (t < 0.5f)
                    return Color32.Lerp(new Color32(36, 93, 190, 255), new Color32(230, 235, 244, 255), t * 2f);
                return Color32.Lerp(new Color32(230, 235, 244, 255), new Color32(205, 54, 56, 255), (t - 0.5f) * 2f);
            }
            return Color32.Lerp(new Color32(27, 52, 90, 255), new Color32(235, 231, 96, 255), t);
        }
    }
}
