using EngineeringPlayground.Flow.Challenges;
using EngineeringPlayground.Flow.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Pipes
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PipeDiameterHandleOverlay : MaskableGraphic, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private FlowLabRuntimeController _controller;
        private FlowChallengeSession _session;
        private int _active=-1;
        private bool _visible;
        private float _pulse;

        public void Configure(FlowLabRuntimeController controller,FlowChallengeSession session)
        {
            _controller=controller;_session=session;raycastTarget=false;
            if(_controller?.PipePath!=null)_controller.PipePath.Changed+=OnPathChanged;
            if(_session!=null)_session.ChallengeChanged+=OnChallengeChanged;
            RefreshVisibility();
        }
        protected override void OnDestroy()
        {
            if(_controller?.PipePath!=null)_controller.PipePath.Changed-=OnPathChanged;
            if(_session!=null)_session.ChallengeChanged-=OnChallengeChanged;
            base.OnDestroy();
        }
        public void SetVisible(bool value){_visible=value&&DiameterEditable();raycastTarget=_visible;SetVerticesDirty();}
        private void OnChallengeChanged(){RefreshVisibility();SetVerticesDirty();}
        private void OnPathChanged()=>SetVerticesDirty();
        private bool DiameterEditable()=>_session?.CurrentChallenge?.DomainConfig?.Value<bool?>("diameter_editable")==true;
        private void RefreshVisibility(){_visible=DiameterEditable();raycastTarget=_visible;}
        private void Update(){var target=_active>=0?1f:0f;_pulse=Mathf.MoveTowards(_pulse,target,Time.unscaledDeltaTime*10f);if(_active>=0||_pulse>0f)SetVerticesDirty();}

        public void OnPointerDown(PointerEventData e)
        {
            if(!_visible||_controller?.PipePath==null)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var local))return;
            var r=rectTransform.rect;var best=55f;_active=-1;
            for(var i=1;i<_controller.PipePath.Points.Count-1;i++)
            {
                var h=HandleLocal(r,i);var d=Vector2.Distance(local,h);if(d<best){best=d;_active=i;}
            }
            SetVerticesDirty();
        }

        public void OnDrag(PointerEventData e)
        {
            if(_active<0||_controller?.PipePath==null)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var local))return;
            var r=rectTransform.rect;var center=ToLocal(r,_controller.PipePath.Points[_active]);var normal=PointNormalLocal(r,_active);
            var signed=Vector2.Dot(local-center,normal);var normalized=Mathf.Abs(signed)/Mathf.Max(1f,r.height);
            _controller.SetPipeRadiusHandle(_active,Mathf.Clamp(normalized,.045f,.18f));
        }
        public void OnPointerUp(PointerEventData e){_active=-1;SetVerticesDirty();}

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(!_visible||_controller?.PipePath==null)return;var r=rectTransform.rect;
            for(var i=1;i<_controller.PipePath.Points.Count-1;i++)
            {
                var center=ToLocal(r,_controller.PipePath.Points[i]);var handle=HandleLocal(r,i);var active=i==_active;
                AddSegment(vh,center,handle,Mathf.Max(2f,r.height*.0025f),new Color32(255,194,92,active?(byte)235:(byte)150));
                var rad=Mathf.Max(12f,r.height*.014f)*(active?1.15f+.08f*_pulse:1f);
                if(active)AddCircle(vh,handle,rad*1.6f,new Color32(255,194,92,55),24);
                AddDiamond(vh,handle,rad,new Color32(255,236,190,255));
                AddDiamond(vh,handle,rad*.55f,new Color32(255,194,92,255));
            }
        }

        private Vector2 HandleLocal(Rect r,int index)
        {
            var center=ToLocal(r,_controller.PipePath.Points[index]);var normal=PointNormalLocal(r,index);var radius=_controller.PipePath.RadiusProfile[index]*r.height;return center+normal*radius;
        }
        private Vector2 PointNormalLocal(Rect r,int index)
        {
            var points=_controller.PipePath.Points;var a=ToLocal(r,points[Mathf.Max(0,index-1)]);var b=ToLocal(r,points[Mathf.Min(points.Count-1,index+1)]);var tangent=(b-a).normalized;var normal=new Vector2(-tangent.y,tangent.x);return normal.y<0?-normal:normal;
        }
        private static Vector2 ToLocal(Rect r,Vector2 p)=>new(r.xMin+p.x*r.width,r.yMin+p.y*r.height);
        private static void AddSegment(VertexHelper vh,Vector2 a,Vector2 b,float width,Color32 color){var d=b-a;if(d.sqrMagnitude<.001f)return;var n=new Vector2(-d.y,d.x).normalized*width*.5f;var s=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(s,s+1,s+2);vh.AddTriangle(s,s+2,s+3);}
        private static void AddCircle(VertexHelper vh,Vector2 c,float rad,Color32 color,int segments){var s=vh.currentVertCount;vh.AddVert(c,color,Vector2.zero);for(var i=0;i<=segments;i++){var a=i*Mathf.PI*2f/segments;vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);if(i>0)vh.AddTriangle(s,s+i,s+i+1);}}
        private static void AddDiamond(VertexHelper vh,Vector2 c,float rad,Color32 color){var s=vh.currentVertCount;vh.AddVert(c+Vector2.up*rad,color,Vector2.zero);vh.AddVert(c+Vector2.right*rad,color,Vector2.zero);vh.AddVert(c+Vector2.down*rad,color,Vector2.zero);vh.AddVert(c+Vector2.left*rad,color,Vector2.zero);vh.AddTriangle(s,s+1,s+2);vh.AddTriangle(s,s+2,s+3);}
    }
}
