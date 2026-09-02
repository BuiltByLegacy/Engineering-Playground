using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FlowTracerOverlay : MaskableGraphic
    {
        private struct Tracer { public Vector2 Position; public Vector2 Previous; public float Life; public float Phase; }
        private readonly List<Tracer> _tracers = new();
        private FlowLabRuntimeController _controller;
        private int _targetCount;

        public void Configure(FlowLabRuntimeController controller){_controller=controller;raycastTarget=false;_targetCount=Screen.width<=1200?96:160;Seed();}

        private void Update()
        {
            if(_controller?.Solver==null||!_controller.Running)return;if(_tracers.Count==0)Seed();
            var solver=_controller.Solver;var dt=Mathf.Min(Time.unscaledDeltaTime,.033f);
            for(var i=0;i<_tracers.Count;i++)
            {
                var t=_tracers[i];t.Previous=t.Position;
                var x=Mathf.Clamp(Mathf.RoundToInt(t.Position.x*(solver.Width-1)),1,solver.Width-2);var y=Mathf.Clamp(Mathf.RoundToInt(t.Position.y*(solver.Height-1)),1,solver.Height-2);var idx=y*solver.Width+x;
                var velocity=new Vector2((float)solver.VelocityX[idx],(float)solver.VelocityY[idx]);t.Position+=velocity*(dt*10.5f);t.Life-=dt;
                if(t.Life<=0||t.Position.x>1.02f||t.Position.x<-.02f||t.Position.y>1.02f||t.Position.y<-.02f||solver.Solid[idx])t=Respawn(i);
                _tracers[i]=t;
            }
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(_controller?.Solver==null||!_controller.Running)return;var rect=rectTransform.rect;
            for(var i=0;i<_tracers.Count;i++)
            {
                var t=_tracers[i];var a=ToLocal(rect,t.Previous);var b=ToLocal(rect,t.Position);var delta=b-a;if(delta.sqrMagnitude<.2f)delta=Vector2.right*3f;var dir=delta.normalized;var normal=new Vector2(-dir.y,dir.x);
                var speed=Mathf.Clamp01(delta.magnitude/14f);var pulse=.78f+.22f*Mathf.Sin(Time.unscaledTime*5f+t.Phase);var width=Mathf.Max(1.6f,rect.height*.0032f)*pulse;var tail=Mathf.Lerp(7f,22f,speed);a=b-dir*tail;
                var c0=new Color32(102,255,226,40);var c1=new Color32(194,255,241,(byte)Mathf.RoundToInt(175+70*pulse));var start=vh.currentVertCount;
                vh.AddVert(a-normal*width*.45f,c0,Vector2.zero);vh.AddVert(a+normal*width*.45f,c0,Vector2.zero);vh.AddVert(b+normal*width,c1,Vector2.zero);vh.AddVert(b-normal*width,c1,Vector2.zero);vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
            }
        }

        private static Vector2 ToLocal(Rect r,Vector2 p)=>new(r.xMin+p.x*r.width,r.yMin+p.y*r.height);
        private void Seed(){_tracers.Clear();for(var i=0;i<_targetCount;i++)_tracers.Add(Respawn(i,true));SetVerticesDirty();}

        private Tracer Respawn(int index,bool distributed=false)
        {
            var lane=Mathf.Repeat(index*.6180339f,1f);
            Vector2 p;
            if(_controller?.PipePath!=null)
            {
                var center=_controller.PipePath.Sample(0f);
                var usableRadius=_controller.PipePath.Radius*.78f;
                var y=center.y+Mathf.Lerp(-usableRadius,usableRadius,lane);
                var x=distributed?Mathf.Lerp(.012f,.11f,Mathf.Repeat(index*.381966f,1f)):.015f;
                p=new Vector2(x,Mathf.Clamp01(y));
            }
            else
            {
                p=new Vector2(distributed?Mathf.Repeat(index/(float)Mathf.Max(1,_targetCount)+lane*.15f,.95f):.015f,.06f+lane*.88f);
            }
            return new Tracer{Position=p,Previous=p-new Vector2(.012f,0),Life=3.5f+lane*4f,Phase=lane*6.28318f};
        }
    }
}
