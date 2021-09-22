using UnityEngine;
using UnityEngine.UI;

//NOTE: Gotten from https://answers.unity.com/questions/1074814/is-it-possible-to-skew-or-shear-ui-elements-in-uni.html
public class SkewedImage : Image
{
    //[SerializeField]
    private static float skewX =10;
    //[SerializeField]
    private static float skewY =0;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);


        var height = rectTransform.rect.height;
        var width = rectTransform.rect.width;
        var xskew = height * Mathf.Tan(Mathf.Deg2Rad * skewX);
        var yskew = width * Mathf.Tan(Mathf.Deg2Rad * skewY);

        var ymin = rectTransform.rect.yMin;
        var xmin = rectTransform.rect.xMin;
        UIVertex v = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);
            v.position += new Vector3
                (Mathf.Lerp(0, xskew, (v.position.y - ymin) / height), Mathf.Lerp(0, yskew, (v.position.x - xmin) / width), 0);
            vh.SetUIVertex(v, i);
        }

    }

    /*Olde version:
     * protected override void OnPopulateMesh(VertexHelper vh)
    {       
              base.OnPopulateMesh(vh);
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            Color32 color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x - skewX, v.y - skewY), color32, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(v.x + skewX, v.w - skewY), color32, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(v.z + skewX, v.w + skewY), color32, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(v.z - skewX, v.y + skewY), color32, new Vector2(1f, 0f));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
    }*/
}

