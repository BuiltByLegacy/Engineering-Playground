using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Showcases
{
    public sealed class ShowcasePackagingOverlay : MonoBehaviour
    {
        private FlowShowcaseSession _session;
        private readonly List<GameObject> _generated = new();

        public void Configure(FlowShowcaseSession session)
        {
            if (_session != null)
                _session.ShowcaseChanged -= Refresh;

            _session = session;
            if (_session != null)
                _session.ShowcaseChanged += Refresh;

            Refresh();
        }

        private void OnDestroy()
        {
            if (_session != null)
                _session.ShowcaseChanged -= Refresh;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (visible)
                Refresh();
        }

        public void Refresh()
        {
            ClearGenerated();
            if (_session?.CurrentEntry == null)
                return;

            switch (_session.CurrentEntry.Id)
            {
                case "plumbing":
                    BuildPlumbing();
                    break;
                case "exhaust":
                    BuildExhaust();
                    break;
                case "hvac":
                    BuildHvac();
                    break;
                case "manifold":
                    BuildManifold();
                    break;
            }
        }

        private void BuildPlumbing()
        {
            AddOutline("House Envelope", new Vector2(0.08f, 0.10f), new Vector2(0.91f, 0.90f));
            AddPanel("Lower Floor", new Vector2(0.09f, 0.45f), new Vector2(0.90f, 0.465f), new Color(1f, 1f, 1f, 0.24f));
            AddPanel("Upper Chase", new Vector2(0.57f, 0.47f), new Vector2(0.60f, 0.86f), new Color(0.32f, 0.74f, 1f, 0.18f));
            AddLabel("SUPPLY", new Vector2(0.02f, 0.36f), new Vector2(0.16f, 0.50f));
            AddLabel("SINK", new Vector2(0.44f, 0.16f), new Vector2(0.56f, 0.28f));
            AddLabel("UPSTAIRS SHOWER", new Vector2(0.69f, 0.72f), new Vector2(0.94f, 0.86f));
            AddBadge("PACKAGING CONTEXT — NOT PIPE-NETWORK GEOMETRY");
        }

        private void BuildExhaust()
        {
            AddOutline("Vehicle Envelope", new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.82f));
            AddPanel("Floor Pan", new Vector2(0.20f, 0.66f), new Vector2(0.82f, 0.76f), new Color(1f, 1f, 1f, 0.14f));
            AddPanel("Drivetrain", new Vector2(0.40f, 0.17f), new Vector2(0.56f, 0.35f), new Color(1f, 0.65f, 0.22f, 0.16f));
            AddPanel("Rear Suspension", new Vector2(0.72f, 0.18f), new Vector2(0.84f, 0.34f), new Color(1f, 0.65f, 0.22f, 0.16f));
            AddLabel("ENGINE", new Vector2(0.02f, 0.38f), new Vector2(0.16f, 0.52f));
            AddLabel("CHASSIS / FLOOR", new Vector2(0.42f, 0.70f), new Vector2(0.62f, 0.82f));
            AddLabel("REAR EXIT", new Vector2(0.82f, 0.38f), new Vector2(0.98f, 0.52f));
            AddBadge("PACKAGING CONTEXT — NOT EXHAUST PERFORMANCE PREDICTION");
        }

        private void BuildHvac()
        {
            AddPanel("Handler", new Vector2(0.02f, 0.33f), new Vector2(0.15f, 0.67f), new Color(0.32f, 0.74f, 1f, 0.18f));
            AddOutline("Room A", new Vector2(0.68f, 0.67f), new Vector2(0.96f, 0.93f));
            AddOutline("Room B", new Vector2(0.68f, 0.37f), new Vector2(0.96f, 0.63f));
            AddOutline("Room C", new Vector2(0.68f, 0.07f), new Vector2(0.96f, 0.33f));
            AddLabel("AIR HANDLER", new Vector2(0.02f, 0.44f), new Vector2(0.16f, 0.56f));
            AddLabel("ROOM A", new Vector2(0.78f, 0.76f), new Vector2(0.92f, 0.87f));
            AddLabel("ROOM B", new Vector2(0.78f, 0.46f), new Vector2(0.92f, 0.57f));
            AddLabel("ROOM C", new Vector2(0.78f, 0.16f), new Vector2(0.92f, 0.27f));
            AddBadge("PACKAGING CONTEXT — VISUAL BALANCE PROXY, NOT COMMISSIONING");
        }

        private void BuildManifold()
        {
            AddOutline("Plenum Envelope", new Vector2(0.28f, 0.10f), new Vector2(0.91f, 0.90f));
            AddPanel("Separator 1", new Vector2(0.57f, 0.31f), new Vector2(0.88f, 0.325f), new Color(1f, 1f, 1f, 0.20f));
            AddPanel("Separator 2", new Vector2(0.57f, 0.49f), new Vector2(0.88f, 0.505f), new Color(1f, 1f, 1f, 0.20f));
            AddPanel("Separator 3", new Vector2(0.57f, 0.67f), new Vector2(0.88f, 0.685f), new Color(1f, 1f, 1f, 0.20f));
            AddLabel("INLET", new Vector2(0.02f, 0.43f), new Vector2(0.14f, 0.57f));
            AddLabel("OUTLET 1", new Vector2(0.84f, 0.76f), new Vector2(0.99f, 0.88f));
            AddLabel("OUTLET 2", new Vector2(0.84f, 0.58f), new Vector2(0.99f, 0.70f));
            AddLabel("OUTLET 3", new Vector2(0.84f, 0.40f), new Vector2(0.99f, 0.52f));
            AddLabel("OUTLET 4", new Vector2(0.84f, 0.22f), new Vector2(0.99f, 0.34f));
            AddBadge("PACKAGING CONTEXT — FOUR-LANE PROXY, SINGLE SOLVER OUTLET");
        }

        private void AddOutline(string name, Vector2 min, Vector2 max)
        {
            const float t = 0.006f;
            var color = new Color(1f, 1f, 1f, 0.22f);
            AddPanel(name + " Top", new Vector2(min.x, max.y - t), new Vector2(max.x, max.y), color);
            AddPanel(name + " Bottom", new Vector2(min.x, min.y), new Vector2(max.x, min.y + t), color);
            AddPanel(name + " Left", new Vector2(min.x, min.y), new Vector2(min.x + t, max.y), color);
            AddPanel(name + " Right", new Vector2(max.x - t, min.y), new Vector2(max.x, max.y), color);
        }

        private void AddBadge(string text)
        {
            AddLabel(text, new Vector2(0.20f, 0.01f), new Vector2(0.80f, 0.08f), 13, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.70f));
        }

        private void AddPanel(string name, Vector2 min, Vector2 max, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            _generated.Add(obj);
        }

        private void AddLabel(
            string text,
            Vector2 min,
            Vector2 max,
            int fontSize = 14,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            Color? color = null)
        {
            var obj = new GameObject(text, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var label = obj.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.text = text;
            label.color = color ?? new Color(1f, 1f, 1f, 0.82f);
            label.raycastTarget = false;
            _generated.Add(obj);
        }

        private void ClearGenerated()
        {
            for (var i = _generated.Count - 1; i >= 0; i--)
            {
                if (_generated[i] != null)
                    Destroy(_generated[i]);
            }
            _generated.Clear();
        }
    }
}
