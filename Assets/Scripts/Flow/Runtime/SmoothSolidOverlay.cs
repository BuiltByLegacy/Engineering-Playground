using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Runtime
{
    [RequireComponent(typeof(RawImage))]
    public sealed class SmoothSolidOverlay : MonoBehaviour
    {
        private FlowLabRuntimeController _controller;
        private RawImage _target;
        private Texture2D _texture;
        private Color32[] _pixels;
        private int _lastHash;
        private const int Scale = 4;

        public void Configure(FlowLabRuntimeController controller, RawImage target)
        {
            _controller=controller;_target=target;_target.raycastTarget=false;
        }

        private void Start(){if(_controller!=null)_controller.SolverUpdated+=Refresh;Refresh();}
        private void OnDestroy(){if(_controller!=null)_controller.SolverUpdated-=Refresh;}

        private void Refresh()
        {
            var solver=_controller?.Solver;if(solver==null||_target==null)return;
            var hash=17;for(var i=0;i<solver.Solid.Length;i+=11)hash=hash*31+(solver.Solid[i]?1:0);if(hash==_lastHash&&_texture!=null)return;_lastHash=hash;
            var width=solver.Width*Scale;var height=solver.Height*Scale;
            if(_texture==null||_texture.width!=width||_texture.height!=height){_texture=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};_pixels=new Color32[width*height];_target.texture=_texture;}
            for(var py=0;py<height;py++)for(var px=0;px<width;px++)
            {
                var gx=(px+.5f)/Scale-.5f;var gy=(py+.5f)/Scale-.5f;var x0=Mathf.Clamp(Mathf.FloorToInt(gx),0,solver.Width-1);var y0=Mathf.Clamp(Mathf.FloorToInt(gy),0,solver.Height-1);var x1=Mathf.Min(x0+1,solver.Width-1);var y1=Mathf.Min(y0+1,solver.Height-1);var tx=Mathf.Clamp01(gx-x0);var ty=Mathf.Clamp01(gy-y0);
                var a00=solver.Solid[y0*solver.Width+x0]?1f:0f;var a10=solver.Solid[y0*solver.Width+x1]?1f:0f;var a01=solver.Solid[y1*solver.Width+x0]?1f:0f;var a11=solver.Solid[y1*solver.Width+x1]?1f:0f;var top=Mathf.Lerp(a00,a10,tx);var bottom=Mathf.Lerp(a01,a11,tx);var alpha=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.28f,.72f,Mathf.Lerp(top,bottom,ty)));
                _pixels[py*width+px]=new Color32(14,25,39,(byte)Mathf.RoundToInt(alpha*255));
            }
            _texture.SetPixels32(_pixels);_texture.Apply(false,false);
        }
    }
}
