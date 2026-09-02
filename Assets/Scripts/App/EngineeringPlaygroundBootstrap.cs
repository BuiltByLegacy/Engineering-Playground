using EngineeringPlayground.Flow.Challenges;
using EngineeringPlayground.Flow.Runtime;
using EngineeringPlayground.Flow.Showcases;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EngineeringPlayground.App
{
    public static class EngineeringPlaygroundBootstrap
    {
        private static readonly Color Background = new Color32(8, 14, 24, 255);
        private static readonly Color Surface = new Color32(18, 29, 44, 238);
        private static readonly Color SurfaceStrong = new Color32(25, 40, 59, 255);
        private static readonly Color Accent = new Color32(46, 204, 171, 255);
        private static readonly Color TextPrimary = new Color32(241, 247, 250, 255);
        private static readonly Color TextMuted = new Color32(166, 183, 196, 255);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildIfNeeded()
        {
            if (Object.FindFirstObjectByType<FlowLabRuntimeController>() != null) return;
            EnsureEventSystem();

            var root = new GameObject("Engineering Playground");
            Object.DontDestroyOnLoad(root);
            var controller = root.AddComponent<FlowLabRuntimeController>();
            var challengeSession = root.AddComponent<FlowChallengeSession>();
            var showcaseSession = root.AddComponent<FlowShowcaseSession>();

            var canvasObject = new GameObject("Game Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Image));
            canvasObject.transform.SetParent(root.transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.GetComponent<Image>().color = Background;
            canvasObject.GetComponent<Image>().raycastTarget = false;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var topCard = CreatePanel(canvasObject.transform, "Objective Card", new Vector2(.025f,.82f), new Vector2(.975f,.975f), Surface);
            var header = CreateText(topCard.transform, "Header", new Vector2(.025f,.56f), new Vector2(.72f,.94f), 30, TextAnchor.MiddleLeft, TextPrimary, FontStyle.Bold);
            var description = CreateText(topCard.transform, "Objective", new Vector2(.025f,.08f), new Vector2(.72f,.58f), 18, TextAnchor.UpperLeft, TextMuted);
            var modeBar = CreateBar(topCard.transform, "Mode Bar", new Vector2(.73f,.18f), new Vector2(.98f,.82f), 8f);

            var workspaceFrame = CreatePanel(canvasObject.transform, "Simulation Frame", new Vector2(.025f,.155f), new Vector2(.975f,.805f), SurfaceStrong);
            var workspaceObject = new GameObject("Flow Workspace", typeof(RectTransform), typeof(RawImage), typeof(FlowFieldVisualizer), typeof(FlowTouchEditor));
            workspaceObject.transform.SetParent(workspaceFrame.transform, false);
            var workspaceRect = workspaceObject.GetComponent<RectTransform>();
            workspaceRect.anchorMin = new Vector2(.008f,.012f);
            workspaceRect.anchorMax = new Vector2(.992f,.988f);
            workspaceRect.offsetMin = workspaceRect.offsetMax = Vector2.zero;
            var rawImage = workspaceObject.GetComponent<RawImage>(); rawImage.raycastTarget = true;
            var visualizer = workspaceObject.GetComponent<FlowFieldVisualizer>(); visualizer.Configure(controller, rawImage);
            var editor = workspaceObject.GetComponent<FlowTouchEditor>(); editor.Configure(controller, workspaceRect);

            var overlayObject = new GameObject("Showcase Packaging Overlay", typeof(RectTransform), typeof(ShowcasePackagingOverlay));
            overlayObject.transform.SetParent(workspaceObject.transform, false);
            var overlayRect = overlayObject.GetComponent<RectTransform>(); overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one; overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
            var showcaseOverlay = overlayObject.GetComponent<ShowcasePackagingOverlay>(); showcaseOverlay.Configure(showcaseSession); showcaseOverlay.SetVisible(false);

            var resultCard = CreatePanel(canvasObject.transform, "Result Card", new Vector2(.18f,.22f), new Vector2(.82f,.48f), new Color32(12,22,34,248));
            var result = CreateText(resultCard.transform, "Result", new Vector2(.04f,.08f), new Vector2(.96f,.92f), 22, TextAnchor.MiddleLeft, TextPrimary);
            resultCard.SetActive(false);
            var reference = CreateText(canvasObject.transform, "Reference Estimate", new Vector2(.04f,.18f), new Vector2(.96f,.78f), 17, TextAnchor.UpperLeft, TextPrimary); reference.gameObject.SetActive(false);

            var hud = root.AddComponent<FlowChallengeHud>(); hud.Configure(challengeSession, header, description, result);
            var modeController = root.AddComponent<FlowLabModeController>();
            modeController.Configure(controller, challengeSession, hud, showcaseSession, showcaseOverlay, workspaceObject, header, description, result, reference);

            AddModeButton(modeBar.transform, "PLAY", () => modeController.SetMode(FlowLabMode.Challenge));
            AddModeButton(modeBar.transform, "FREE", () => modeController.SetMode(FlowLabMode.Sandbox));
            AddModeButton(modeBar.transform, "LEARN", () => modeController.SetMode(FlowLabMode.Learn));
            AddModeButton(modeBar.transform, "REAL", () => modeController.SetMode(FlowLabMode.Showcase));

            var toolRail = CreateBar(canvasObject.transform, "Tool Rail", new Vector2(.025f,.035f), new Vector2(.43f,.135f), 8f);
            AddButton(toolRail.transform, "DRAW", () => editor.SetTool(FlowEditorTool.Draw));
            AddButton(toolRail.transform, "ERASE", () => editor.SetTool(FlowEditorTool.Erase));
            AddButton(toolRail.transform, "UNDO", editor.Undo);
            AddButton(toolRail.transform, "VIEW", () => visualizer.CycleViewMode());

            var actionRail = CreateBar(canvasObject.transform, "Action Rail", new Vector2(.57f,.035f), new Vector2(.975f,.135f), 8f);
            AddButton(actionRail.transform, "RESET", modeController.Reset);
            AddButton(actionRail.transform, "PREV", modeController.Previous);
            AddPrimaryButton(actionRail.transform, "RUN FLOW", modeController.ToggleRunning);
            AddButton(actionRail.transform, "SCORE", () => { resultCard.SetActive(true); modeController.Score(); });
            AddButton(actionRail.transform, "NEXT", modeController.Next);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(eventSystem);
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            go.GetComponent<Image>().color=color; return go;
        }

        private static GameObject CreateBar(Transform parent, string name, Vector2 min, Vector2 max, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var layout=go.GetComponent<HorizontalLayoutGroup>(); layout.spacing=spacing; layout.childForceExpandWidth=true; layout.childForceExpandHeight=true; return go;
        }

        private static Text CreateText(Transform parent,string name,Vector2 min,Vector2 max,int size,TextAnchor align,Color color,FontStyle style=FontStyle.Normal)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false); var rect=go.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var text=go.GetComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=size; text.alignment=align; text.color=color; text.fontStyle=style; text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Truncate; return text;
        }

        private static void AddModeButton(Transform parent,string label,UnityEngine.Events.UnityAction action) => AddStyledButton(parent,label,action,SurfaceStrong,TextMuted,15);
        private static void AddButton(Transform parent,string label,UnityEngine.Events.UnityAction action) => AddStyledButton(parent,label,action,Surface,TextPrimary,16);
        private static void AddPrimaryButton(Transform parent,string label,UnityEngine.Events.UnityAction action) => AddStyledButton(parent,label,action,Accent,Background,18);
        private static void AddStyledButton(Transform parent,string label,UnityEngine.Events.UnityAction action,Color bg,Color fg,int fontSize)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(parent,false); go.GetComponent<Image>().color=bg; go.GetComponent<Button>().onClick.AddListener(action);
            var text=CreateText(go.transform,"Label",Vector2.zero,Vector2.one,fontSize,TextAnchor.MiddleCenter,fg,FontStyle.Bold); text.text=label;
        }
    }
}
