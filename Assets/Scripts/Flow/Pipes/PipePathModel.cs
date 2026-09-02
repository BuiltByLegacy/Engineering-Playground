using System;
using System.Collections.Generic;
using UnityEngine;

namespace EngineeringPlayground.Flow.Pipes
{
    public sealed class PipePathModel
    {
        private readonly List<Vector2> _points = new();
        private readonly List<Vector2> _preset = new();

        public IReadOnlyList<Vector2> Points => _points;
        public float Radius { get; private set; } = .09f;
        public float RouteLength { get; private set; }
        public float CurvatureCost { get; private set; }
        public event Action Changed;

        public void SetPreset(IEnumerable<Vector2> points, float radius)
        {
            _points.Clear(); _preset.Clear();
            foreach (var p in points) { var c = ClampPoint(p); _points.Add(c); _preset.Add(c); }
            Radius = Mathf.Clamp(radius, .045f, .18f);
            Recalculate(); Changed?.Invoke();
        }

        public void ResetToPreset()
        {
            _points.Clear(); _points.AddRange(_preset); Recalculate(); Changed?.Invoke();
        }

        public void MovePoint(int index, Vector2 position)
        {
            if (index <= 0 || index >= _points.Count - 1) return;
            var minX = _points[index - 1].x + .035f;
            var maxX = _points[index + 1].x - .035f;
            var p = ClampPoint(position); p.x = Mathf.Clamp(p.x, minX, maxX);
            _points[index] = p; Recalculate(); Changed?.Invoke();
        }

        public Vector2 Sample(float t)
        {
            if (_points.Count == 0) return new Vector2(t, .5f);
            if (_points.Count == 1) return _points[0];
            t = Mathf.Clamp01(t);
            var scaled = t * (_points.Count - 1);
            var i = Mathf.Min(Mathf.FloorToInt(scaled), _points.Count - 2);
            var u = scaled - i;
            var p0 = _points[Mathf.Max(0, i - 1)];
            var p1 = _points[i];
            var p2 = _points[i + 1];
            var p3 = _points[Mathf.Min(_points.Count - 1, i + 2)];

            // Cardinal spline with mild tension. It preserves the satisfying smooth route edit while
            // reducing Catmull-Rom overshoot that made early pipe presets balloon into giant arcs.
            const float tension = .35f;
            var m1 = (1f - tension) * .5f * (p2 - p0);
            var m2 = (1f - tension) * .5f * (p3 - p1);
            var u2 = u * u; var u3 = u2 * u;
            var h00 = 2f*u3 - 3f*u2 + 1f;
            var h10 = u3 - 2f*u2 + u;
            var h01 = -2f*u3 + 3f*u2;
            var h11 = u3 - u2;
            return h00*p1 + h10*m1 + h01*p2 + h11*m2;
        }

        private void Recalculate()
        {
            RouteLength = 0f; CurvatureCost = 0f;
            var prev = Sample(0); var prevDir = Vector2.right;
            const int samples = 128;
            for (var i=1;i<=samples;i++)
            {
                var p=Sample(i/(float)samples); var d=p-prev; RouteLength += d.magnitude;
                if(d.sqrMagnitude>1e-6f){var dir=d.normalized; CurvatureCost += Vector2.Angle(prevDir,dir)/180f; prevDir=dir;}
                prev=p;
            }
        }

        private static Vector2 ClampPoint(Vector2 p) => new(Mathf.Clamp(p.x,.02f,.98f), Mathf.Clamp(p.y,.08f,.92f));
    }

    public static class PipePathPresets
    {
        public static Vector2[] ForLevel(int level)
        {
            return level switch
            {
                1 => new[]{new Vector2(.02f,.5f),new Vector2(.34f,.5f),new Vector2(.66f,.5f),new Vector2(.98f,.5f)},
                // A plausible but improvable detour: the obstacle is immediately understandable and the
                // player has room to make the arc gentler instead of starting from an exaggerated mountain.
                2 => new[]{new Vector2(.02f,.5f),new Vector2(.28f,.58f),new Vector2(.50f,.66f),new Vector2(.72f,.58f),new Vector2(.98f,.5f)},
                3 => new[]{new Vector2(.02f,.5f),new Vector2(.27f,.66f),new Vector2(.50f,.70f),new Vector2(.73f,.58f),new Vector2(.98f,.5f)},
                4 => new[]{new Vector2(.02f,.5f),new Vector2(.30f,.5f),new Vector2(.50f,.58f),new Vector2(.70f,.5f),new Vector2(.98f,.5f)},
                5 => new[]{new Vector2(.02f,.5f),new Vector2(.28f,.55f),new Vector2(.50f,.66f),new Vector2(.72f,.55f),new Vector2(.98f,.5f)},
                _ => new[]{new Vector2(.02f,.5f),new Vector2(.22f,.64f),new Vector2(.42f,.40f),new Vector2(.63f,.66f),new Vector2(.80f,.48f),new Vector2(.98f,.5f)}
            };
        }
    }
}
