using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EngineeringPlayground.UI
{
    [RequireComponent(typeof(Button),typeof(RoundedPanelGraphic))]
    public sealed class ProductionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Variant { Tool, Primary, Icon }
        [SerializeField] private Variant variant;
        private RoundedPanelGraphic _panel; private Button _button; private Text _label; private bool _selected;

        public void Configure(Variant value,Text label){variant=value;_label=label;_panel=GetComponent<RoundedPanelGraphic>();_button=GetComponent<Button>();Apply(false);}
        public void SetSelected(bool selected){_selected=selected;Apply(false);}
        public void OnPointerDown(PointerEventData eventData)=>Apply(true);
        public void OnPointerUp(PointerEventData eventData)=>Apply(false);
        public void OnPointerExit(PointerEventData eventData)=>Apply(false);

        private void Apply(bool pressed)
        {
            if(_panel==null)_panel=GetComponent<RoundedPanelGraphic>();
            var primary=variant==Variant.Primary; var active=primary||_selected;
            _panel.color=pressed?(primary?EngineeringPlaygroundTheme.AccentPressed:EngineeringPlaygroundTheme.SurfacePressed):(active?EngineeringPlaygroundTheme.Accent:EngineeringPlaygroundTheme.SurfaceRaised);
            if(_label!=null)_label.color=active?EngineeringPlaygroundTheme.Canvas:EngineeringPlaygroundTheme.Text;
        }
    }
}
