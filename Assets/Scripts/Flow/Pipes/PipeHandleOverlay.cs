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
        private Vector2 _dragOffset;
        private float _pressPulse;

        public void Configure(FlowLabRuntimeController controller){_controller=controller;raycastTarget=true;if(_controller?.PipePath!=null)_controller.PipePath.Changed+=OnPathChanged;SetVerticesDirty();}
        protected override void OnDestroy(){if(_controller?.PipePath!=null)_controller.PipePath.Changed-=OnPathChanged;base.OnDestroy();}
        public void SetVisible(bool value){_visible=value;raycastTarget=value;SetVerticesDirty();}
        private void OnPathChanged()=>SetVerticesDirty();
        private void Update(){if(_active>=0){_pressPulse=Mathf.MoveTowards(_pressPulse,1f,Time.unscaledDeltaTime*10f);SetVerticesDirty();}else if(_pressPulse>0f){_pressPulse=Mathf.MoveTowards(_pressPulse,0f,Time.unscaledDeltaTime*12f);SetVerticesDirty();}}

        public void OnPointerDown(PointerEventData e)
        {
            if(!_visible||_controller?.PipePath==null)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var local))return;
            var uv=LocalToUv(local);var best=.105f;_active=-1;
            for(var i=1;i<_controller.PipePath.Points.Count-1;i++){var d=Vector2.Distance(uv,_controller.PipePath.Points[i]);if(d<best){best=d;_active=i;}}
            if(_active>=0){_dragOffset=_controller.PipePath.Points[_active]-uv;_pressPulse=.35f;SetVerticesDirty();}
        }
        public void OnDrag(PointerEventData e)
        {
            if(_active<0||_controller==null)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var local))return;
            var target=LocalToUv(local)+_dragOffset;
            target.x=Mathf.Clamp(target.x,.08f,.92f);target.y=Mathf.Clamp(target.y,.10f,.90f);
            _controller.MovePipeHandle(_active,target);
        }
        public void OnPointerUp(PointerEventData e){_active=-1;SetVerticesDirty();}

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(!_visible||_controller?.PipePath==null)return;var r=rectTransform.rect;
            const int samples=96;var last=ToLocal(r,_controller.PipePath.Sample(0f));
            for(var s=1;s<=samples;s++){var next=ToLocal(r,_controller.PipePath.Sample(s/(float)samples));AddSegment(vh,last,next,Mathf.Max(2.4f,r.height*.0032f),new Color32(201,255,243,170));last=next;}

            for(var i=1;i<_controller.PipePath.Points.Count-1;i++)
            {
                var p=_controller.PipePath.Points[i];var c=ToLocal(r,p);var active=i==_active;var baseRad=Mathf.Max(15f,r.height*.018f);var rad=baseRad*(active?1.12f+.08f*_pressPulse:1f);
                if(active)AddCircle(vh,c,rad*1.55f,new Color32(55,220,184,55),32);
                AddCircle(vh,c,rad,new Color32(246,250,252,250),32);
                AddCircle(vh,c,rad*.56f,active?new Color32(255,194,92,255):new Color32(55,220,184,255),28);
            }
        }

        private static Vector2 ToLocal(Rect r,Vector2 p)=>new(r.xMin+p.x*r.width,r.yMin+p.y*r.height);
        private Vector2 LocalToUv(Vector2 local){var r=rectTransform.rect;return new Vector2(Mathf.InverseLerp(r.xMin,r.xMax,local.x),Mathf.InverseLerp(r.yMin,r.yMax,local.y));}
        private static void AddSegment(VertexHelper vh,Vector2 a,Vector2 b,float width,Color32 color){var d=b-a;if(d.sqrMagnitude<.001f)return;var n=new Vector2(-d.y,d.x).normalized*width*.5f;var start=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);}
        private static void AddCircle(VertexHelper vh,Vector2 c,float rad,Color32 color,int segments){var start=vh.currentVertCount;vh.AddVert(c,color,Vector2.zero);for(var i=0;i<=segments;i++){var a=i*Mathf.PI*2f/segments;vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}}
    }
}
