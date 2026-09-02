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
        private static readonly Color Background = new Color32(5, 10, 18, 255);
        private static readonly Color Surface = new Color32(13, 24, 38, 242);
        private static readonly Color SurfaceStrong = new Color32(18, 34, 51, 255);
        private static readonly Color Accent = new Color32(55, 220, 184, 255);
        private static readonly Color TextPrimary = new Color32(246, 250, 252, 255);
        private static readonly Color TextMuted = new Color32(156, 178, 194, 255);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildIfNeeded()
        {
            if (Object.FindFirstObjectByType<FlowLabRuntimeController>() != null) return;
            EnsureEventSystem();

            var root = new GameObject("Engineering Playground"); Object.DontDestroyOnLoad(root);
            var controller = root.AddComponent<FlowLabRuntimeController>();
            var challengeSession = root.AddComponent<FlowChallengeSession>();
            var showcaseSession = root.AddComponent<FlowShowcaseSession>();

            var canvasObject = new GameObject("Game Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Image));
            canvasObject.transform.SetParent(root.transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.GetComponent<Image>().color = Background; canvasObject.GetComponent<Image>().raycastTarget = false;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920f, 1080f); scaler.matchWidthOrHeight = .5f;

            // Thin in-level HUD. Mode selection is intentionally not persistent gameplay chrome.
            var topCard = CreatePanel(canvasObject.transform, "Level HUD", new Vector2(.025f,.895f), new Vector2(.975f,.975f), Surface);
            var header = CreateText(topCard.transform, "Header", new Vector2(.025f,.12f), new Vector2(.77f,.88f), 27, TextAnchor.MiddleLeft, TextPrimary, FontStyle.Bold);
            var description = CreateText(topCard.transform, "Objective", new Vector2(.78f,.12f), new Vector2(.975f,.88f), 15, TextAnchor.MiddleRight, TextMuted);

            // Nearly edge-to-edge simulation: the game, not the menus, owns the screen.
            var workspaceFrame = CreatePanel(canvasObject.transform, "Simulation Frame", new Vector2(.025f,.145f), new Vector2(.975f,.88f), SurfaceStrong);
            var workspaceObject = new GameObject("Flow Workspace", typeof(RectTransform), typeof(RawImage), typeof(FlowFieldVisualizer), typeof(FlowTouchEditor));
            workspaceObject.transform.SetParent(workspaceFrame.transform, false);
            var workspaceRect = workspaceObject.GetComponent<RectTransform>(); workspaceRect.anchorMin = new Vector2(.006f,.008f); workspaceRect.anchorMax = new Vector2(.994f,.992f); workspaceRect.offsetMin = workspaceRect.offsetMax = Vector2.zero;
            var rawImage = workspaceObject.GetComponent<RawImage>(); rawImage.raycastTarget = true;
            var visualizer = workspaceObject.GetComponent<FlowFieldVisualizer>(); visualizer.Configure(controller, rawImage);
            var editor = workspaceObject.GetComponent<FlowTouchEditor>(); editor.Configure(controller, workspaceRect);

            var tracerObject = new GameObject("Flow Tracers", typeof(RectTransform), typeof(CanvasRenderer), typeof(FlowTracerOverlay)); tracerObject.transform.SetParent(workspaceObject.transform, false);
            var tracerRect = tracerObject.GetComponent<RectTransform>(); tracerRect.anchorMin = Vector2.zero; tracerRect.anchorMax = Vector2.one; tracerRect.offsetMin = tracerRect.offsetMax = Vector2.zero; tracerObject.GetComponent<FlowTracerOverlay>().Configure(controller);

            var overlayObject = new GameObject("Showcase Packaging Overlay", typeof(RectTransform), typeof(ShowcasePackagingOverlay)); overlayObject.transform.SetParent(workspaceObject.transform, false);
            var overlayRect = overlayObject.GetComponent<RectTransform>(); overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one; overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
            var showcaseOverlay = overlayObject.GetComponent<ShowcasePackagingOverlay>(); showcaseOverlay.Configure(showcaseSession); showcaseOverlay.SetVisible(false);

            // Small contextual coaching pill rather than a full-width rectangle over the simulation.
            var hintPanel = CreatePanel(workspaceFrame.transform, "Coach Hint", new Vector2(.035f,.89f), new Vector2(.57f,.97f), new Color32(7,17,27,205));
            var hint = CreateText(hintPanel.transform, "Hint", new Vector2(.035f,.08f), new Vector2(.965f,.92f), 15, TextAnchor.MiddleLeft, TextPrimary, FontStyle.Bold);
            var inlet = CreateText(workspaceFrame.transform, "Inlet", new Vector2(.012f,.44f), new Vector2(.12f,.56f), 15, TextAnchor.MiddleLeft, Accent, FontStyle.Bold); inlet.text = "IN  →";
            var outlet = CreateText(workspaceFrame.transform, "Outlet", new Vector2(.86f,.44f), new Vector2(.988f,.56f), 15, TextAnchor.MiddleRight, Accent, FontStyle.Bold); outlet.text = "→  OUT";

            var resultCard = CreatePanel(canvasObject.transform, "Result Card", new Vector2(.19f,.21f), new Vector2(.81f,.49f), new Color32(10,21,32,250));
            var result = CreateText(resultCard.transform, "Result", new Vector2(.045f,.08f), new Vector2(.955f,.92f), 21, TextAnchor.MiddleLeft, TextPrimary); resultCard.SetActive(false);
            var reference = CreateText(canvasObject.transform, "Reference Estimate", new Vector2(.04f,.18f), new Vector2(.96f,.78f), 17, TextAnchor.UpperLeft, TextPrimary); reference.gameObject.SetActive(false);

            var hud = root.AddComponent<FlowChallengeHud>(); hud.Configure(challengeSession, header, description, result);
            var modeController = root.AddComponent<FlowLabModeController>(); modeController.Configure(controller, challengeSession, hud, showcaseSession, showcaseOverlay, workspaceObject, header, description, result, reference);

            var toolRail = CreateBar(canvasObject.transform, "Tool Rail", new Vector2(.025f,.035f), new Vector2(.49f,.125f), 10f);
            AddButton(toolRail.transform, "DRAW", () => editor.SetTool(FlowEditorTool.Draw));
            AddButton(toolRail.transform, "ERASE", () => editor.SetTool(FlowEditorTool.Erase));
            AddButton(toolRail.transform, "UNDO", editor.Undo);
            AddButton(toolRail.transform, "VIEW", () => visualizer.CycleViewMode());

            var actionRail = CreateBar(canvasObject.transform, "Action Rail", new Vector2(.64f,.035f), new Vector2(.975f,.125f), 10f);
            var resetButton = AddIconButton(actionRail.transform, "↻", modeController.Reset);
            var primaryButton = AddPrimaryButton(actionRail.transform, "RUN FLOW  ▶", () => { });
            var primaryLabel = primaryButton.GetComponentInChildren<Text>();
            var nextButton = AddButton(actionRail.transform, "NEXT", () => { }); nextButton.gameObject.SetActive(false);
            var prevButton = AddButton(actionRail.transform, "PREV", modeController.Previous); prevButton.gameObject.SetActive(false);

            var play = root.AddComponent<FlowChallengePlayController>(); play.Configure(controller, challengeSession);
            var experience = root.AddComponent<FlowChallengeExperience>(); experience.Configure(play, challengeSession, editor, toolRail, resultCard, hint, primaryButton, primaryLabel, nextButton);

            primaryButton.onClick.RemoveAllListeners(); primaryButton.onClick.AddListener(() => { if (modeController.Mode == FlowLabMode.Challenge) experience.PrimaryAction(); else modeController.ToggleRunning(); });
            nextButton.onClick.RemoveAllListeners(); nextButton.onClick.AddListener(() => { if (modeController.Mode == FlowLabMode.Challenge) experience.Next(); else modeController.Next(); });
            resetButton.onClick.RemoveAllListeners(); resetButton.onClick.AddListener(() => { if (modeController.Mode == FlowLabMode.Challenge) play.Retry(); else modeController.Reset(); });
            prevButton.onClick.RemoveAllListeners(); prevButton.onClick.AddListener(() => { if (modeController.Mode == FlowLabMode.Challenge) play.Previous(); else modeController.Previous(); });

            // Temporary keyboard-accessible mode switching remains available for development without occupying phone UI.
            root.AddComponent<FlowLabModeHotkeys>().Configure(modeController, experience);
        }

        private static void EnsureEventSystem(){if(EventSystem.current!=null)return;var eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));Object.DontDestroyOnLoad(eventSystem);}
        private static GameObject CreatePanel(Transform parent,string name,Vector2 min,Vector2 max,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;go.GetComponent<Image>().color=color;return go;}
        private static GameObject CreateBar(Transform parent,string name,Vector2 min,Vector2 max,float spacing){var go=new GameObject(name,typeof(RectTransform),typeof(HorizontalLayoutGroup));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var layout=go.GetComponent<HorizontalLayoutGroup>();layout.spacing=spacing;layout.childForceExpandWidth=true;layout.childForceExpandHeight=true;return go;}
        private static Text CreateText(Transform parent,string name,Vector2 min,Vector2 max,int size,TextAnchor align,Color color,FontStyle style=FontStyle.Normal){var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=align;text.color=color;text.fontStyle=style;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;}
        private static Button AddButton(Transform parent,string label,UnityEngine.Events.UnityAction action)=>AddStyledButton(parent,label,action,Surface,TextPrimary,16);
        private static Button AddIconButton(Transform parent,string label,UnityEngine.Events.UnityAction action)=>AddStyledButton(parent,label,action,SurfaceStrong,TextMuted,24);
        private static Button AddPrimaryButton(Transform parent,string label,UnityEngine.Events.UnityAction action)=>AddStyledButton(parent,label,action,Accent,Background,18);
        private static Button AddStyledButton(Transform parent,string label,UnityEngine.Events.UnityAction action,Color bg,Color fg,int fontSize){var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);go.GetComponent<Image>().color=bg;var button=go.GetComponent<Button>();button.onClick.AddListener(action);var text=CreateText(go.transform,"Label",Vector2.zero,Vector2.one,fontSize,TextAnchor.MiddleCenter,fg,FontStyle.Bold);text.text=label;return button;}
    }

    // Development-only navigation replacement while the production Flow Lab home screen is built.
    public sealed class FlowLabModeHotkeys : MonoBehaviour
    {
        private FlowLabModeController _m; private FlowChallengeExperience _e;
        public void Configure(FlowLabModeController mode,FlowChallengeExperience experience){_m=mode;_e=experience;}
        private void Update(){if(UnityEngine.InputSystem.Keyboard.current==null)return;var k=UnityEngine.InputSystem.Keyboard.current;if(k.digit1Key.wasPressedThisFrame){_m.SetMode(FlowLabMode.Challenge);_e.SetChallengeModeActive(true);}else if(k.digit2Key.wasPressedThisFrame){_e.SetChallengeModeActive(false);_m.SetMode(FlowLabMode.Sandbox);}else if(k.digit3Key.wasPressedThisFrame){_e.SetChallengeModeActive(false);_m.SetMode(FlowLabMode.Learn);}else if(k.digit4Key.wasPressedThisFrame){_e.SetChallengeModeActive(false);_m.SetMode(FlowLabMode.Showcase);}}
    }
}
