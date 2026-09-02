using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Runtime
{
    public enum FlowViewMode { Flow, Velocity, Pressure, Vorticity }

    public sealed class FlowFieldVisualizer : MonoBehaviour
    {
        [SerializeField] private FlowLabRuntimeController controller;
        [SerializeField] private RawImage target;
        [SerializeField] private FlowViewMode viewMode = FlowViewMode.Flow;
        private Texture2D _texture;
        private Color32[] _pixels;

        public FlowViewMode ViewMode => viewMode;

        public void Configure(FlowLabRuntimeController runtimeController, RawImage rawImage) { controller = runtimeController; target = rawImage; }
        private void Start() { if (controller == null) controller = GetComponent<FlowLabRuntimeController>(); BuildTexture(); controller.SolverUpdated += Render; Render(); }
        private void OnDestroy() { if (controller != null) controller.SolverUpdated -= Render; }
        public void SetViewMode(FlowViewMode mode) { viewMode = mode; Render(); }
        public void CycleViewMode() { viewMode = (FlowViewMode)(((int)viewMode + 1) % 4); Render(); }

        private void BuildTexture()
        {
            var solver=controller.Solver;
            _texture=new Texture2D(solver.Width,solver.Height,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            _pixels=new Color32[solver.Width*solver.Height]; if(target!=null) target.texture=_texture;
        }

        private void Render()
        {
            if(controller==null||controller.Solver==null||_texture==null)return;
            var solver=controller.Solver; var rho=solver.Density; var ux=solver.VelocityX; var uy=solver.VelocityY;
            for(var y=0;y<solver.Height;y++) for(var x=0;x<solver.Width;x++)
            {
                var i=y*solver.Width+x;
                var speed=Mathf.Clamp01((float)(System.Math.Sqrt(ux[i]*ux[i]+uy[i]*uy[i])/0.10));
                var densityDelta=Mathf.Clamp((float)((rho[i]-1.0)/0.05),-1f,1f);
                var vorticity=Mathf.Clamp((float)(solver.VorticityAt(x,y)/0.02),-1f,1f);
                var nx=x/(float)Mathf.Max(1,solver.Width-1);var ny=y/(float)Mathf.Max(1,solver.Height-1);
                switch(viewMode)
                {
                    case FlowViewMode.Pressure:_pixels[i]=Diverging(.5f+.5f*densityDelta,nx,ny);break;
                    case FlowViewMode.Vorticity:_pixels[i]=Diverging(.5f+.5f*vorticity,nx,ny);break;
                    case FlowViewMode.Velocity:_pixels[i]=Sequential(speed,nx,ny);break;
                    default:_pixels[i]=FlowColor(speed,(float)uy[i],vorticity,nx,ny);break;
                }
            }
            _texture.SetPixels32(_pixels);_texture.Apply(false,false);
        }

        private static Color32 FlowColor(float speed,float uy,float vorticity,float x,float y)
        {
            // A subtle spatial material gives the chamber depth even when paused. Physics-driven
            // speed/vorticity then supplies the high-contrast information during a run.
            var vertical=1f-Mathf.Abs(y-.5f)*2f;
            var edge=Mathf.Clamp01(Mathf.Min(Mathf.Min(x,1f-x),Mathf.Min(y,1f-y))*7f);
            var material=.025f*vertical+.025f*edge+.012f*Mathf.Sin((x*.7f+y)*7f);
            var slow=new Color(.015f,.085f,.13f);
            var fast=new Color(.015f,.58f,.67f);
            var baseColor=Color.Lerp(slow,fast,Mathf.Pow(Mathf.Clamp01(speed),.72f));
            baseColor+=new Color(material*.22f,material*.65f,material,0f);

            var directionBias=Mathf.Clamp01(.5f+5f*uy);
            var wake=Mathf.Clamp01(Mathf.Abs(vorticity));
            var highlight=Mathf.Clamp01(speed*.36f+directionBias*speed*.14f);
            baseColor=Color.Lerp(baseColor,new Color(.31f,.95f,.80f),highlight);
            baseColor=Color.Lerp(baseColor,new Color(.10f,.30f,.43f),wake*.24f);
            return ClampColor(baseColor);
        }

        private static Color32 Sequential(float t,float x,float y)
        {
            t=Mathf.Clamp01(t);var edge=.04f*Mathf.Clamp01(Mathf.Min(Mathf.Min(x,1f-x),Mathf.Min(y,1f-y))*8f);
            return ClampColor(Color.Lerp(new Color(.015f,.065f,.12f),new Color(.08f,.90f,.68f),Mathf.Pow(t,.75f))+new Color(edge*.2f,edge*.5f,edge*.5f));
        }

        private static Color32 Diverging(float t,float x,float y)
        {
            t=Mathf.Clamp01(t);var c=t<.5f?Color.Lerp(new Color(.055f,.22f,.58f),new Color(.79f,.89f,.91f),t*2f):Color.Lerp(new Color(.79f,.89f,.91f),new Color(.89f,.20f,.22f),(t-.5f)*2f);
            var vignette=1f-.07f*(1f-Mathf.Clamp01(Mathf.Min(Mathf.Min(x,1f-x),Mathf.Min(y,1f-y))*6f));
            c*=vignette;return ClampColor(c);
        }

        private static Color32 ClampColor(Color c)
        {
            c.r=Mathf.Clamp01(c.r);c.g=Mathf.Clamp01(c.g);c.b=Mathf.Clamp01(c.b);c.a=1f;return c;
        }
    }
}
