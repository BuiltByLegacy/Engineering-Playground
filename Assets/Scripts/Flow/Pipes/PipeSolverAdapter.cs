using EngineeringPlayground.Flow.Simulation;
using UnityEngine;

namespace EngineeringPlayground.Flow.Pipes
{
    public static class PipeSolverAdapter
    {
        public static bool[] BuildSolidMask(D2Q9LbmSolver solver, PipePathModel path)
        {
            var mask = new bool[solver.Width * solver.Height];
            for (var i=0;i<mask.Length;i++) mask[i]=true;

            const int samples = 320;
            var rx = path.Radius * (solver.Width - 1);
            var ry = path.Radius * (solver.Height - 1);
            for (var s=0;s<=samples;s++)
            {
                var p=path.Sample(s/(float)samples);
                var cx=p.x*(solver.Width-1); var cy=p.y*(solver.Height-1);
                var minX=Mathf.Max(1,Mathf.FloorToInt(cx-rx-1)); var maxX=Mathf.Min(solver.Width-2,Mathf.CeilToInt(cx+rx+1));
                var minY=Mathf.Max(1,Mathf.FloorToInt(cy-ry-1)); var maxY=Mathf.Min(solver.Height-2,Mathf.CeilToInt(cy+ry+1));
                for(var y=minY;y<=maxY;y++) for(var x=minX;x<=maxX;x++)
                {
                    var dx=(x-cx)/Mathf.Max(1f,rx); var dy=(y-cy)/Mathf.Max(1f,ry);
                    if(dx*dx+dy*dy<=1f) mask[y*solver.Width+x]=false;
                }
            }

            for(var x=0;x<solver.Width;x++){mask[x]=true;mask[(solver.Height-1)*solver.Width+x]=true;}
            return mask;
        }

        public static void Apply(D2Q9LbmSolver solver, PipePathModel path)
        {
            solver.ApplySolidMask(BuildSolidMask(solver,path), true);
        }
    }
}
