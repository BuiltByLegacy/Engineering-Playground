using EngineeringPlayground.Flow.Challenges;
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
            var challengeSession = root.AddComponent<FlowChallengeSession>();

            var canvasObject = new GameObject("Flow Lab Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var header = CreateText(canvasObject.transform, "Primary Header", new Vector2(0.04f, 0.945f), new Vector2(0.96f, 0.99f), 28, TextAnchor.MiddleLeft);
            var description = CreateText(canvasObject.transform, "Primary Description", new Vector2(0.04f, 0.82f), new Vector2(0.96f, 0.89f), 20, TextAnchor.UpperLeft);
            var result = CreateText(canvasObject.transform, "Primary Result", new Vector2(0.04f, 0.74f), new Vector2(0.96f, 0.82f), 20, TextAnchor.UpperLeft);

            var workspaceObject = new GameObject("Flow Workspace", typeof(RectTransform), typeof(RawImage), typeof(FlowFieldVisualizer), typeof(FlowTouchEditor));
            workspaceObject.transform.SetParent(canvasObject.transform, false);
            var workspaceRect = workspaceObject.GetComponent<RectTransform>();
            workspaceRect.anchorMin = new Vector2(0.04f, 0.16f);
            workspaceRect.anchorMax = new Vector2(0.96f, 0.735f);
            workspaceRect.offsetMin = Vector2.zero;
            workspaceRect.offsetMax = Vector2.zero;

            var rawImage = workspaceObject.GetComponent<RawImage>();
            rawImage.raycastTarget = true;

            var visualizer = workspaceObject.GetComponent<FlowFieldVisualizer>();
            visualizer.Configure(controller, rawImage);

            var editor = workspaceObject.GetComponent<FlowTouchEditor>();
            editor.Configure(controller, workspaceRect);

            var hud = root.AddComponent<FlowChallengeHud>();
            hud.Configure(challengeSession, header, description, result);

            var modeController = root.AddComponent<FlowLabModeController>();
            modeController.Configure(controller, challengeSession, hud, workspaceObject, header, description, result);

            CreateModeBar(canvasObject.transform, modeController);
            CreateNavigationBar(canvasObject.transform, modeController);
            CreateToolBar(canvasObject.transform, visualizer, editor);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(eventSystem);
        }

        private static void CreateModeBar(Transform parent, FlowLabModeController modeController)
        {
            var bar = CreateBar(parent, "Mode Bar", new Vector2(0.04f, 0.895f), new Vector2(0.96f, 0.94f), 10f);
            AddButton(bar.transform, "CHALLENGE", () => modeController.SetMode(FlowLabMode.Challenge));
            AddButton(bar.transform, "SANDBOX", () => modeController.SetMode(FlowLabMode.Sandbox));
            AddButton(bar.transform, "LEARN", () => modeController.SetMode(FlowLabMode.Learn));
            AddButton(bar.transform, "MODE", modeController.TogglePresentationMode);
        }

        private static void CreateNavigationBar(Transform parent, FlowLabModeController modeController)
        {
            var bar = CreateBar(parent, "Navigation Bar", new Vector2(0.04f, 0.09f), new Vector2(0.96f, 0.15f), 10f);
            AddButton(bar.transform, "PREV", modeController.Previous);
            AddButton(bar.transform, "RESET", modeController.Reset);
            AddButton(bar.transform, "RUN / PAUSE", modeController.ToggleRunning);
            AddButton(bar.transform, "SCORE", modeController.Score);
            AddButton(bar.transform, "NEXT", modeController.Next);
        }

        private static void CreateToolBar(Transform parent, FlowFieldVisualizer visualizer, FlowTouchEditor editor)
        {
            var bar = CreateBar(parent, "Tool Bar", new Vector2(0.04f, 0.015f), new Vector2(0.96f, 0.08f), 8f);
            AddButton(bar.transform, "DRAW", () => editor.SetTool(FlowEditorTool.Draw));
            AddButton(bar.transform, "ERASE", () => editor.SetTool(FlowEditorTool.Erase));
            AddButton(bar.transform, "PAN", () => editor.SetTool(FlowEditorTool.Pan));
            AddButton(bar.transform, "UNDO", editor.Undo);
            AddButton(bar.transform, "REDO", editor.Redo);
            AddButton(bar.transform, "FLOW", () => visualizer.SetViewMode(FlowViewMode.Flow));
            AddButton(bar.transform, "SPEED", () => visualizer.SetViewMode(FlowViewMode.Velocity));
            AddButton(bar.transform, "PRESS", () => visualizer.SetViewMode(FlowViewMode.Pressure));
            AddButton(bar.transform, "SWIRL", () => visualizer.SetViewMode(FlowViewMode.Vorticity));
        }

        private static GameObject CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float spacing)
        {
            var bar = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            bar.transform.SetParent(parent, false);
            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return bar;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
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
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
        }
    }
}
