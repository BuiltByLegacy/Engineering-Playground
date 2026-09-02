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
        private float[] _occupancy;
        private int _lastHash;
        private const int Scale = 8;

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

            BuildSmoothedOccupancy(solver);
            var width=solver.Width*Scale;var height=solver.Height*Scale;
            if(_texture==null||_texture.width!=width||_texture.height!=height)
            {
                _texture=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                _pixels=new Color32[width*height];_target.texture=_texture;
            }

            for(var py=0;py<height;py++)for(var px=0;px<width;px++)
            {
                var gx=(px+.5f)/Scale-.5f;var gy=(py+.5f)/Scale-.5f;
                var x0=Mathf.Clamp(Mathf.FloorToInt(gx),0,solver.Width-1);var y0=Mathf.Clamp(Mathf.FloorToInt(gy),0,solver.Height-1);
                var x1=Mathf.Min(x0+1,solver.Width-1);var y1=Mathf.Min(y0+1,solver.Height-1);
                var tx=Mathf.Clamp01(gx-x0);var ty=Mathf.Clamp01(gy-y0);
                var a00=_occupancy[y0*solver.Width+x0];var a10=_occupancy[y0*solver.Width+x1];var a01=_occupancy[y1*solver.Width+x0];var a11=_occupancy[y1*solver.Width+x1];
                var field=Mathf.Lerp(Mathf.Lerp(a00,a10,tx),Mathf.Lerp(a01,a11,tx),ty);

                // Broad feather then a firm inner body. This visually rounds cell corners while preserving
                // the same solver mask underneath.
                var outer=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.18f,.58f,field));
                var inner=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.42f,.72f,field));
                var edge=Mathf.Clamp01(outer-inner);
                var baseColor=Color.Lerp(new Color32(27,53,69,255),new Color32(10,20,32,255),inner);
                var lit=Color.Lerp(baseColor,new Color32(55,93,105,255),edge*.55f);
                _pixels[py*width+px]=new Color32((byte)(lit.r*255),(byte)(lit.g*255),(byte)(lit.b*255),(byte)Mathf.RoundToInt(outer*255));
            }
            _texture.SetPixels32(_pixels);_texture.Apply(false,false);
        }

        private void BuildSmoothedOccupancy(FlowLbmSolver solver)
        {
            var count=solver.Width*solver.Height;if(_occupancy==null||_occupancy.Length!=count)_occupancy=new float[count];
            for(var y=0;y<solver.Height;y++)for(var x=0;x<solver.Width;x++)
            {
                var sum=0f;var weight=0f;
                for(var oy=-1;oy<=1;oy++)for(var ox=-1;ox<=1;ox++)
                {
                    var sx=Mathf.Clamp(x+ox,0,solver.Width-1);var sy=Mathf.Clamp(y+oy,0,solver.Height-1);
                    var w=(ox==0&&oy==0)?4f:(ox==0||oy==0?2f:1f);
                    if(solver.Solid[sy*solver.Width+sx])sum+=w;weight+=w;
                }
                _occupancy[y*solver.Width+x]=sum/weight;
            }
        }
    }
}
