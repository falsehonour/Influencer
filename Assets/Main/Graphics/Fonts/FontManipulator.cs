using UnityEditor;
using UnityEngine;
public class FontManipulator : MonoBehaviour
{
    [SerializeField] private Font[] fonts;
    [ContextMenu("Make It Crispy")]
    private void MakeThemCrispy()
    {
        if(fonts != null)
        {
            for (int i = 0; i < fonts.Length; i++)
            {
                Font font = fonts[i];
                if(font != null)
                {
                    font.material.mainTexture.filterMode = FilterMode.Point;
                    font.material.mainTexture.anisoLevel = 0;
                }
            }
        }
        //EditorUtility.SetDirty(font);

    }

    private void Start()
    {
        MakeThemCrispy();
    }

}
