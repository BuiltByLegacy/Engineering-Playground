using System;
using System.Collections.Generic;
using UnityEngine;

namespace EngineeringPlayground.Flow.Pipes
{
    public sealed class PipePathModel
    {
        private readonly List<Vector2> _points = new();
        private readonly List<Vector2> _preset = new();
        private readonly List<float> _radii = new();
        private readonly List<float> _presetRadii = new();

        public IReadOnlyList<Vector2> Points => _points;
        public IReadOnlyList<float> RadiusProfile => _radii;
        public float Radius { get; private set; } = .09f;
        public float RouteLength { get; private set; }
        public float CurvatureCost { get; private set; }
        public float MinimumBendRadius { get; private set; } = 999f;
        public float MinimumRadius => _radii.Count == 0 ? Radius : Mathf.Min(_radii.ToArray());
        public float MaximumRadius => _radii.Count == 0 ? Radius : Mathf.Max(_radii.ToArray());
        public float RequiredMinimumBendRadius => Mathf.Max(.045f, MaximumRadius * .9f);
        public bool MeetsBendRadiusGuardrail => MinimumBendRadius + 1e-4f >= RequiredMinimumBendRadius;
        public event Action Changed;

        public void SetPreset(IEnumerable<Vector2> points, float radius)
        {
            _points.Clear(); _preset.Clear(); _radii.Clear(); _presetRadii.Clear();
            Radius = Mathf.Clamp(radius, .045f, .18f);
            foreach (var p in points)
            {
                var c = ClampPoint(p); _points.Add(c); _preset.Add(c); _radii.Add(Radius); _presetRadii.Add(Radius);
            }
            Recalculate(); Changed?.Invoke();
        }

        public void SetPreset(IEnumerable<Vector2> points, IEnumerable<float> radii)
        {
            _points.Clear(); _preset.Clear(); _radii.Clear(); _presetRadii.Clear();
            using var radiusEnumerator = radii.GetEnumerator();
            foreach (var p in points)
            {
                var c=ClampPoint(p);_points.Add(c);_preset.Add(c);
                var r=radiusEnumerator.MoveNext()?Mathf.Clamp(radiusEnumerator.Current,.045f,.18f):.09f;
                _radii.Add(r);_presetRadii.Add(r);
            }
            Radius=_radii.Count==0?.09f:_radii[0];
            Recalculate();Changed?.Invoke();
        }

        public void ResetToPreset()
        {
            _points.Clear(); _points.AddRange(_preset); _radii.Clear(); _radii.AddRange(_presetRadii); Recalculate(); Changed?.Invoke();
        }

        public void MovePoint(int index, Vector2 position)
        {
            if (index <= 0 || index >= _points.Count - 1) return;
            var minX = _points[index - 1].x + .035f;
            var maxX = _points[index + 1].x - .035f;
            var target = ClampPoint(position); target.x = Mathf.Clamp(target.x, minX, maxX);
            var original = _points[index]; var originalMinRadius = MinimumBendRadius;
            _points[index] = target; Recalculate();
            if (IsMoveAcceptable(originalMinRadius)) { Changed?.Invoke(); return; }
            var best = original; var low = 0f; var high = 1f;
            for (var i=0;i<10;i++)
            {
                var mid = (low + high) * .5f; _points[index] = Vector2.Lerp(original, target, mid); Recalculate();
                if (IsMoveAcceptable(originalMinRadius)) { best = _points[index]; low = mid; } else high = mid;
            }
            _points[index] = best; Recalculate(); Changed?.Invoke();
        }

        public void SetRadiusAtPoint(int index,float radius)
        {
            if(index<0||index>=_radii.Count)return;
            _radii[index]=Mathf.Clamp(radius,.045f,.18f);
            if(index==0)Radius=_radii[0];
            Recalculate();Changed?.Invoke();
        }

        public float RadiusAt(float t)
        {
            if(_radii.Count==0)return Radius;
            if(_radii.Count==1)return _radii[0];
            t=Mathf.Clamp01(t);var scaled=t*(_radii.Count-1);var i=Mathf.Min(Mathf.FloorToInt(scaled),_radii.Count-2);var u=scaled-i;
            u=u*u*(3f-2f*u);
            return Mathf.Lerp(_radii[i],_radii[i+1],u);
        }

