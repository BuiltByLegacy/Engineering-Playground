using UnityEngine;

namespace EngineeringPlayground.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ProductionToast : MonoBehaviour
    {
        private CanvasGroup _group; private bool _visible=true; private float _target=1f;
        private void Awake(){_group=GetComponent<CanvasGroup>();}
        public void SetVisible(bool visible,bool immediate=false){_visible=visible;_target=visible?1f:0f;if(gameObject.activeSelf==false&&visible)gameObject.SetActive(true);if(immediate){_group.alpha=_target;if(!visible)gameObject.SetActive(false);}}
        private void Update(){if(_group==null)return;_group.alpha=Mathf.MoveTowards(_group.alpha,_target,Time.unscaledDeltaTime/EngineeringPlaygroundTheme.MotionNormal);transform.localScale=Vector3.one*(.96f+.04f*_group.alpha);if(!_visible&&_group.alpha<=.001f)gameObject.SetActive(false);}
    }
}
