using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.UI
{
    public sealed class ProductionMetricRow : MonoBehaviour
    {
        private RectTransform _fill; private Text _label; private Text _value;
        public void Configure(string label)
        {
            var track=ProductionUIFactory.Panel(transform,"Track",Vector2.zero,Vector2.one,new Color32(28,45,61,220),EngineeringPlaygroundTheme.RadiusSmall);
            var fill=ProductionUIFactory.Panel(track.transform,"Fill",Vector2.zero,new Vector2(.01f,1f),EngineeringPlaygroundTheme.Accent,EngineeringPlaygroundTheme.RadiusSmall);_fill=fill.GetComponent<RectTransform>();
            _label=ProductionUIFactory.Text(transform,"Label",new Vector2(.035f,.08f),new Vector2(.64f,.92f),14,TextAnchor.MiddleLeft,EngineeringPlaygroundTheme.Text,FontStyle.Bold);_label.text=label.ToUpperInvariant();
            _value=ProductionUIFactory.Text(transform,"Value",new Vector2(.72f,.08f),new Vector2(.96f,.92f),14,TextAnchor.MiddleRight,EngineeringPlaygroundTheme.Text,FontStyle.Bold);SetValue(0);
        }
        public void SetValue(double score)
        {
            var t=Mathf.Clamp01((float)(score/100.0));if(_fill!=null){_fill.anchorMax=new Vector2(Mathf.Lerp(.01f,1f,t),1f);_fill.offsetMax=Vector2.zero;}if(_value!=null)_value.text=$"{score:F0}";
        }
    }
}
