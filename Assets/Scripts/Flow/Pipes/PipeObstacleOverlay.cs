using EngineeringPlayground.Flow.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Pipes
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PipeObstacleOverlay : MaskableGraphic
    {
        private FlowLabRuntimeController _controller;

        public void Configure(FlowLabRuntimeController controller)
        {
            _controller=controller;raycastTarget=false;
            if(_controller!=null)_controller.SolverUpdated+=Refresh;
            SetVerticesDirty();
        }

        protected override void OnDestroy()
        {
            if(_controller!=null)_controller.SolverUpdated-=Refresh;
            base.OnDestroy();
        }

        private void Refresh()=>SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(_controller==null||!_controller.HasFixedObstacle)return;
            var r=rectTransform.rect;
            var uv=_controller.FixedObstacleCenter;
            var c=new Vector2(r.xMin+uv.x*r.width,r.yMin+uv.y*r.height);
            var rad=Mathf.Max(18f,_controller.FixedObstacleRadius*r.height);

            // Distinct from both the dark environment and teal pipe: a warm physical component with
            // a crisp rim and hazard accent so the routing constraint is obvious at first glance.
            AddCircle(vh,c,rad*1.30f,new Color32(255,174,74,34),40);
            AddCircle(vh,c,rad*1.12f,new Color32(255,194,92,255),40);
            AddCircle(vh,c,rad,new Color32(67,76,91,255),40);
            AddCircle(vh,c-new Vector2(rad*.18f,-rad*.18f),rad*.68f,new Color32(91,103,119,255),36);

            var stripeWidth=rad*.20f;
            for(var i=-2;i<=2;i++)
            {
                var y=c.y+i*rad*.30f;
                AddSegment(vh,new Vector2(c.x-rad*.58f,y-stripeWidth*.45f),new Vector2(c.x+rad*.58f,y+stripeWidth*.45f),stripeWidth,new Color32(255,194,92,210));
            }
        }

        private static void AddSegment(VertexHelper vh,Vector2 a,Vector2 b,float width,Color32 color)
        {
            var d=b-a;if(d.sqrMagnitude<.001f)return;var n=new Vector2(-d.y,d.x).normalized*width*.5f;var start=vh.currentVertCount;
            vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);
            vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
        }

        private static void AddCircle(VertexHelper vh,Vector2 c,float rad,Color32 color,int segments)
        {
            var start=vh.currentVertCount;vh.AddVert(c,color,Vector2.zero);
            for(var i=0;i<=segments;i++){var a=i*Mathf.PI*2f/segments;vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}
        }
    }
}