        private bool IsMoveAcceptable(float originalMinRadius)
        {
            if (MeetsBendRadiusGuardrail) return true;
            return MinimumBendRadius + 1e-4f >= originalMinRadius;
        }

        public Vector2 Sample(float t)
        {
            if (_points.Count == 0) return new Vector2(t, .5f);
            if (_points.Count == 1) return _points[0];
            t = Mathf.Clamp01(t); var scaled = t * (_points.Count - 1); var i = Mathf.Min(Mathf.FloorToInt(scaled), _points.Count - 2); var u = scaled - i;
            var p0 = _points[Mathf.Max(0, i - 1)]; var p1 = _points[i]; var p2 = _points[i + 1]; var p3 = _points[Mathf.Min(_points.Count - 1, i + 2)];
            const float tension = .35f; var m1 = (1f - tension) * .5f * (p2 - p0); var m2 = (1f - tension) * .5f * (p3 - p1); var u2 = u * u; var u3 = u2 * u;
            return (2f*u3 - 3f*u2 + 1f)*p1 + (u3 - 2f*u2 + u)*m1 + (-2f*u3 + 3f*u2)*p2 + (u3 - u2)*m2;
        }

        private void Recalculate()
        {
            RouteLength = 0f; CurvatureCost = 0f; MinimumBendRadius = 999f; const int samples = 192;
            var previous = Sample(0f); var beforePrevious = previous; var previousDir = Vector2.right;
            for (var i=1;i<=samples;i++)
            {
                var p = Sample(i/(float)samples); var d = p - previous; RouteLength += d.magnitude;
                if (d.sqrMagnitude > 1e-8f) { var dir = d.normalized; CurvatureCost += Vector2.Angle(previousDir, dir) / 180f; previousDir = dir; }
                if (i >= 2) { var r = Circumradius(beforePrevious, previous, p); if (r < MinimumBendRadius) MinimumBendRadius = r; }
                beforePrevious = previous; previous = p;
            }
        }

        private static float Circumradius(Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = Vector2.Distance(a,b); var bc = Vector2.Distance(b,c); var ca = Vector2.Distance(c,a); var twiceArea = Mathf.Abs((b.x-a.x)*(c.y-a.y) - (b.y-a.y)*(c.x-a.x));
            if (twiceArea < 1e-7f || ab < 1e-6f || bc < 1e-6f || ca < 1e-6f) return 999f;
            return ab * bc * ca / (2f * twiceArea);
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
                2 => new[]{new Vector2(.02f,.5f),new Vector2(.28f,.58f),new Vector2(.50f,.66f),new Vector2(.72f,.58f),new Vector2(.98f,.5f)},
                3 => new[]{new Vector2(.02f,.5f),new Vector2(.27f,.66f),new Vector2(.50f,.70f),new Vector2(.73f,.58f),new Vector2(.98f,.5f)},
                4 => new[]{new Vector2(.02f,.5f),new Vector2(.28f,.5f),new Vector2(.50f,.5f),new Vector2(.72f,.5f),new Vector2(.98f,.5f)},
                5 => new[]{new Vector2(.02f,.5f),new Vector2(.27f,.5f),new Vector2(.50f,.5f),new Vector2(.73f,.5f),new Vector2(.98f,.5f)},
                _ => new[]{new Vector2(.02f,.5f),new Vector2(.22f,.64f),new Vector2(.42f,.40f),new Vector2(.63f,.66f),new Vector2(.80f,.48f),new Vector2(.98f,.5f)}
            };
        }

        public static float[] RadiusProfileForLevel(int level)
        {
            return level switch
            {
                4 => new[]{.10f,.10f,.055f,.10f,.10f},
                5 => new[]{.065f,.085f,.125f,.085f,.065f},
                2 => new[]{.075f,.075f,.075f,.075f,.075f},
                _ => null
            };
        }
    }
}
