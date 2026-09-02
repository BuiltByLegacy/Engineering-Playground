using UnityEngine;

namespace EngineeringPlayground.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ProductionResultSheet : MonoBehaviour
    {
        private CanvasGroup _group; private RectTransform _rect; private bool _visible; private float _target;
        private void Awake(){_group=GetComponent<CanvasGroup>();_rect=transform as RectTransform;_group.alpha=0f;}
        public void SetVisible(bool visible,bool immediate=false){_visible=visible;_target=visible?1f:0f;if(visible&&!gameObject.activeSelf)gameObject.SetActive(true);if(immediate){Apply(_target);if(!visible)gameObject.SetActive(false);}}
        private void Update(){if(_group==null)return;var value=Mathf.MoveTowards(_group.alpha,_target,Time.unscaledDeltaTime/EngineeringPlaygroundTheme.MotionNormal);Apply(value);if(!_visible&&value<=.001f)gameObject.SetActive(false);}
        private void Apply(float value){_group.alpha=value;_group.interactable=value>.95f;_group.blocksRaycasts=value>.95f;if(_rect!=null)_rect.anchoredPosition=new Vector2(_rect.anchoredPosition.x,Mathf.Lerp(-48f,0f,value));}
    }
}
