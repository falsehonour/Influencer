
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
//using Nicrom.PM;

namespace SemiColours
{
    [CustomEditor(typeof(SemiPaletteModifier))]
    public class SemiPaletteModifier_Editor : Editor
    {
        
        /// <summary>
        /// List of warning messages.
        /// </summary>
        private List<string> warningMessages = new List<string>();
        /// <summary>
        /// The mesh of the current object.  
        /// </summary>
        private Mesh mesh;
        /// <summary>
        /// Reference to the texture atlas used by the current object.  
        /// </summary>
        private Texture2D tex;
        /// <summary>
        /// A material used to change the color of those texture atlas parts that have a texture pattern.   
        /// </summary>
        private Material texPatternMaterial;
        /// <summary>
        /// Instance of a texture that has the same colors the texture atlas had before any changes were made.
        /// </summary>
        private Texture2D origTexture;
        /// <summary>
        /// Render texture used for storing a modified texture.
        /// </summary>
        private RenderTexture texPatternRT;
        /// <summary>
        /// Palette button style.  
        /// </summary>
        private GUIStyle bStyle;
        /// <summary>
        /// An array that stores the current pixel colors of the texture atlas. 
        /// </summary>
        private Color32[] currentTexColors;
        /// <summary>
        /// An array that stores the pixel colors the texture atlas had before any changes were made.
        /// </summary>
        private Color32[] origTexColors;
        /// <summary>
        /// Color key used by a gradient.  
        /// </summary>
        private GradientColorKey[] colorKey;
        /// <summary>
        /// Alpha key used by a gradient.  
        /// </summary>
        private GradientAlphaKey[] alphaKey;
        /// <summary>
        /// An array that stores the current object mesh UVs.  
        /// </summary>
        //private Vector2[] UVs;
        /// <summary>
        /// Used to determine if the texture atlas can be modified. A texture can be modified if it
        /// is readable and has the format ARGB32, RGBA32 or RGB24.  
        /// </summary>
        private bool texCanBeModified = true;
        /// <summary>
        /// Used to determine if the custom inspector can be drawn.
        /// </summary>
        private bool canDrawInspector = true;
        /// <summary>
        /// Used to determine if the custom inspector can be drawn.
        /// </summary>
        private bool drawTexNameOnly = false;
        /// <summary>
        /// Palette name.  
        /// </summary>
        private string headerText = "";
        /// <summary>
        /// Relative path of an asset.  
        /// </summary>
        private string relativePath = "Assets/";
        /// <summary>
        /// Absolute path of an asset.  
        /// </summary>
        private string absolutePath = "";
        /// <summary>
        /// Default name to use when creating a new texture.  
        /// </summary>
        private string textureName = "NewTextureAtlas";

        void OnEnable()
        {
            SemiPaletteModifier pMod = (SemiPaletteModifier)target;
            InitializePMData(pMod);
        }

        private void InitializePMData(SemiPaletteModifier pMod)
        {
            if (pMod.GetComponent<MeshFilter>() != null)
            {
                mesh = pMod.GetComponent<MeshFilter>().sharedMesh;
            }
            else if (pMod.GetComponent<SkinnedMeshRenderer>() != null)
            {
                mesh = pMod.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }

            if (mesh == null)
            {
                return;
            }

            //UVs = mesh.uv;
            tex = null;

            if (pMod.GetComponent<Renderer>().sharedMaterial.HasProperty(pMod.textureName))
            {
                tex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;
            }

            if (tex == null)
            {
                return;
            }

            Utils.SetTextureGridReference(pMod, tex);

            if (pMod.texGrid == null)
            {
                Debug.LogError("No texture grid detected!");
                return;
            }

            if (pMod.texGrid.originTexAtlas == null)
            {
                pMod.texGrid.GetOriginalTextureColors();
            }

            if (Utils.IsTextureReadable(tex) && Utils.HasSuportedTextureFormat(tex))
            {
                currentTexColors = tex.GetPixels32();

                if (pMod.texGrid.originTexAtlas != null)
                {
                    origTexColors = pMod.texGrid.originTexAtlas.GetPixels32();
                }

                if (pMod.generatePaletteModifierData)
                {
                    GeneratePaletteModifierData(pMod);
                    pMod.generatePaletteModifierData = false;
                }
                else
                {
                    //GetCellsColorFromCurrentTexture(pMod);
                }

                serializedObject.Update();
            }
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Modified Inspector");
            SemiPaletteModifier pMod = (SemiPaletteModifier)target;

            if (Event.current.type == EventType.Layout)
            {
                CheckReferenceValues(pMod);
            }

            if (bStyle == null)
            {
                bStyle = new GUIStyle(GUI.skin.button);
            }

            serializedObject.Update();

            if (warningMessages.Count > 0)
            {
                DrawWarningMessages();
            }

            if (drawTexNameOnly)
            {
                pMod.textureName = EditorGUILayout.TextField(new GUIContent("Main Texture Name"), pMod.textureName);

                if (GUILayout.Button(new GUIContent("Initialize PM Data")))
                {
                    InitializePMData(pMod);
                }
            }

            CustomInspector(pMod);

            serializedObject.ApplyModifiedProperties();
        }


