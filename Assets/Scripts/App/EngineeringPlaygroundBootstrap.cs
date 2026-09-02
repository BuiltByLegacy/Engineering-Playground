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

            // Compact phone-first HUD: level title is the hero, progress/stars are secondary.
            var hudPanel=ProductionUIFactory.Panel(canvasObject.transform,"Level HUD",new Vector2(.025f,.91f),new Vector2(.975f,.978f),EngineeringPlaygroundTheme.Surface,EngineeringPlaygroundTheme.RadiusLarge);
            var header=ProductionUIFactory.Text(hudPanel.transform,"Header",new Vector2(.035f,.12f),new Vector2(.70f,.88f),29,TextAnchor.MiddleLeft,EngineeringPlaygroundTheme.Text,FontStyle.Bold);
            var description=ProductionUIFactory.Text(hudPanel.transform,"Progress",new Vector2(.71f,.12f),new Vector2(.96f,.88f),17,TextAnchor.MiddleRight,EngineeringPlaygroundTheme.TextMuted,FontStyle.Bold);

            var workspaceFrame=ProductionUIFactory.Panel(canvasObject.transform,"Simulation Shell",new Vector2(.025f,.145f),new Vector2(.975f,.895f),EngineeringPlaygroundTheme.SurfaceRaised,EngineeringPlaygroundTheme.RadiusLarge);
            var workspaceObject=new GameObject("Flow Workspace",typeof(RectTransform),typeof(RawImage),typeof(FlowFieldVisualizer),typeof(FlowTouchEditor));workspaceObject.transform.SetParent(workspaceFrame.transform,false);
            var workspaceRect=workspaceObject.GetComponent<RectTransform>();ProductionUIFactory.Stretch(workspaceRect,new Vector2(.008f,.012f),new Vector2(.992f,.988f));
            var rawImage=workspaceObject.GetComponent<RawImage>();rawImage.raycastTarget=true;
            var visualizer=workspaceObject.GetComponent<FlowFieldVisualizer>();visualizer.Configure(controller,rawImage);
            var editor=workspaceObject.GetComponent<FlowTouchEditor>();editor.Configure(controller,workspaceRect);

            var solidObject=new GameObject("Smooth Geometry",typeof(RectTransform),typeof(RawImage),typeof(SmoothSolidOverlay));solidObject.transform.SetParent(workspaceObject.transform,false);
            ProductionUIFactory.Stretch(solidObject.GetComponent<RectTransform>(),Vector2.zero,Vector2.one);var solidImage=solidObject.GetComponent<RawImage>();solidObject.GetComponent<SmoothSolidOverlay>().Configure(controller,solidImage);

            var tracerObject=new GameObject("Flow Streaks",typeof(RectTransform),typeof(CanvasRenderer),typeof(FlowTracerOverlay));tracerObject.transform.SetParent(workspaceObject.transform,false);
            ProductionUIFactory.Stretch(tracerObject.GetComponent<RectTransform>(),Vector2.zero,Vector2.one);tracerObject.GetComponent<FlowTracerOverlay>().Configure(controller);

            var overlayObject=new GameObject("Showcase Packaging Overlay",typeof(RectTransform),typeof(ShowcasePackagingOverlay));overlayObject.transform.SetParent(workspaceObject.transform,false);
            ProductionUIFactory.Stretch(overlayObject.GetComponent<RectTransform>(),Vector2.zero,Vector2.one);var showcaseOverlay=overlayObject.GetComponent<ShowcasePackagingOverlay>();showcaseOverlay.Configure(showcaseSession);showcaseOverlay.SetVisible(false);

            // Direction chips replace tiny diagnostic labels.
            var inletChip=ProductionUIFactory.Panel(workspaceFrame.transform,"Inlet Chip",new Vector2(.018f,.455f),new Vector2(.115f,.545f),new Color32(8,31,40,225),EngineeringPlaygroundTheme.RadiusMedium);
            var inlet=ProductionUIFactory.Text(inletChip.transform,"Inlet",new Vector2(.08f,.08f),new Vector2(.92f,.92f),15,TextAnchor.MiddleCenter,EngineeringPlaygroundTheme.Accent,FontStyle.Bold);inlet.text="IN  →";
            var outletChip=ProductionUIFactory.Panel(workspaceFrame.transform,"Outlet Chip",new Vector2(.885f,.455f),new Vector2(.982f,.545f),new Color32(8,31,40,225),EngineeringPlaygroundTheme.RadiusMedium);
            var outlet=ProductionUIFactory.Text(outletChip.transform,"Outlet",new Vector2(.08f,.08f),new Vector2(.92f,.92f),15,TextAnchor.MiddleCenter,EngineeringPlaygroundTheme.Accent,FontStyle.Bold);outlet.text="→  OUT";

            var hintPanel=ProductionUIFactory.Panel(workspaceFrame.transform,"Coach Toast",new Vector2(.035f,.895f),new Vector2(.50f,.97f),new Color32(8,20,31,225),EngineeringPlaygroundTheme.RadiusMedium);
            hintPanel.AddComponent<CanvasGroup>();hintPanel.AddComponent<ProductionToast>();
            var hint=ProductionUIFactory.Text(hintPanel.transform,"Hint",new Vector2(.04f,.08f),new Vector2(.96f,.92f),15,TextAnchor.MiddleLeft,EngineeringPlaygroundTheme.Text,FontStyle.Bold);

            var resultCard=ProductionUIFactory.Panel(canvasObject.transform,"Result Sheet",new Vector2(.15f,.16f),new Vector2(.85f,.55f),new Color32(10,22,34,252),EngineeringPlaygroundTheme.RadiusLarge);
            resultCard.AddComponent<CanvasGroup>();resultCard.AddComponent<ProductionResultSheet>();
            var result=ProductionUIFactory.Text(resultCard.transform,"Result",new Vector2(.055f,.60f),new Vector2(.945f,.93f),21,TextAnchor.UpperLeft,EngineeringPlaygroundTheme.Text);
            var metricNames=new[]{"Flow","Pressure","Smoothness","Material"};var metrics=new ProductionMetricRow[4];
            for(var i=0;i<4;i++)
            {
                var row=new GameObject(metricNames[i],typeof(RectTransform),typeof(ProductionMetricRow));row.transform.SetParent(resultCard.transform,false);var yMax=.55f-i*.105f;ProductionUIFactory.Stretch(row.GetComponent<RectTransform>(),new Vector2(.055f,yMax-.08f),new Vector2(.945f,yMax));metrics[i]=row.GetComponent<ProductionMetricRow>();metrics[i].Configure(metricNames[i]);
            }
            resultCard.SetActive(false);
            var reference=ProductionUIFactory.Text(canvasObject.transform,"Reference Estimate",new Vector2(.04f,.18f),new Vector2(.96f,.78f),17,TextAnchor.UpperLeft,EngineeringPlaygroundTheme.Text);reference.gameObject.SetActive(false);

            var hud=root.AddComponent<FlowChallengeHud>();hud.Configure(challengeSession,header,description,result,metrics);
            var modeController=root.AddComponent<FlowLabModeController>();modeController.Configure(controller,challengeSession,hud,showcaseSession,showcaseOverlay,workspaceObject,header,description,result,reference);

            // Compact floating tool dock with vector icons and real selected state.
            var toolDock=ProductionUIFactory.Panel(canvasObject.transform,"Tool Dock",new Vector2(.025f,.035f),new Vector2(.50f,.125f),EngineeringPlaygroundTheme.Surface,EngineeringPlaygroundTheme.RadiusLarge);
            var toolRail=ProductionUIFactory.Bar(toolDock.transform,"Tools",new Vector2(.025f,.09f),new Vector2(.975f,.91f),8f);
            Button drawButton=null,eraseButton=null;
            drawButton=ProductionUIFactory.ToolButton(toolRail.transform,"DRAW",ProductionIconGraphic.Icon.Draw,()=>{editor.SetTool(FlowEditorTool.Draw);drawButton.GetComponent<ProductionButton>().SetSelected(true);eraseButton.GetComponent<ProductionButton>().SetSelected(false);},100f);
            eraseButton=ProductionUIFactory.ToolButton(toolRail.transform,"ERASE",ProductionIconGraphic.Icon.Erase,()=>{editor.SetTool(FlowEditorTool.Erase);drawButton.GetComponent<ProductionButton>().SetSelected(false);eraseButton.GetComponent<ProductionButton>().SetSelected(true);},100f);
            ProductionUIFactory.ToolButton(toolRail.transform,"UNDO",ProductionIconGraphic.Icon.Undo,editor.Undo,92f);
            ProductionUIFactory.ToolButton(toolRail.transform,"VIEW",ProductionIconGraphic.Icon.View,()=>visualizer.CycleViewMode(),92f);
            drawButton.GetComponent<ProductionButton>().SetSelected(true);

            // Only one secondary action and one dominant CTA. No font-dependent glyph buttons.
            var actionDock=ProductionUIFactory.Panel(canvasObject.transform,"Action Dock",new Vector2(.64f,.035f),new Vector2(.975f,.125f),EngineeringPlaygroundTheme.Surface,EngineeringPlaygroundTheme.RadiusLarge);
            var actionRail=ProductionUIFactory.Bar(actionDock.transform,"Actions",new Vector2(.035f,.09f),new Vector2(.965f,.91f),10f);
            var resetButton=ProductionUIFactory.IconButton(actionRail.transform,ProductionIconGraphic.Icon.Reset,modeController.Reset,72f);
            var primaryButton=ProductionUIFactory.PrimaryActionButton(actionRail.transform,"RUN FLOW",()=>{},224f);var primaryLabel=primaryButton.GetComponentInChildren<Text>();
            var nextButton=ProductionUIFactory.Button(actionRail.transform,"NEXT",()=>{},ProductionButton.Variant.Primary,16,110f);nextButton.gameObject.SetActive(false);
            var prevButton=ProductionUIFactory.IconButton(actionRail.transform,ProductionIconGraphic.Icon.Undo,modeController.Previous,72f);prevButton.gameObject.SetActive(false);

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
