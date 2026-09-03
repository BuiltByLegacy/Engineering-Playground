using EngineeringPlayground.Flow.Simulation;
using UnityEngine;

namespace EngineeringPlayground.Flow.Pipes
{
    public static class PipeSolverAdapter
    {
        private const int Supersample = 3;

        public static bool[] BuildSolidMask(D2Q9LbmSolver solver, PipePathModel path)
        {
            var highW = solver.Width * Supersample;
            var highH = solver.Height * Supersample;
            var fluidHi = new bool[highW * highH];

            // Rasterize the continuous centerline at higher resolution first. Downsampling by
            // coverage gives the LBM mask a much smoother inner wall than drawing directly onto
            // coarse lattice cells.
            const int samples = 720;
            var rx = path.Radius * (highW - 1);
            var ry = path.Radius * (highH - 1);
            for (var s=0;s<=samples;s++)
            {
                var p = path.Sample(s/(float)samples);
                var cx = p.x * (highW - 1); var cy = p.y * (highH - 1);
                var minX = Mathf.Max(0, Mathf.FloorToInt(cx-rx-1)); var maxX = Mathf.Min(highW-1, Mathf.CeilToInt(cx+rx+1));
                var minY = Mathf.Max(0, Mathf.FloorToInt(cy-ry-1)); var maxY = Mathf.Min(highH-1, Mathf.CeilToInt(cy+ry+1));
                for (var y=minY;y<=maxY;y++) for (var x=minX;x<=maxX;x++)
                {
                    var dx=(x-cx)/Mathf.Max(1f,rx); var dy=(y-cy)/Mathf.Max(1f,ry);
                    if(dx*dx+dy*dy<=1f) fluidHi[y*highW+x]=true;
                }
            }

            var mask = new bool[solver.Width * solver.Height];
            var threshold = (Supersample * Supersample + 1) / 2;
            for (var y=0;y<solver.Height;y++) for (var x=0;x<solver.Width;x++)
            {
                var fluidSamples = 0;
                for(var sy=0;sy<Supersample;sy++) for(var sx=0;sx<Supersample;sx++)
                    if(fluidHi[(y*Supersample+sy)*highW + x*Supersample+sx]) fluidSamples++;
                mask[y*solver.Width+x] = fluidSamples < threshold;
            }

            // Keep top/bottom domain edges solid. The pipe itself may reach the inlet/outlet columns;
            // D2Q9 boundary conditions only operate where the generated passage is actually open.
            for(var x=0;x<solver.Width;x++){mask[x]=true;mask[(solver.Height-1)*solver.Width+x]=true;}
            return mask;
        }

        public static void Apply(D2Q9LbmSolver solver, PipePathModel path)
        {
            solver.ApplySolidMask(BuildSolidMask(solver,path), true);
        }
    }
}