        /// <summary>
        /// Performs a series of checks to ensure the custom inspector can be drawn.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CheckReferenceValues(SemiPaletteModifier pMod)
        {
            canDrawInspector = true;
            texCanBeModified = true;
            drawTexNameOnly = false;

            warningMessages.Clear();

            if (mesh == null)
            {
                warningMessages.Add("Object doesn't have a MeshFilter component or a mesh asset assigned to it.");
                canDrawInspector = false;
                texCanBeModified = false;
                return;
            }

            if (tex == null)
            {
                warningMessages.Add("Palette Modifier can't be initialised. Possible solutions to fix this:");
                warningMessages.Add("- Make sure the current GameObject has a material and an albedo texture assigned to it.");
                warningMessages.Add("- If you are using a custom shader, make sure the Texture Name is the same as the property name of main texture in the shader.");

                drawTexNameOnly = true;
                canDrawInspector = false;
                texCanBeModified = false;

                return;
            }

            if (pMod.texGrid == null)
            {
                warningMessages.Add("A Texture Grid asset with a reference to the texture atlas used by the material of this object, was not found. "
                    + "Please create a Texture Grid asset for this texture atlas.");

                canDrawInspector = false;
                return;
            }

            if (!Utils.IsTextureReadable(tex))
            {
                warningMessages.Add("The texture " + tex.name + " is not readable. You can make the texture readable in the Texture Import Settings.");
                texCanBeModified = false;
                canDrawInspector = false;
            }

            if (!Utils.HasSuportedTextureFormat(tex))
            {
                warningMessages.Add("Texture format needs to be ARGB32, RGBA32, or RGB24.");
                texCanBeModified = false;
                canDrawInspector = false;
            }

            Texture2D tempTex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            if (tempTex != null && tempTex != tex)
            {
                if (Utils.IsTextureReadable(tempTex))
                {
                    OnMaterialTextureChange(pMod);
                }
                else
                {
                    warningMessages.Add("The texture " + tempTex.name + " is not readable. You can make the texture readable in the Texture Import Settings.");
                    canDrawInspector = false;
                }
            }
        }

        /// <summary>
        /// Updates different references and array data. Called when the material texture is replaced by the user.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void OnMaterialTextureChange(SemiPaletteModifier pMod)
        {
            Debug.LogError("!");
        }

