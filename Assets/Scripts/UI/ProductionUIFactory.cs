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
            var layout=go.GetComponent<HorizontalLayoutGroup>();layout.spacing=spacing;layout.childForceExpandWidth=true;layout.childForceExpandHeight=true;layout.childControlWidth=true;layout.childControlHeight=true;return go;
        }

        public static Text Text(Transform parent,string name,Vector2 min,Vector2 max,int size,TextAnchor align,Color color,FontStyle style=FontStyle.Normal)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);Stretch(go.GetComponent<RectTransform>(),min,max);
            var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=align;text.color=color;text.fontStyle=style;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
        }

        public static Button Button(Transform parent,string label,UnityEngine.Events.UnityAction action,ProductionButton.Variant variant=ProductionButton.Variant.Tool,int fontSize=16)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(RoundedPanelGraphic),typeof(Button),typeof(ProductionButton));go.transform.SetParent(parent,false);
            var button=go.GetComponent<Button>();button.transition=Selectable.Transition.None;button.onClick.AddListener(action);
            var text=Text(go.transform,"Label",Vector2.zero,Vector2.one,fontSize,TextAnchor.MiddleCenter,EngineeringPlaygroundTheme.Text,FontStyle.Bold);text.text=label;
            go.GetComponent<ProductionButton>().Configure(variant,text);return button;
        }

        public static void Stretch(RectTransform rect,Vector2 min,Vector2 max){rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
