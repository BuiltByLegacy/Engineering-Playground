using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.UI
{
    /// <summary>Procedural rounded rectangle so production UI does not depend on square default Image sprites.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RoundedPanelGraphic : MaskableGraphic
    {
        [SerializeField] private float radius = EngineeringPlaygroundTheme.RadiusMedium;
        [SerializeField, Range(2, 12)] private int cornerSegments = 6;

        public void SetRadius(float value){radius=Mathf.Max(0,value);SetVerticesDirty();}

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r=rectTransform.rect; var rad=Mathf.Min(radius,Mathf.Min(r.width,r.height)*.5f);
            var center=new Vector2(r.center.x,r.center.y); vh.AddVert(center,color,Vector2.zero); var centerIndex=0;
            var corners=new[]{new Vector2(r.xMax-rad,r.yMax-rad),new Vector2(r.xMin+rad,r.yMax-rad),new Vector2(r.xMin+rad,r.yMin+rad),new Vector2(r.xMax-rad,r.yMin+rad)};
            var starts=new[]{0f,90f,180f,270f}; var ring=1;
            for(var c=0;c<4;c++) for(var s=0;s<=cornerSegments;s++)
            {
                var a=(starts[c]+90f*s/cornerSegments)*Mathf.Deg2Rad; var p=corners[c]+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad; vh.AddVert(p,color,Vector2.zero);
                if(ring>1)vh.AddTriangle(centerIndex,ring-1,ring); ring++;
            }
            vh.AddTriangle(centerIndex,ring-1,1);
        }
    }
}