        /// <summary>
        /// Draws the warning messages.
        /// </summary>
        private void DrawWarningMessages()
        {
            for (int i = 0; i < warningMessages.Count; i++)
            {
                EditorGUILayout.HelpBox(warningMessages[i], MessageType.Warning);
            }
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CustomInspector(SemiPaletteModifier pMod)
        {          
            if (canDrawInspector)
            {
                if (pMod.texGrid != null)
                {
                    DrawColourSwatchTab(pMod);
                }

            }
        }


        private void DrawColourSwatchTab(SemiPaletteModifier pMod)
        {
            GUILayout.Space(-4);

            InspectorBox(10, () =>
            {
                EditorGUIUtility.labelWidth = 150;

                 List<CellData> cellsList = pMod.cellsList;
                 //Palette palette = pMod.palettesList[i];
                 {
                     int rows = Mathf.CeilToInt((cellsList.Count * 25f) / Screen.width);
                     int elementsPerRow = Mathf.CeilToInt(Screen.width / 25f);
                     int count = 0;

                     if (elementsPerRow > cellsList.Count)
                     {
                         elementsPerRow = cellsList.Count;
                     }

                     for (int k = 0; k < rows; k++)
                     {

                         for (int j = 0; j < elementsPerRow; j++)
                         {
                             if (count > cellsList.Count - 1)
                             {
                                 break;
                             }

                             cellsList[count].swatchCellColor = EditorGUILayout.ColorField(cellsList[count].swatchCellColor);
                             count++;
                         }

                         if (count > cellsList.Count)
                         {
                             break;
                         }
                     }
                 }
                
                if (GUILayout.Button(new GUIContent("Set colours")))
                {
                    absolutePath = EditorUtility.SaveFilePanel("Save Image", relativePath, "NewMesh.asset", "asset");

                    if (absolutePath != "")
                    {
                        BreakAndSetToSwatchColours(pMod);
                    }
                    /*var usedCells =  PM_Utils.GetUsedGridCells(pMod, tex);
                    Debug.Log($"usedCells: {usedCells.Count}");*/
                }
            });
        }
 
        /// <summary>
        /// Generates all the data necessary for Palette Modifier to work correctly.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GeneratePaletteModifierData(SemiPaletteModifier pMod)
        {
            pMod.cellsList.Clear();

            GenerateCellsDataFromCustomGrids(pMod);
            //GetCellsColorFromCurrentTexture(pMod);

            pMod.flatColorsOnObject = pMod.cellsList.Count;

            pMod.flatColorsInInspector = pMod.flatColorsOnObject;
        }

        /// <summary>
        /// Generates all the data necessary for Palette Modifier to work correctly.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GenerateCellsDataFromCustomGrids(SemiPaletteModifier pMod)
        {
            Vector2[] UVs = mesh.uv;

            Vector2Int texelCoord;
            Rect rect;
            bool colorFound = false;
            bool isPointInsideCG;
            int x, y;
            int count = 0;

            if (pMod.texGrid == null || tex.width != pMod.texGrid.texAtlas.width || tex.height != pMod.texGrid.texAtlas.height)
            {
                return;
            }
            int UvsLength = UVs.Length;
            Debug.Log("UVs:" + UvsLength);
            for (int i = 0; i < UvsLength; i++)
            {
                Vector2 UV = Vector2.zero;

                if (UVs[i].y < 0)
                {
                    continue;
                }

                if (Mathf.Abs(UVs[i].x) % 1 == 0)
                {
                    if (Mathf.Abs(UVs[i].x) % 2 == 0)
                        UV.x = 0;
                    else
                        UV.x = 1;
                }
                else
                {
                    UV.x = Mathf.Abs(UVs[i].x) % 1;
                }

                if (Mathf.Abs(UVs[i].y) % 1 == 0)
                {
                    if (Mathf.Abs(UVs[i].y) % 2 == 0)
                        UV.y = 0;
                    else
                        UV.y = 1;
                }
                else
                {
                    UV.y = Mathf.Abs(UVs[i].y) % 1;
                }

                texelCoord = new Vector2Int
                    (Mathf.CeilToInt(UV.x * tex.width - 1), Mathf.CeilToInt(UV.y * tex.height - 1));

                List<CellData> cells = pMod.cellsList;
                for (int j = 0; j < cells.Count; j++)
                {
                    if (Utils.PointInsideRect(cells[j].gridCell, texelCoord))
                    {
                        cells[j].uvIndex.Add(i);
                        colorFound = true;
                        count++;
                        break;
                    }
                }

                if (!colorFound)
                {
                    rect = Utils.GetCellRect(pMod, texelCoord, out isPointInsideCG);

                    if (isPointInsideCG)
                    {

                        {
                            x = (int)(rect.x + rect.width * 0.5f);
                            y = (int)(rect.y + rect.height * 0.5f);

                            Color pixelColor = tex.GetPixel(x, y);
                            cells.Add(new CellData(pixelColor, rect));
                        }

                        int lastIndex = cells.Count - 1;
                        cells[lastIndex].uvIndex.Add(i);

                        count++;
                    }
                }

                colorFound = false;
            }

            if (count < UVs.Length)
            {
                Debug.LogWarning("Not all the mesh UVs are inside the Texture Grid. Open the Texture Grid Editor and make "
    + "sure all flat colors and texture patterns have a Flat Color Grid/Texture Pattern Rect on top of them."
    + " Then go to Misc tab and press Rebuild PM Data button.");
            }

        }
  
        private void BreakAndSetToSwatchColours(SemiPaletteModifier pMod)
        {
            mesh = Instantiate(mesh);
            Vector2[] UVs = OffsetUVsToSwatchColours(pMod);
            mesh.uv = UVs;
            SaveMeshAsset();
            pMod.GetComponent<MeshFilter>().sharedMesh = mesh;

            Debug.Log("Break Color Sharing operation is completed. A new mesh was created and added to the current GameObject.");
        }

        private Vector2[] OffsetUVsToSwatchColours(SemiPaletteModifier pMod)
        {

            /* if (pMod.GetComponent<MeshFilter>() != null)
             {
                 mesh = Instantiate(pMod.GetComponent<MeshFilter>().sharedMesh);
             }

             if (pMod.GetComponent<SkinnedMeshRenderer>() != null)
             {
                 mesh = Instantiate(pMod.GetComponent<SkinnedMeshRenderer>().sharedMesh);
             }*/
            Vector2[] UVs = mesh.uv;
      
            List<CellData> cellsList = pMod.cellsList;
            int cellsListCount = cellsList.Count;
            Debug.Log($"cellsList.Count = {cellsListCount}");//Number of colours on model?

            for (int j = 0; j < cellsListCount; j++)
            {
                CellData cellData = cellsList[j];
               
                //Vector2 uv = PM_Utils.GetBestApproximateUV(pMod, tex, cellData.swatchCellColor);
                Rect newCell = Utils.GetBestApproximateCell(pMod, tex, cellData.swatchCellColor);
                cellData.gridCell = newCell;

                float u = ((newCell.x + newCell.width * 0.5f) / tex.width);
                float v = ((newCell.y + newCell.height * 0.5f) / tex.height);
                Vector2 uv = new Vector2(u, v);

                int uvIndices = cellData.uvIndex.Count;
                Debug.Log($"cellsList[j].uvIndex.Count = {uvIndices}");

                for (int k = 0; k < uvIndices; k++)
                {
                    int n = cellData.uvIndex[k];
                    UVs[n] = uv;
                }     
            }

            return UVs;
        }

        /// <summary>
        /// Saves a mesh to HDD.  
        /// </summary>
        private void SaveMeshAsset()
        {
            relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
            AssetDatabase.CreateAsset(mesh, relativePath);
        }


        /// <summary>
        /// Sets the texture import options.
        /// </summary>
        /// <param name="texture"> Reference to a texture asset. </param>
        public void SetTextureImporterOptions(Texture2D texture)
        {
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;

                tImporter.isReadable = true;
                tImporter.wrapMode = TextureWrapMode.Clamp;
                tImporter.filterMode = FilterMode.Point;
                tImporter.textureCompression = TextureImporterCompression.Uncompressed;

                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }

        public void InspectorBox(int aBorder, System.Action inside, int aWidthOverride = 0, int aHeightOverride = 0)
        {
            Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(aWidthOverride));
            if (aWidthOverride != 0)
            {
                r.width = aWidthOverride;
            }
            GUI.Box(r, GUIContent.none);
            GUILayout.Space(aBorder);
            if (aHeightOverride != 0)
                EditorGUILayout.BeginVertical(GUILayout.Height(aHeightOverride));
            else
                EditorGUILayout.BeginVertical();
            GUILayout.Space(aBorder);
            inside();
            GUILayout.Space(aBorder);
            EditorGUILayout.EndVertical();
            GUILayout.Space(aBorder);
            EditorGUILayout.EndHorizontal();
        }

        public void BoldFontStyle(System.Action inside)
        {
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            inside();
            style.fontStyle = previousStyle;
        }
    }
}
