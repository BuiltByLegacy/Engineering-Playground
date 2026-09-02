using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.UI
{
    /// <summary>
    /// Lightweight vector icon set for runtime UI. Keeps core game controls crisp on WebGL/mobile
    /// without relying on Unicode glyph support in the temporary runtime font.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ProductionIconGraphic : MaskableGraphic
    {
        public enum Icon { Draw, Erase, Undo, View, Reset, Run, Info }

        [SerializeField] private Icon icon;
        [SerializeField] private float stroke = 3.2f;

        public void Configure(Icon value, Color tint)
        {
            icon = value;
            color = tint;
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var size = Mathf.Min(r.width, r.height);
            var c = r.center;
            var s = size * .34f;
            var w = Mathf.Max(2f, stroke * Mathf.Max(.7f, size / 52f));

            switch (icon)
            {
                case Icon.Draw:
                    AddThickLine(vh, c + new Vector2(-s*.58f,-s*.58f), c + new Vector2(s*.55f,s*.55f), w*1.25f);
                    AddTriangle(vh, c + new Vector2(s*.62f,s*.62f), c + new Vector2(s*.25f,s*.50f), c + new Vector2(s*.50f,s*.25f));
                    AddThickLine(vh, c + new Vector2(-s*.68f,-s*.72f), c + new Vector2(-s*.35f,-s*.72f), w*.75f);
                    break;
                case Icon.Erase:
                    AddRotatedRect(vh, c, new Vector2(s*1.05f,s*.58f), -38f);
                    AddThickLine(vh, c + new Vector2(-s*.25f,s*.10f), c + new Vector2(s*.25f,-s*.10f), w*.6f, EngineeringPlaygroundTheme.SurfaceRaised);
                    break;
                case Icon.Undo:
                    AddArc(vh, c + new Vector2(s*.10f,0), s*.68f, 35f, 210f, w, 12);
                    AddTriangle(vh, c + new Vector2(-s*.72f,s*.08f), c + new Vector2(-s*.28f,s*.40f), c + new Vector2(-s*.26f,-s*.28f));
                    break;
                case Icon.View:
                    AddEye(vh,c,s,w);
                    break;
                case Icon.Reset:
                    AddArc(vh,c,s*.72f,-45f,275f,w,18);
                    AddTriangle(vh,c+new Vector2(-s*.02f,s*.78f),c+new Vector2(s*.45f,s*.66f),c+new Vector2(s*.22f,s*.28f));
                    break;
                case Icon.Run:
                    AddTriangle(vh,c+new Vector2(-s*.34f,-s*.60f),c+new Vector2(-s*.34f,s*.60f),c+new Vector2(s*.62f,0));
                    break;
                case Icon.Info:
                    AddRing(vh,c,s*.70f,w,22);
                    AddThickLine(vh,c+new Vector2(0,-s*.18f),c+new Vector2(0,s*.22f),w);
                    AddDisc(vh,c+new Vector2(0,s*.48f),w*.62f,10);
                    break;
            }
        }

        private void AddEye(VertexHelper vh, Vector2 c, float s, float w)
        {
            const int n=14;
            for(var i=0;i<n;i++)
            {
                var t0=i/(float)n; var t1=(i+1)/(float)n;
                var a0=Mathf.PI*t0; var a1=Mathf.PI*t1;
                var p0=c+new Vector2(Mathf.Cos(a0)*s*.78f,Mathf.Sin(a0)*s*.43f);
                var p1=c+new Vector2(Mathf.Cos(a1)*s*.78f,Mathf.Sin(a1)*s*.43f);
                AddThickLine(vh,p0,p1,w*.78f);
                var q0=c+new Vector2(Mathf.Cos(-a0)*s*.78f,Mathf.Sin(-a0)*s*.43f);
                var q1=c+new Vector2(Mathf.Cos(-a1)*s*.78f,Mathf.Sin(-a1)*s*.43f);
                AddThickLine(vh,q0,q1,w*.78f);
            }
            AddDisc(vh,c,s*.20f,16);
        }

        private void AddArc(VertexHelper vh, Vector2 c, float radius, float startDeg, float endDeg, float width, int segments)
        {
            var prev=c+Direction(startDeg)*radius;
            for(var i=1;i<=segments;i++)
            {
                var a=Mathf.Lerp(startDeg,endDeg,i/(float)segments);
                var next=c+Direction(a)*radius;
                AddThickLine(vh,prev,next,width);
                prev=next;
            }
        }

        private void AddRing(VertexHelper vh, Vector2 c, float radius, float width, int segments)
        {
            var prev=c+Vector2.right*radius;
            for(var i=1;i<=segments;i++)
            {
                var a=360f*i/segments;
                var next=c+Direction(a)*radius;
                AddThickLine(vh,prev,next,width);
                prev=next;
            }
        }

        private static Vector2 Direction(float degrees)
        {
            var a=degrees*Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a),Mathf.Sin(a));
        }

        private void AddDisc(VertexHelper vh, Vector2 c, float radius, int segments)
        {
            var start=vh.currentVertCount;
            vh.AddVert(c,color,Vector2.zero);
            for(var i=0;i<=segments;i++)
            {
                var a=2f*Mathf.PI*i/segments;
                vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);
                if(i>0)vh.AddTriangle(start,start+i,start+i+1);
            }
        }

        private void AddRotatedRect(VertexHelper vh, Vector2 c, Vector2 half, float degrees)
        {
            var a=degrees*Mathf.Deg2Rad; var cs=Mathf.Cos(a); var sn=Mathf.Sin(a);
            Vector2 Rot(Vector2 p)=>new(p.x*cs-p.y*sn,p.x*sn+p.y*cs);
            var p0=c+Rot(new Vector2(-half.x,-half.y));
            var p1=c+Rot(new Vector2(-half.x, half.y));
            var p2=c+Rot(new Vector2( half.x, half.y));
            var p3=c+Rot(new Vector2( half.x,-half.y));
            AddQuad(vh,p0,p1,p2,p3,color);
        }

        private void AddThickLine(VertexHelper vh, Vector2 a, Vector2 b, float width) => AddThickLine(vh,a,b,width,color);

        private void AddThickLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color tint)
        {
            var d=(b-a).normalized; if(d.sqrMagnitude<.01f)d=Vector2.right;
            var n=new Vector2(-d.y,d.x)*width*.5f;
            AddQuad(vh,a-n,a+n,b+n,b-n,tint);
        }

        private void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c)
        {
            var start=vh.currentVertCount;
            vh.AddVert(a,color,Vector2.zero); vh.AddVert(b,color,Vector2.zero); vh.AddVert(c,color,Vector2.zero);
            vh.AddTriangle(start,start+1,start+2);
        }

        private static void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
        {
            var start=vh.currentVertCount;
            vh.AddVert(a,tint,Vector2.zero); vh.AddVert(b,tint,Vector2.zero); vh.AddVert(c,tint,Vector2.zero); vh.AddVert(d,tint,Vector2.zero);
            vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
        }
    }
}
