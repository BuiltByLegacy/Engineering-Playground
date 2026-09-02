using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FlowTracerOverlay : MaskableGraphic
    {
        private struct Tracer { public Vector2 Position; public float Life; public float Phase; }
        private readonly List<Tracer> _tracers = new();
        private FlowLabRuntimeController _controller;
        private int _targetCount;

        public void Configure(FlowLabRuntimeController controller)
        {
            _controller = controller;
            raycastTarget = false;
            _targetCount = Screen.width <= 1200 ? 72 : 128;
            Seed();
        }

        private void Update()
        {
            if (_controller?.Solver == null || !_controller.Running) return;
            if (_tracers.Count == 0) Seed();

            var solver = _controller.Solver;
            var dt = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
            for (var i = 0; i < _tracers.Count; i++)
            {
                var t = _tracers[i];
                var x = Mathf.Clamp(Mathf.RoundToInt(t.Position.x * (solver.Width - 1)), 1, solver.Width - 2);
                var y = Mathf.Clamp(Mathf.RoundToInt(t.Position.y * (solver.Height - 1)), 1, solver.Height - 2);
                var idx = y * solver.Width + x;
                var vx = (float)solver.VelocityX[idx];
                var vy = (float)solver.VelocityY[idx];

                t.Position += new Vector2(vx, vy) * (dt * 8.5f);
                t.Life -= dt;
                if (t.Life <= 0f || t.Position.x > 1.02f || t.Position.x < -0.02f || t.Position.y > 1.02f || t.Position.y < -0.02f || solver.Solid[idx])
                    t = Respawn(i);
                _tracers[i] = t;
            }
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_controller?.Solver == null || !_controller.Running) return;
            var rect = rectTransform.rect;
            for (var i = 0; i < _tracers.Count; i++)
            {
                var t = _tracers[i];
                var center = new Vector2(rect.xMin + t.Position.x * rect.width, rect.yMin + t.Position.y * rect.height);
                var pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 5f + t.Phase);
                var size = Mathf.Max(2.5f, rect.height * 0.0065f) * pulse;
                var alpha = (byte)Mathf.RoundToInt(150 + 90 * pulse);
                var c = new Color32(177, 255, 237, alpha);
                var start = vh.currentVertCount;
                vh.AddVert(center + new Vector2(-size, -size * .45f), c, Vector2.zero);
                vh.AddVert(center + new Vector2(-size,  size * .45f), c, Vector2.zero);
                vh.AddVert(center + new Vector2( size,  size * .45f), c, Vector2.zero);
                vh.AddVert(center + new Vector2( size, -size * .45f), c, Vector2.zero);
                vh.AddTriangle(start, start + 1, start + 2);
                vh.AddTriangle(start, start + 2, start + 3);
            }
        }

        private void Seed()
        {
            _tracers.Clear();
            for (var i = 0; i < _targetCount; i++)
                _tracers.Add(Respawn(i, true));
            SetVerticesDirty();
        }

        private Tracer Respawn(int index, bool distributed = false)
        {
            var lane = Mathf.Repeat(index * 0.6180339f, 1f);
            return new Tracer
            {
                Position = new Vector2(distributed ? Mathf.Repeat(index / (float)Mathf.Max(1, _targetCount) + lane * .15f, .95f) : 0.015f, 0.08f + lane * 0.84f),
                Life = 3.5f + lane * 4f,
                Phase = lane * 6.28318f
            };
        }
    }
}
