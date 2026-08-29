using EngineeringPlayground.Flow.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EngineeringPlayground.App
{
    public static class EngineeringPlaygroundBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildIfNeeded()
        {
            if (Object.FindFirstObjectByType<FlowLabRuntimeController>() != null)
                return;

            EnsureEventSystem();

            var root = new GameObject("Engineering Playground");
            Object.DontDestroyOnLoad(root);
            var controller = root.AddComponent<FlowLabRuntimeController>();

            var canvasObject = new GameObject("Flow Lab Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var workspaceObject = new GameObject("Flow Workspace", typeof(RectTransform), typeof(RawImage), typeof(FlowFieldVisualizer), typeof(FlowTouchEditor));
            workspaceObject.transform.SetParent(canvasObject.transform, false);
            var rect = workspaceObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.04f, 0.10f);
            rect.anchorMax = new Vector2(0.96f, 0.90f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var rawImage = workspaceObject.GetComponent<RawImage>();
            rawImage.raycastTarget = true;

            var visualizer = workspaceObject.GetComponent<FlowFieldVisualizer>();
            visualizer.Configure(controller, rawImage);

            var editor = workspaceObject.GetComponent<FlowTouchEditor>();
            editor.Configure(controller, rect);

            CreateStatusText(canvasObject.transform, controller);
            CreateToolbar(canvasObject.transform, controller, visualizer, editor);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(eventSystem);
        }

        private static void CreateStatusText(Transform parent, FlowLabRuntimeController controller)
        {
            var textObject = new GameObject("Status", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.04f, 0.92f);
            rect.anchorMax = new Vector2(0.96f, 0.99f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = $"FLOW LAB — Unity production runtime   |   {controller.GetGuardrailStatus()}";
        }

        private static void CreateToolbar(Transform parent, FlowLabRuntimeController controller, FlowFieldVisualizer visualizer, FlowTouchEditor editor)
        {
            var bar = new GameObject("Toolbar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            bar.transform.SetParent(parent, false);
            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.04f, 0.01f);
            rect.anchorMax = new Vector2(0.96f, 0.085f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            AddButton(bar.transform, "DRAW", () => editor.SetTool(FlowEditorTool.Draw));
            AddButton(bar.transform, "ERASE", () => editor.SetTool(FlowEditorTool.Erase));
            AddButton(bar.transform, "PAN", () => editor.SetTool(FlowEditorTool.Pan));
            AddButton(bar.transform, "UNDO", editor.Undo);
            AddButton(bar.transform, "REDO", editor.Redo);
            AddButton(bar.transform, "FLOW", () => visualizer.SetViewMode(FlowViewMode.Flow));
            AddButton(bar.transform, "SPEED", () => visualizer.SetViewMode(FlowViewMode.Velocity));
            AddButton(bar.transform, "PRESS", () => visualizer.SetViewMode(FlowViewMode.Pressure));
            AddButton(bar.transform, "SWIRL", () => visualizer.SetViewMode(FlowViewMode.Vorticity));
            AddButton(bar.transform, "CLEAR", controller.ClearGeometry);
            AddButton(bar.transform, "RUN / PAUSE", controller.ToggleRunning);
        }

        private static void AddButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
        }
    }
}
