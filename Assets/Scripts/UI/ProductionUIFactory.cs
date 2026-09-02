using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.UI
{
    public static class ProductionUIFactory
    {
        public static GameObject Panel(Transform parent,string name,Vector2 min,Vector2 max,Color color,float radius=EngineeringPlaygroundTheme.RadiusMedium)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(RoundedPanelGraphic));go.transform.SetParent(parent,false);
            Stretch(go.GetComponent<RectTransform>(),min,max);var panel=go.GetComponent<RoundedPanelGraphic>();panel.color=color;panel.SetRadius(radius);panel.raycastTarget=false;return go;
        }

        public static GameObject Bar(Transform parent,string name,Vector2 min,Vector2 max,float spacing)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(HorizontalLayoutGroup));go.transform.SetParent(parent,false);Stretch(go.GetComponent<RectTransform>(),min,max);
            var layout=go.GetComponent<HorizontalLayoutGroup>();layout.spacing=spacing;layout.childForceExpandWidth=false;layout.childForceExpandHeight=true;layout.childControlWidth=true;layout.childControlHeight=true;layout.childAlignment=TextAnchor.MiddleCenter;return go;
        }

        public static Text Text(Transform parent,string name,Vector2 min,Vector2 max,int size,TextAnchor align,Color color,FontStyle style=FontStyle.Normal)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);Stretch(go.GetComponent<RectTransform>(),min,max);
            var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=align;text.color=color;text.fontStyle=style;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
        }

        public static Button Button(Transform parent,string label,UnityEngine.Events.UnityAction action,ProductionButton.Variant variant=ProductionButton.Variant.Tool,int fontSize=16,float preferredWidth=0f)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(RoundedPanelGraphic),typeof(Button),typeof(ProductionButton),typeof(LayoutElement));go.transform.SetParent(parent,false);
            var button=go.GetComponent<Button>();button.transition=Selectable.Transition.None;button.onClick.AddListener(action);
            var layout=go.GetComponent<LayoutElement>();layout.minHeight=EngineeringPlaygroundTheme.TouchMin;if(preferredWidth>0)layout.preferredWidth=preferredWidth;else layout.flexibleWidth=1;
            var text=Text(go.transform,"Label",Vector2.zero,Vector2.one,fontSize,TextAnchor.MiddleCenter,EngineeringPlaygroundTheme.Text,FontStyle.Bold);text.text=label;
            go.GetComponent<ProductionButton>().Configure(variant,text);return button;
        }

        public static Button ToolButton(Transform parent,string label,ProductionIconGraphic.Icon icon,UnityEngine.Events.UnityAction action,float width=104f)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(RoundedPanelGraphic),typeof(Button),typeof(ProductionButton),typeof(LayoutElement));go.transform.SetParent(parent,false);
            var button=go.GetComponent<Button>();button.transition=Selectable.Transition.None;button.onClick.AddListener(action);
            var layout=go.GetComponent<LayoutElement>();layout.preferredWidth=width;layout.minHeight=EngineeringPlaygroundTheme.TouchMin;
            var iconObject=new GameObject("Icon",typeof(RectTransform),typeof(ProductionIconGraphic));iconObject.transform.SetParent(go.transform,false);Stretch(iconObject.GetComponent<RectTransform>(),new Vector2(.25f,.36f),new Vector2(.75f,.90f));iconObject.GetComponent<ProductionIconGraphic>().Configure(icon,EngineeringPlaygroundTheme.Text);
            var text=Text(go.transform,"Label",new Vector2(.08f,.05f),new Vector2(.92f,.34f),12,TextAnchor.MiddleCenter,EngineeringPlaygroundTheme.TextMuted,FontStyle.Bold);text.text=label;
            go.GetComponent<ProductionButton>().Configure(ProductionButton.Variant.Tool,text);return button;
        }

        public static Button IconButton(Transform parent,ProductionIconGraphic.Icon icon,UnityEngine.Events.UnityAction action,float width=68f)
        {
            var go=new GameObject(icon.ToString(),typeof(RectTransform),typeof(RoundedPanelGraphic),typeof(Button),typeof(ProductionButton),typeof(LayoutElement));go.transform.SetParent(parent,false);
            var button=go.GetComponent<Button>();button.transition=Selectable.Transition.None;button.onClick.AddListener(action);
            var layout=go.GetComponent<LayoutElement>();layout.preferredWidth=width;layout.minHeight=EngineeringPlaygroundTheme.TouchMin;
            var iconObject=new GameObject("Icon",typeof(RectTransform),typeof(ProductionIconGraphic));iconObject.transform.SetParent(go.transform,false);Stretch(iconObject.GetComponent<RectTransform>(),new Vector2(.22f,.22f),new Vector2(.78f,.78f));iconObject.GetComponent<ProductionIconGraphic>().Configure(icon,EngineeringPlaygroundTheme.TextMuted);
            var label=Text(go.transform,"StateLabel",Vector2.zero,Vector2.zero,1,TextAnchor.MiddleCenter,Color.clear);label.text=string.Empty;
            go.GetComponent<ProductionButton>().Configure(ProductionButton.Variant.Icon,label);return button;
        }

        public static Button PrimaryActionButton(Transform parent,string label,UnityEngine.Events.UnityAction action,float preferredWidth=230f)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(RoundedPanelGraphic),typeof(Button),typeof(ProductionButton),typeof(LayoutElement));go.transform.SetParent(parent,false);
            var button=go.GetComponent<Button>();button.transition=Selectable.Transition.None;button.onClick.AddListener(action);
            var layout=go.GetComponent<LayoutElement>();layout.preferredWidth=preferredWidth;layout.minHeight=EngineeringPlaygroundTheme.TouchMin;
            var iconObject=new GameObject("Icon",typeof(RectTransform),typeof(ProductionIconGraphic));iconObject.transform.SetParent(go.transform,false);Stretch(iconObject.GetComponent<RectTransform>(),new Vector2(.08f,.21f),new Vector2(.29f,.79f));iconObject.GetComponent<ProductionIconGraphic>().Configure(ProductionIconGraphic.Icon.Run,EngineeringPlaygroundTheme.Canvas);
            var text=Text(go.transform,"Label",new Vector2(.29f,.08f),new Vector2(.95f,.92f),17,TextAnchor.MiddleCenter,EngineeringPlaygroundTheme.Canvas,FontStyle.Bold);text.text=label;
            go.GetComponent<ProductionButton>().Configure(ProductionButton.Variant.Primary,text);return button;
        }

        public static void Stretch(RectTransform rect,Vector2 min,Vector2 max){rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
