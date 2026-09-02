using EngineeringPlayground.Flow.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Pipes
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PipeHandleOverlay : MaskableGraphic, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private FlowLabRuntimeController _controller;
        private int _active = -1;
        private bool _visible = true;

        public void Configure(FlowLabRuntimeController controller){_controller=controller;raycastTarget=true;if(_controller?.PipePath!=null)_controller.PipePath.Changed+=OnPathChanged;SetVerticesDirty();}
        protected override void OnDestroy(){if(_controller?.PipePath!=null)_controller.PipePath.Changed-=OnPathChanged;base.OnDestroy();}
        public void SetVisible(bool value){_visible=value;raycastTarget=value;SetVerticesDirty();}
        private void OnPathChanged()=>SetVerticesDirty();

        public void OnPointerDown(PointerEventData e)
        {
            if(!_visible||_controller?.PipePath==null)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var local))return;
            var uv=LocalToUv(local);var best=.06f;_active=-1;
            for(var i=1;i<_controller.PipePath.Points.Count-1;i++){var d=Vector2.Distance(uv,_controller.PipePath.Points[i]);if(d<best){best=d;_active=i;}}
        }
        public void OnDrag(PointerEventData e)
        {
            if(_active<0||_controller==null)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var local))return;
            _controller.MovePipeHandle(_active,LocalToUv(local));
        }
        public void OnPointerUp(PointerEventData e){_active=-1;}

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(!_visible||_controller?.PipePath==null)return;var r=rectTransform.rect;
            for(var i=1;i<_controller.PipePath.Points.Count-1;i++)
            {
                var p=_controller.PipePath.Points[i];var c=new Vector2(r.xMin+p.x*r.width,r.yMin+p.y*r.height);var rad=Mathf.Max(14f,r.height*.018f);
                AddCircle(vh,c,rad,new Color32(244,250,252,245),24);
                AddCircle(vh,c,rad*.55f,new Color32(55,220,184,255),20);
            }
        }

        private Vector2 LocalToUv(Vector2 local){var r=rectTransform.rect;return new Vector2(Mathf.InverseLerp(r.xMin,r.xMax,local.x),Mathf.InverseLerp(r.yMin,r.yMax,local.y));}
        private static void AddCircle(VertexHelper vh,Vector2 c,float rad,Color32 color,int segments)
        {
            var start=vh.currentVertCount;vh.AddVert(c,color,Vector2.zero);
            for(var i=0;i<=segments;i++){var a=i*Mathf.PI*2f/segments;vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}
        }
    }
}
