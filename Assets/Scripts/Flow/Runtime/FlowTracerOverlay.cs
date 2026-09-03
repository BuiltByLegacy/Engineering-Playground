using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FlowTracerOverlay : MaskableGraphic
    {
        private const int History = 9;
        private sealed class Tracer
        {
            public Vector2 Position;
            public readonly Vector2[] Trail = new Vector2[History];
            public float Life;
            public float Phase;
            public float Stuck;
            public int TrailCount;
        }

        private readonly List<Tracer> _tracers = new();
        private FlowLabRuntimeController _controller;
        private int _targetCount;
        private float _runBlend;

        public void Configure(FlowLabRuntimeController controller){_controller=controller;raycastTarget=false;_targetCount=Screen.width<=1200?42:64;Seed();}

        private void Update()
        {
            var active=_controller?.Solver!=null&&_controller.Running;
            _runBlend=Mathf.MoveTowards(_runBlend,active?1f:0f,Time.unscaledDeltaTime*(active?2.8f:7f));
            if(!active){if(_runBlend>0f)SetVerticesDirty();return;}
            if(_tracers.Count==0)Seed();var solver=_controller.Solver;var dt=Mathf.Min(Time.unscaledDeltaTime,.033f);
            for(var i=0;i<_tracers.Count;i++)
            {
                var t=_tracers[i];PushTrail(t,t.Position);
                var velocity=SampleVelocity(solver,t.Position);var speed=velocity.magnitude;
                t.Stuck=speed<.0035f?t.Stuck+dt:Mathf.Max(0f,t.Stuck-dt*2f);
                // Midpoint integration follows curved solver streamlines more smoothly than one nearest-cell Euler step.
                var mid=t.Position+velocity*(dt*5.5f);var v2=SampleVelocity(solver,mid);
                t.Position+=v2*(dt*11f);t.Life-=dt;
                var idx=IndexAt(solver,t.Position);
                if(t.Life<=0||t.Position.x>.997f||t.Position.x<0f||t.Position.y>1f||t.Position.y<0f||solver.Solid[idx]||t.Stuck>.55f)Respawn(t,i);
            }
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(_controller?.Solver==null||_runBlend<=.01f)return;var rect=rectTransform.rect;
            for(var i=0;i<_tracers.Count;i++)
            {
                var t=_tracers[i];if(t.TrailCount<2)continue;
                var pulse=.86f+.14f*Mathf.Sin(Time.unscaledTime*3.2f+t.Phase);var width=Mathf.Max(1.15f,rect.height*.0022f)*pulse;
                for(var s=1;s<t.TrailCount;s++)
                {
                    var a=ToLocal(rect,t.Trail[s-1]);var b=ToLocal(rect,t.Trail[s]);var d=b-a;if(d.sqrMagnitude<.08f)continue;
                    var age=s/(float)(t.TrailCount-1);var alpha=(byte)Mathf.RoundToInt(Mathf.Lerp(18f,205f,age)*_runBlend);var c=new Color32(180,255,242,alpha);
                    AddSegment(vh,a,b,width*Mathf.Lerp(.45f,1f,age),c);
                }
                var head=ToLocal(rect,t.Position);AddCircle(vh,head,width*1.25f,new Color32(220,255,248,(byte)(220*_runBlend)),8);
            }
        }

        private static Vector2 SampleVelocity(Flow.Simulation.D2Q9LbmSolver solver,Vector2 p)
        {
            var fx=Mathf.Clamp01(p.x)*(solver.Width-1);var fy=Mathf.Clamp01(p.y)*(solver.Height-1);var x0=Mathf.Clamp(Mathf.FloorToInt(fx),1,solver.Width-2);var y0=Mathf.Clamp(Mathf.FloorToInt(fy),1,solver.Height-2);var x1=Mathf.Min(x0+1,solver.Width-2);var y1=Mathf.Min(y0+1,solver.Height-2);var tx=fx-x0;var ty=fy-y0;
            Vector2 V(int x,int y){var k=y*solver.Width+x;return solver.Solid[k]?Vector2.zero:new Vector2((float)solver.VelocityX[k],(float)solver.VelocityY[k]);}
            return Vector2.Lerp(Vector2.Lerp(V(x0,y0),V(x1,y0),tx),Vector2.Lerp(V(x0,y1),V(x1,y1),tx),ty);
        }
        private static int IndexAt(Flow.Simulation.D2Q9LbmSolver solver,Vector2 p){var x=Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(p.x)*(solver.Width-1)),1,solver.Width-2);var y=Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(p.y)*(solver.Height-1)),1,solver.Height-2);return y*solver.Width+x;}
        private static void PushTrail(Tracer t,Vector2 p){var count=Mathf.Min(t.TrailCount+1,History);for(var i=count-1;i>0;i--)t.Trail[i]=t.Trail[i-1];t.Trail[0]=p;t.TrailCount=count;}
        private static Vector2 ToLocal(Rect r,Vector2 p)=>new(r.xMin+p.x*r.width,r.yMin+p.y*r.height);
        private void Seed(){_tracers.Clear();for(var i=0;i<_targetCount;i++){var t=new Tracer();Respawn(t,i,true);_tracers.Add(t);}SetVerticesDirty();}

        private void Respawn(Tracer t,int index,bool distributed=false)
        {
            var lane=Mathf.Repeat(index*.6180339f,1f);Vector2 p;
            if(_controller?.PipePath!=null){var center=_controller.PipePath.Sample(0f);var usableRadius=_controller.PipePath.Radius*.48f;var y=center.y+Mathf.Lerp(-usableRadius,usableRadius,lane);var x=distributed?Mathf.Lerp(.012f,.07f,Mathf.Repeat(index*.381966f,1f)):.014f;p=new Vector2(x,Mathf.Clamp01(y));}
            else p=new Vector2(.014f,.08f+lane*.84f);
            t.Position=p;t.Life=6f+lane*3f;t.Phase=lane*6.28318f;t.Stuck=0f;t.TrailCount=History;for(var j=0;j<History;j++)t.Trail[j]=p-new Vector2(.0045f*j,0f);
        }
        private static void AddSegment(VertexHelper vh,Vector2 a,Vector2 b,float width,Color32 color){var d=b-a;if(d.sqrMagnitude<.001f)return;var n=new Vector2(-d.y,d.x).normalized*width*.5f;var start=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);}
        private static void AddCircle(VertexHelper vh,Vector2 c,float rad,Color32 color,int segments){var start=vh.currentVertCount;vh.AddVert(c,color,Vector2.zero);for(var i=0;i<=segments;i++){var a=i*Mathf.PI*2f/segments;vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}}
    }
}
