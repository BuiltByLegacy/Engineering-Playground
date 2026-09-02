using EngineeringPlayground.Flow.Challenges;
using EngineeringPlayground.Flow.Runtime;
using EngineeringPlayground.Flow.Showcases;
using EngineeringPlayground.UI;
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
            if(Object.FindFirstObjectByType<FlowLabRuntimeController>()!=null)return;
            EnsureEventSystem();

            var root=new GameObject("Engineering Playground");Object.DontDestroyOnLoad(root);
            var controller=root.AddComponent<FlowLabRuntimeController>();
            var challengeSession=root.AddComponent<FlowChallengeSession>();
            var showcaseSession=root.AddComponent<FlowShowcaseSession>();

            var canvasObject=new GameObject("Game Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image));canvasObject.transform.SetParent(root.transform,false);
            canvasObject.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvasObject.GetComponent<Image>().color=EngineeringPlaygroundTheme.Canvas;canvasObject.GetComponent<Image>().raycastTarget=false;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;

            var hudPanel=ProductionUIFactory.Panel(canvasObject.transform,"Level HUD",new Vector2(.025f,.90f),new Vector2(.975f,.975f),EngineeringPlaygroundTheme.Surface,EngineeringPlaygroundTheme.RadiusLarge);
            var header=ProductionUIFactory.Text(hudPanel.transform,"Header",new Vector2(.035f,.12f),new Vector2(.73f,.88f),27,TextAnchor.MiddleLeft,EngineeringPlaygroundTheme.Text,FontStyle.Bold);
            var description=ProductionUIFactory.Text(hudPanel.transform,"Objective",new Vector2(.74f,.14f),new Vector2(.965f,.86f),15,TextAnchor.MiddleRight,EngineeringPlaygroundTheme.TextMuted);

            var workspaceFrame=ProductionUIFactory.Panel(canvasObject.transform,"Simulation Shell",new Vector2(.025f,.145f),new Vector2(.975f,.885f),EngineeringPlaygroundTheme.SurfaceRaised,EngineeringPlaygroundTheme.RadiusLarge);
            var workspaceObject=new GameObject("Flow Workspace",typeof(RectTransform),typeof(RawImage),typeof(FlowFieldVisualizer),typeof(FlowTouchEditor));workspaceObject.transform.SetParent(workspaceFrame.transform,false);
            var workspaceRect=workspaceObject.GetComponent<RectTransform>();ProductionUIFactory.Stretch(workspaceRect,new Vector2(.008f,.012f),new Vector2(.992f,.988f));
            var rawImage=workspaceObject.GetComponent<RawImage>();rawImage.raycastTarget=true;
            var visualizer=workspaceObject.GetComponent<FlowFieldVisualizer>();visualizer.Configure(controller,rawImage);
            var editor=workspaceObject.GetComponent<FlowTouchEditor>();editor.Configure(controller,workspaceRect);

            var solidObject=new GameObject("Smooth Geometry",typeof(RectTransform),typeof(RawImage),typeof(SmoothSolidOverlay));solidObject.transform.SetParent(workspaceObject.transform,false);
            var solidRect=solidObject.GetComponent<RectTransform>();ProductionUIFactory.Stretch(solidRect,Vector2.zero,Vector2.one);var solidImage=solidObject.GetComponent<RawImage>();solidObject.GetComponent<SmoothSolidOverlay>().Configure(controller,solidImage);

            var tracerObject=new GameObject("Flow Streaks",typeof(RectTransform),typeof(CanvasRenderer),typeof(FlowTracerOverlay));tracerObject.transform.SetParent(workspaceObject.transform,false);
            ProductionUIFactory.Stretch(tracerObject.GetComponent<RectTransform>(),Vector2.zero,Vector2.one);tracerObject.GetComponent<FlowTracerOverlay>().Configure(controller);

            var overlayObject=new GameObject("Showcase Packaging Overlay",typeof(RectTransform),typeof(ShowcasePackagingOverlay));overlayObject.transform.SetParent(workspaceObject.transform,false);
            ProductionUIFactory.Stretch(overlayObject.GetComponent<RectTransform>(),Vector2.zero,Vector2.one);var showcaseOverlay=overlayObject.GetComponent<ShowcasePackagingOverlay>();showcaseOverlay.Configure(showcaseSession);showcaseOverlay.SetVisible(false);

            var inlet=ProductionUIFactory.Text(workspaceFrame.transform,"Inlet",new Vector2(.014f,.43f),new Vector2(.13f,.57f),15,TextAnchor.MiddleLeft,EngineeringPlaygroundTheme.Accent,FontStyle.Bold);inlet.text="IN  →";
            var outlet=ProductionUIFactory.Text(workspaceFrame.transform,"Outlet",new Vector2(.86f,.43f),new Vector2(.986f,.57f),15,TextAnchor.MiddleRight,EngineeringPlaygroundTheme.Accent,FontStyle.Bold);outlet.text="→  OUT";

            var hintPanel=ProductionUIFactory.Panel(workspaceFrame.transform,"Coach Toast",new Vector2(.035f,.895f),new Vector2(.50f,.97f),new Color32(8,20,31,225),EngineeringPlaygroundTheme.RadiusMedium);
            hintPanel.AddComponent<CanvasGroup>();hintPanel.AddComponent<ProductionToast>();
            var hint=ProductionUIFactory.Text(hintPanel.transform,"Hint",new Vector2(.04f,.08f),new Vector2(.96f,.92f),15,TextAnchor.MiddleLeft,EngineeringPlaygroundTheme.Text,FontStyle.Bold);

            var resultCard=ProductionUIFactory.Panel(canvasObject.transform,"Result Sheet",new Vector2(.16f,.18f),new Vector2(.84f,.51f),new Color32(10,22,34,252),EngineeringPlaygroundTheme.RadiusLarge);
            resultCard.AddComponent<CanvasGroup>();resultCard.AddComponent<ProductionResultSheet>();
            var result=ProductionUIFactory.Text(resultCard.transform,"Result",new Vector2(.055f,.10f),new Vector2(.945f,.90f),21,TextAnchor.MiddleLeft,EngineeringPlaygroundTheme.Text);
            resultCard.SetActive(false);
            var reference=ProductionUIFactory.Text(canvasObject.transform,"Reference Estimate",new Vector2(.04f,.18f),new Vector2(.96f,.78f),17,TextAnchor.UpperLeft,EngineeringPlaygroundTheme.Text);reference.gameObject.SetActive(false);

            var hud=root.AddComponent<FlowChallengeHud>();hud.Configure(challengeSession,header,description,result);
            var modeController=root.AddComponent<FlowLabModeController>();modeController.Configure(controller,challengeSession,hud,showcaseSession,showcaseOverlay,workspaceObject,header,description,result,reference);

            var toolDock=ProductionUIFactory.Panel(canvasObject.transform,"Tool Dock",new Vector2(.025f,.035f),new Vector2(.47f,.125f),EngineeringPlaygroundTheme.Surface,EngineeringPlaygroundTheme.RadiusLarge);
            var toolRail=ProductionUIFactory.Bar(toolDock.transform,"Tools",new Vector2(.02f,.10f),new Vector2(.98f,.90f),10f);
            Button drawButton=null,eraseButton=null;
            drawButton=ProductionUIFactory.Button(toolRail.transform,"DRAW",()=>{editor.SetTool(FlowEditorTool.Draw);drawButton.GetComponent<ProductionButton>().SetSelected(true);eraseButton.GetComponent<ProductionButton>().SetSelected(false);});
            eraseButton=ProductionUIFactory.Button(toolRail.transform,"ERASE",()=>{editor.SetTool(FlowEditorTool.Erase);drawButton.GetComponent<ProductionButton>().SetSelected(false);eraseButton.GetComponent<ProductionButton>().SetSelected(true);});
            ProductionUIFactory.Button(toolRail.transform,"UNDO",editor.Undo,ProductionButton.Variant.Icon,16);
            ProductionUIFactory.Button(toolRail.transform,"VIEW",()=>visualizer.CycleViewMode(),ProductionButton.Variant.Icon,16);
            drawButton.GetComponent<ProductionButton>().SetSelected(true);

            var actionDock=ProductionUIFactory.Panel(canvasObject.transform,"Action Dock",new Vector2(.65f,.035f),new Vector2(.975f,.125f),EngineeringPlaygroundTheme.Surface,EngineeringPlaygroundTheme.RadiusLarge);
            var actionRail=ProductionUIFactory.Bar(actionDock.transform,"Actions",new Vector2(.025f,.10f),new Vector2(.975f,.90f),10f);
            var resetButton=ProductionUIFactory.Button(actionRail.transform,"↻",modeController.Reset,ProductionButton.Variant.Icon,24);
            var primaryButton=ProductionUIFactory.Button(actionRail.transform,"RUN FLOW  ▶",()=>{},ProductionButton.Variant.Primary,18);var primaryLabel=primaryButton.GetComponentInChildren<Text>();
            var nextButton=ProductionUIFactory.Button(actionRail.transform,"NEXT",()=>{},ProductionButton.Variant.Primary,16);nextButton.gameObject.SetActive(false);
            var prevButton=ProductionUIFactory.Button(actionRail.transform,"PREV",modeController.Previous,ProductionButton.Variant.Icon,15);prevButton.gameObject.SetActive(false);

            var play=root.AddComponent<FlowChallengePlayController>();play.Configure(controller,challengeSession);
            var experience=root.AddComponent<FlowChallengeExperience>();experience.Configure(play,challengeSession,editor,toolDock,resultCard,hint,primaryButton,primaryLabel,nextButton);

            primaryButton.onClick.RemoveAllListeners();primaryButton.onClick.AddListener(()=>{if(modeController.Mode==FlowLabMode.Challenge)experience.PrimaryAction();else modeController.ToggleRunning();});
            nextButton.onClick.RemoveAllListeners();nextButton.onClick.AddListener(()=>{if(modeController.Mode==FlowLabMode.Challenge)experience.Next();else modeController.Next();});
            resetButton.onClick.RemoveAllListeners();resetButton.onClick.AddListener(()=>{if(modeController.Mode==FlowLabMode.Challenge)play.Retry();else modeController.Reset();});
            prevButton.onClick.RemoveAllListeners();prevButton.onClick.AddListener(()=>{if(modeController.Mode==FlowLabMode.Challenge)play.Previous();else modeController.Previous();});

            root.AddComponent<FlowLabModeHotkeys>().Configure(modeController,experience);
        }

        private static void EnsureEventSystem(){if(EventSystem.current!=null)return;var eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));Object.DontDestroyOnLoad(eventSystem);}
    }

    public sealed class FlowLabModeHotkeys:MonoBehaviour
    {
        private FlowLabModeController _m;private FlowChallengeExperience _e;
        public void Configure(FlowLabModeController mode,FlowChallengeExperience experience){_m=mode;_e=experience;}
        private void Update(){if(UnityEngine.InputSystem.Keyboard.current==null)return;var k=UnityEngine.InputSystem.Keyboard.current;if(k.digit1Key.wasPressedThisFrame){_m.SetMode(FlowLabMode.Challenge);_e.SetChallengeModeActive(true);}else if(k.digit2Key.wasPressedThisFrame){_e.SetChallengeModeActive(false);_m.SetMode(FlowLabMode.Sandbox);}else if(k.digit3Key.wasPressedThisFrame){_e.SetChallengeModeActive(false);_m.SetMode(FlowLabMode.Learn);}else if(k.digit4Key.wasPressedThisFrame){_e.SetChallengeModeActive(false);_m.SetMode(FlowLabMode.Showcase);}}
    }
}
