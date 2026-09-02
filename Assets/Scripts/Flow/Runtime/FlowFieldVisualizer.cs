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
                switch(viewMode)
                {
                    case FlowViewMode.Pressure:_pixels[i]=Diverging(.5f+.5f*densityDelta);break;
                    case FlowViewMode.Vorticity:_pixels[i]=Diverging(.5f+.5f*vorticity);break;
                    case FlowViewMode.Velocity:_pixels[i]=Sequential(speed);break;
                    default:_pixels[i]=FlowColor(speed,(float)uy[i],x/(float)Mathf.Max(1,solver.Width-1));break;
                }
            }
            _texture.SetPixels32(_pixels);_texture.Apply(false,false);
        }

        private static Color32 FlowColor(float speed,float uy,float x)
        {
            var directionBias=Mathf.Clamp01(.5f+4f*uy);
            var depth=.04f*Mathf.Sin(x*6.28318f);
            var baseColor=Color.Lerp(new Color(.018f,.10f,.17f),new Color(.025f,.62f,.72f),Mathf.Clamp01(speed*.9f+.1f));
            baseColor+=new Color(depth*.3f,depth*.7f,depth,0f);
            return Color.Lerp(baseColor,new Color(.60f,1f,.83f),Mathf.Clamp01(speed*directionBias*.32f));
        }
        private static Color32 Sequential(float t){t=Mathf.Clamp01(t);return Color.Lerp(new Color(.018f,.08f,.15f),new Color(.10f,.88f,.68f),t);}
        private static Color32 Diverging(float t){t=Mathf.Clamp01(t);return t<.5f?Color.Lerp(new Color(.08f,.28f,.68f),new Color(.84f,.92f,.94f),t*2f):Color.Lerp(new Color(.84f,.92f,.94f),new Color(.90f,.24f,.24f),(t-.5f)*2f);}
    }
}
