using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Nicrom.PM 
{
    [CustomEditor(typeof(PaletteModifier))]
    public class PaletteModifier_Editor : Editor {
        /// <summary>
        /// A list of all the reordable lists.  
        /// </summary>
        private List<ReorderableList> reorderableLists = new List<ReorderableList>();
        /// <summary>
        /// List of color cells that are not used by any model.  
        /// </summary>
        private List<Rect> emptyCells = new List<Rect>();
        /// <summary>
        /// List of warning messages.
        /// </summary>
        private List<string> warningMessages = new List<string>();
        /// <summary>
        /// Serialized Property for palettesList list.  
        /// </summary>
        private SerializedProperty palettesList;
        /// <summary>
        /// Color gradient.  
        /// </summary>
        private Gradient colorGradient;
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
        private Vector2[] UVs;
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
        /// <summary>
        /// Min value of a gradient.  
        /// </summary>
        private float minGradVal = 1;
        /// <summary>
        /// Max value of a gradient.  
        /// </summary>
        private float maxGradVal = 3;
        /// <summary>
        /// Width of a palette button.  
        /// </summary>
        private int bWidth = 25;
        /// <summary>
        /// Height of a palette button.
        /// </summary>
        private int bHeight = 14;

        void OnEnable()
        {
            PaletteModifier pMod = (PaletteModifier)target;
            palettesList = serializedObject.FindProperty("palettesList");

            InitializePMData(pMod);
        }

        private void OnDisable()
        {
            PaletteModifier pModifier = (PaletteModifier)target;

            if (pModifier != null)
            {
                ClearSelection(pModifier);

                if (pModifier.texGrid != null && HasTexturePattern(pModifier))
                    SaveTintColotInTextureGrid(pModifier);
            }
        }

        private void OnDestroy()
        {
            PaletteModifier pModifier = (PaletteModifier)target;

            if (pModifier != null)
                ClearSelection(pModifier);
        }

        private void InitializePMData(PaletteModifier pMod)
        {
            if (pMod.GetComponent<MeshFilter>() != null)
                mesh = pMod.GetComponent<MeshFilter>().sharedMesh;

            if (pMod.GetComponent<SkinnedMeshRenderer>() != null)
                mesh = pMod.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            if (mesh == null)
                return;

            UVs = mesh.uv;
            tex = null;

            if (pMod.GetComponent<Renderer>().sharedMaterial.HasProperty(pMod.textureName))
                tex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            if (tex == null)
                return;

            PM_Utils.SetTextureGridReference(pMod, tex);

            if (pMod.texGrid == null)
                return;

            if (pMod.texGrid.originTexAtlas == null)
                pMod.texGrid.GetOriginalTextureColors();

            if (PM_Utils.IsTextureReadable(tex) && PM_Utils.HasSuportedTextureFormat(tex))
            {
                currentTexColors = tex.GetPixels32();

                if (pMod.texGrid.originTexAtlas != null)
                    origTexColors = pMod.texGrid.originTexAtlas.GetPixels32();

                if (pMod.generatePaletteModifierData)
                {
                    GeneratePaletteModifierData(pMod);
                    pMod.generatePaletteModifierData = false;
                }
                else
                {
                    GetCellsColorFromCurrentTexture(pMod);
                }

                if (HasTexturePattern(pMod))
                {
                    SetTextureAndMaterialReferences(pMod);
                }

                serializedObject.Update();
                CreateReorderableLists(pMod);

                minGradVal = pMod.gradientStart;
                maxGradVal = pMod.gradientEnd;
            }
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Modified Inspector");
            PaletteModifier pMod = (PaletteModifier)target;

            if (Event.current.type == EventType.Layout)
                CheckReferenceValues(pMod);

            if (bStyle == null)
                bStyle = new GUIStyle(GUI.skin.button);

            serializedObject.Update();

            if (warningMessages.Count > 0)
                DrawWarningMessages();

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
            CheckForUndoRedo(pMod);
        }

        /// <summary>
        /// Sets all the texture and material references that are used to apply a color tint to a segment of the main texture.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void SetTextureAndMaterialReferences(PaletteModifier pMod)
        {
            texPatternRT = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            texPatternRT.filterMode = tex.filterMode;

            origTexColors = pMod.texGrid.originTexAtlas.GetPixels32();
            origTexture = new Texture2D(tex.width, tex.height, tex.format, false, true);
    
            origTexture.SetPixels32(origTexColors);
            origTexture.Apply();

            texPatternMaterial = new Material(Shader.Find("Hidden/TextureColorTint"));
            texPatternMaterial.SetTexture(pMod.textureName, origTexture);
        }

        /// <summary>
        /// Performs a series of checks to ensure the custom inspector can be drawn.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CheckReferenceValues(PaletteModifier pMod)
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

            if (!PM_Utils.IsTextureReadable(tex))
            {
                warningMessages.Add("The texture " + tex.name + " is not readable. You can make the texture readable in the Texture Import Settings.");
                texCanBeModified = false;
                canDrawInspector = false;
            }

            if (!PM_Utils.HasSuportedTextureFormat(tex))
            {
                warningMessages.Add("Texture format needs to be ARGB32, RGBA32, or RGB24.");
                texCanBeModified = false;
                canDrawInspector = false;
            }

            Texture2D tempTex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            if (tempTex != null && tempTex != tex)
            {
                if (PM_Utils.IsTextureReadable(tempTex))
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
        private void OnMaterialTextureChange(PaletteModifier pMod)
        {
            ClearSelection(pMod);

            Texture2D tempTex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            if (HasTexturePattern(pMod))
                SaveTintColotInTextureGrid(pMod);

            if (PM_Utils.SetTextureGridReference(pMod, tempTex))
            {
                if (pMod.texGrid.originTexAtlas == null)
                    pMod.texGrid.GetOriginalTextureColors();

                tex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;
                currentTexColors = tex.GetPixels32();

                SetTextureAndMaterialReferences(pMod);
                GetCellsColorFromCurrentTexture(pMod);
            }
            else
            {
                pMod.texGrid = null;
            }     
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
        private void CustomInspector(PaletteModifier pMod)
        {
            if (pMod.textureUpdate == TextureUpdate.Auto)
                CheckForColorChanges(pMod, true);
            else
                CheckForColorChanges(pMod, false);

            if (canDrawInspector)
            {
                DrawReorderableLists(pMod);

                GUILayout.BeginHorizontal();
                pMod.selectedToolBar = GUILayout.Toolbar(pMod.selectedToolBar, pMod.toolBarTitles);
                GUILayout.EndHorizontal();

                if (texCanBeModified && pMod.selectedToolBar == 0)
                    DrawTextureTab(pMod);

                if (canDrawInspector && pMod.selectedToolBar == 1)
                {
                    if (pMod.texGrid != null && pMod.flatColorsInInspector > 2)
                        DrawGradientTab(pMod);
                    else
                        EditorGUILayout.HelpBox("Gradient options are only available when there are at least 3 flat colors in the inspector.", MessageType.Info);
                }

                if (pMod.texGrid != null && canDrawInspector && pMod.selectedToolBar == 2)
                    DrawMiscellaneousTab(pMod);
            }
        }

        /// <summary>
        /// Draws the reorderable lists.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void DrawReorderableLists(PaletteModifier pMod)
        {
            if (pMod.texGrid == null)      
                return;      

            if(reorderableLists.Count != pMod.palettesList.Count)          
                CreateReorderableLists(pMod);       

            for (int i = 0; i < reorderableLists.Count; i++)
            {
                if (pMod.palettesList[i].editPaletteName)
                {
                    EditorGUIUtility.labelWidth = 120;
                    pMod.palettesList[i].paletteName = EditorGUILayout.TextField(new GUIContent("Palette Name"), pMod.palettesList[i].paletteName);

                    if (pMod.palettesList[i].isColorListExpanded)
                    {
                        pMod.palettesList[i].elementHeight = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Element Height"),
                            pMod.palettesList[i].elementHeight), 16, 200);
                        pMod.palettesList[i].propFieldHeight = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Color Field Height"),
                            pMod.palettesList[i].propFieldHeight), 14, 200);
                    }
                }

                if (pMod.palettesList[i].isColorListExpanded)
                    reorderableLists[i].DoLayoutList();
                
                if(!pMod.palettesList[i].isColorListExpanded)
                {
                    GUILayout.BeginVertical("RL Header");
                    GUILayout.Space(1);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(6);
                    GUILayout.Label(pMod.palettesList[i].paletteName, EditorStyles.boldLabel);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("RL Background", GUILayout.Height(30));
                    GUILayout.Space(5);

                    int rows = Mathf.CeilToInt((pMod.palettesList[i].cellsList.Count * 25f) / Screen.width);
                    int elementsPerRow = Mathf.CeilToInt(Screen.width / 25f);
                    int count = 0;

                    if (elementsPerRow > pMod.palettesList[i].cellsList.Count)
                        elementsPerRow = pMod.palettesList[i].cellsList.Count;

                    for (int k = 0; k < rows; k++)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 1;
                        EditorGUIUtility.fieldWidth = 1;

                        for (int j = 0; j < elementsPerRow; j++)
                        {
                            if (count > pMod.palettesList[i].cellsList.Count - 1)
                                break;
#if UNITY_2018_1_OR_NEWER
                            pMod.palettesList[i].cellsList[count].currentCellColor = EditorGUILayout.ColorField(new GUIContent(""),
                                pMod.palettesList[i].cellsList[count].currentCellColor, false, true, false, GUILayout.MinWidth(16));
#else
                            pMod.palettesList[i].cellsList[count].currentCellColor = EditorGUILayout.ColorField(new GUIContent(""),
                                pMod.palettesList[i].cellsList[count].currentCellColor, false, true, false, null, GUILayout.MinWidth(16));
#endif
                            count++;
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        if (count > pMod.palettesList[i].cellsList.Count)
                            break;
                    }
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                }

                DrawPaletteButtons(pMod, i);
            }

            if (pMod.colorFieldsInInspector < (pMod.flatColorsOnObject + pMod.texPatternsOnObject))
            {
                int dif = (pMod.flatColorsOnObject + pMod.texPatternsOnObject) - pMod.colorFieldsInInspector;
                GUILayout.Space(-4);
                if(dif == 1)
                    EditorGUILayout.HelpBox(dif + " color is deleted.", MessageType.Info);
                else
                    EditorGUILayout.HelpBox(dif + " colors are deleted.", MessageType.Info);
                GUILayout.Space(10);
            }
        }

        /// <summary>
        /// Draws the palettes buttons.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, these buttons belongs to. </param>
        private void DrawPaletteButtons(PaletteModifier pMod, int i)
        {     
            bool isColorListExpanded = pMod.palettesList[i].isColorListExpanded;
            bStyle.normal.textColor = Color.black;

            if (pMod.palettesList[i].isColorListExpanded)
            {
#if UNITY_2019_3_OR_NEWER
                GUILayout.Space(-20);
#else
                GUILayout.Space(-16);
#endif
            }
            else
            {
#if UNITY_2019_3_OR_NEWER
                GUILayout.Space(-1);
#else
                GUILayout.Space(-4);
#endif
            }

            EditorGUILayout.BeginHorizontal();

            if (isColorListExpanded)
                DrawAddRemoveColorButton(pMod, i, bStyle);

            GUI.enabled = true;
            DrawMoveUpDownPaletteButton(pMod, i, bStyle);

            GUI.enabled = true;
            DrawAddRemovePaletteButton(pMod, i, bStyle);

            
            DrawResetFieldColorButton(pMod, i, bStyle);
            DrawResetPaletteColors(pMod, i, bStyle);
            DrawEditButton(pMod, i);

            DrawExpandCollapseButton(pMod, i, isColorListExpanded);

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws the buttons that add a color or remove a selected color.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, these buttons belongs to. </param>
        /// <param name="style"> Buttons style. </param>
        private void DrawAddRemoveColorButton(PaletteModifier pMod, int i, GUIStyle style)
        {
            if (pMod.colorFieldsInInspector < (pMod.flatColorsOnObject + pMod.texPatternsOnObject))
            {
                style.normal.textColor = Color.black;
                GUI.enabled = true;
            }
            else
            {
                style.normal.textColor = Color.grey;
                GUI.enabled = false;
            }

            if (GUILayout.Button(new GUIContent("+"), style, GUILayout.Width(bWidth - 5), GUILayout.Height(bHeight)))
            {
                if (pMod.colorFieldsInInspector < (pMod.flatColorsOnObject + pMod.texPatternsOnObject))
                {
                    ClearSelection(pMod);
                    GetCellDataFromStorage(pMod, i);
                    serializedObject.Update();
                }
            }

            GUILayout.Space(-5);

            if (IsColorFieldSelected(pMod, i))
            {
                style.normal.textColor = Color.black;
                GUI.enabled = true;
            }
            else
            {
                style.normal.textColor = Color.grey;
                GUI.enabled = false;
            }

            if (GUILayout.Button(new GUIContent("−"), style, GUILayout.Width(bWidth - 5), GUILayout.Height(bHeight)))
            {
                int len = pMod.palettesList[i].cellsList.Count;

                if (len > 1)
                {
                    for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                    {
                        if (pMod.palettesList[i].cellsList[j].isSelected)
                        {            
                            pMod.palettesList[i].cellsList[j].isSelected = false;
                            UpdateTexture(pMod, i, j);
                            AddCellToStorage(pMod, i, j);                      
                            serializedObject.Update();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the buttons that move a palette up and down.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, these buttons belongs to. </param>
        /// <param name="style"> Buttons style. </param>
        private void DrawMoveUpDownPaletteButton(PaletteModifier pMod, int i, GUIStyle style)
        {
            if(pMod.palettesList.Count > 1 && i > 0)
            {
                style.normal.textColor = Color.black;
                GUI.enabled = true;
            }
            else
            {
                style.normal.textColor = Color.grey;
                GUI.enabled = false;
            }

            if (GUILayout.Button(new GUIContent("▲"), style, GUILayout.Width(bWidth), GUILayout.Height(bHeight)))
            {
                if (i > 0)
                {
                    ClearSelection(pMod);

                    Palette palette = pMod.palettesList[i];
                    pMod.palettesList.RemoveAt(i);
                    pMod.palettesList.Insert(i - 1, palette);
                    serializedObject.Update();
                    CreateReorderableLists(pMod);
                }
            }

            GUILayout.Space(-5);

            if (pMod.palettesList.Count > 1 && i < pMod.palettesList.Count - 1)
            {
                style.normal.textColor = Color.black;
                GUI.enabled = true;
            }
            else
            {
                style.normal.textColor = Color.grey;
                GUI.enabled = false;
            }

            if (GUILayout.Button(new GUIContent("▼"), style, GUILayout.Width(bWidth), GUILayout.Height(bHeight)))
            {
                if (i < pMod.palettesList.Count - 1)
                {
                    ClearSelection(pMod);

                    Palette palette = pMod.palettesList[i];
                    pMod.palettesList.RemoveAt(i);
                    pMod.palettesList.Insert(i + 1, palette);
                    serializedObject.Update();
                    CreateReorderableLists(pMod);
                }
            }
        }

        /// <summary>
        /// Draws the buttons that add and remove a palette.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, these buttons belongs to. </param>
        /// <param name="style"> Buttons style. </param>
        private void DrawAddRemovePaletteButton(PaletteModifier pMod, int i, GUIStyle style)
        {
            if (pMod.colorFieldsInInspector < (pMod.flatColorsOnObject + pMod.texPatternsOnObject))
            {
                style.normal.textColor = Color.black;
                GUI.enabled = true;
            }
            else
            {
                style.normal.textColor = Color.grey;
                GUI.enabled = false;
            }

            if (GUILayout.Button(new GUIContent("+"), style, GUILayout.Width(bWidth - 5), GUILayout.Height(bHeight)))
            {
                if (pMod.colorFieldsInInspector < (pMod.flatColorsOnObject + pMod.texPatternsOnObject))
                {
                    int count = pMod.palettesList.Count;
                    int len = pMod.cellStorage.Count;

                    ClearSelection(pMod);
                    pMod.palettesList.Add(new Palette());

                    for (int j = 0; j < len; j++)
                        GetCellDataFromStorage(pMod, count);

                    serializedObject.Update();
                    reorderableLists.Add(CreateReorderableList(pMod, palettesList.arraySize - 1));
                }
            }

            GUILayout.Space(-5);

            if (pMod.palettesList.Count > 1)
            {
                style.normal.textColor = Color.black;
                GUI.enabled = true;
            }
            else
            {
                style.normal.textColor = Color.grey;
                GUI.enabled = false;
            }

            if (GUILayout.Button(new GUIContent("−"), style, GUILayout.Width(bWidth - 5), GUILayout.Height(bHeight)))
            {
                if (pMod.palettesList.Count > 1)
                {
                    ClearSelection(pMod);
                    AddCellsToStorage(pMod, i);
                    serializedObject.Update();
                    CreateReorderableLists(pMod);
                }
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Draws the palette edit button.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, these buttons belongs to. </param>
        private void DrawEditButton(PaletteModifier pMod, int i)
        {
            GUI.enabled = true;

            if (GUILayout.Button(new GUIContent("E"), GUILayout.Width(bWidth), GUILayout.Height(bHeight)))
            {
                if (pMod.palettesList[i].editPaletteName)
                {
                    pMod.palettesList[i].editPaletteName = false;
                    ClearSelection(pMod);
                    CreateReorderableLists(pMod);
                    GUIUtility.hotControl = 0;
                }
                else
                {
                    pMod.palettesList[i].editPaletteName = true;
                }
            }
        }

        /// <summary>
        /// Draws the button that resets the color of a selected field.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, this button belongs to. </param>
        /// <param name="style"> Button style. </param>
        private void DrawResetFieldColorButton(PaletteModifier pMod, int i, GUIStyle style)
        {
            if (IsColorFieldSelected(pMod, i))
            {
                style.normal.textColor = Color.black;
                GUI.enabled = true;
            }
            else
            {
                style.normal.textColor = Color.grey;
                GUI.enabled = false;
            }

            if (GUILayout.Button(new GUIContent("RF"), GUILayout.Width(bWidth), GUILayout.Height(bHeight)))
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if (pMod.palettesList[i].cellsList[j].isSelected)
                    {
                        pMod.palettesList[i].cellsList[j].isSelected = false;
                        UpdateTexture(pMod, i, j);
                        ResetFieldColor(pMod, i, j);
                        break;
                    }
                }
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Resets the color of a field.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list. </param>
        /// <param name="i"> Index of a color field. </param>
        public void ResetFieldColor(PaletteModifier pMod, int i, int j)
        {
            if (pMod.palettesList[i].cellsList[j].isTexture)
            {
                pMod.palettesList[i].cellsList[j].currentCellColor = Color.white;
                pMod.palettesList[i].cellsList[j].previousCellColor = Color.white;
            }
            else
            {
                int x = (int)(pMod.palettesList[i].cellsList[j].gridCell.x + pMod.palettesList[i].cellsList[j].gridCell.width * 0.5f);
                int y = (int)(pMod.palettesList[i].cellsList[j].gridCell.y + pMod.palettesList[i].cellsList[j].gridCell.height * 0.5f);
                Color32 texelColor = pMod.texGrid.originTexAtlas.GetPixel(x, y);

                pMod.palettesList[i].cellsList[j].currentCellColor = texelColor;
                pMod.palettesList[i].cellsList[j].previousCellColor = texelColor;
            }

            UpdateTexture(pMod, i, j);
        }

        /// <summary>
        /// Draws the button that resets the colors of a palette.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, this button belongs to. </param>
        /// <param name="style"> Button style. </param>
        private void DrawResetPaletteColors(PaletteModifier pMod, int i, GUIStyle style)
        {
            GUILayout.Space(-5);
            if (GUILayout.Button(new GUIContent("RP"), GUILayout.Width(bWidth), GUILayout.Height(bHeight)))
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if (pMod.palettesList[i].cellsList[j].isSelected)
                    {
                        pMod.palettesList[i].cellsList[j].isSelected = false;
                        UpdateTexture(pMod, i, j);
                    }

                    ResetFieldColor(pMod, i, j);
                }
            }
        }

        /// <summary>
        /// Draws the palette expand/collapse button.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of the list, these buttons belongs to. </param>
        /// <param name="isColorListCollapsed"> Used to determine if a reorderable list of colors should be drawn. </param>
        private void DrawExpandCollapseButton(PaletteModifier pMod, int i, bool isColorListCollapsed)
        {
            string bLabel = "";

            if (isColorListCollapsed)
                bLabel = "▲";
            else
                bLabel = "▼";

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(bLabel, ""), GUILayout.Width(bWidth), GUILayout.Height(bHeight)))
            {
                if (pMod.palettesList[i].isColorListExpanded)
                {
                    pMod.palettesList[i].isColorListExpanded = false;
                    ClearSelection(pMod);
                }
                else
                {
                    pMod.palettesList[i].isColorListExpanded = true;
                }
            }
        }

        /// <summary>
        /// Draws the gradient options in the custom inspector.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void DrawGradientTab(PaletteModifier pMod)
        {
            GUILayout.Space(-4);

            InspectorBox(10, () =>
            {
                EditorGUIUtility.labelWidth = 100;

                pMod.gradStartColor = EditorGUILayout.ColorField(new GUIContent("Color 1",
                    "Start color of the linear gradient."), pMod.gradStartColor);
                pMod.gradEndColor = EditorGUILayout.ColorField(new GUIContent("Color 2",
                    "End color of the linear gradient."), pMod.gradEndColor);

                pMod.gradientStart = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Gradient Start",
                    "Index of the first color to apply a gradient to."), pMod.gradientStart), 1, pMod.flatColorsInInspector);
                pMod.gradientEnd = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("Gradient End",
                    "Index of the last color to apply a gradient to."), pMod.gradientEnd), 1, pMod.flatColorsInInspector);

                minGradVal = pMod.gradientStart;
                maxGradVal = pMod.gradientEnd;

                EditorGUILayout.MinMaxSlider(ref minGradVal, ref maxGradVal, 1, pMod.flatColorsInInspector);
                pMod.gradientStart = Mathf.Clamp((int)minGradVal, 1, pMod.flatColorsInInspector);
                pMod.gradientEnd = Mathf.Clamp((int)maxGradVal, 1, pMod.flatColorsInInspector);

                if (GUILayout.Button(new GUIContent("Apply Gradient")))
                    ApplyGradient(pMod);         
            });
        }

        /// <summary>
        /// Draws the Texture options in the custom inspector.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void DrawTextureTab(PaletteModifier pMod)
        {
            GUILayout.Space(-4);

            InspectorBox(10, () =>
            {
                EditorGUIUtility.labelWidth = 100;

                if (pMod.texGrid != null && canDrawInspector)
                {
                    pMod.textureUpdate = (TextureUpdate)EditorGUILayout.EnumPopup(new GUIContent("Texture Update", ""), pMod.textureUpdate);

                    GUILayout.Space(5);

                    if (pMod.textureUpdate == TextureUpdate.Manual)
                    {
                        if (GUILayout.Button(new GUIContent("Update Texture")))
                        {
                            CheckForColorChanges(pMod, true);
                        }
                    }
                 
                    if (GUILayout.Button(new GUIContent("Reset Texture Colors")) && EditorUtility.DisplayDialog("Reset Texture Colors",
                        "Are you sure you want to reset the changes you made to the texture atlas. This action can't be undone.", "Yes", "No"))
                    {
                        ResetTextureColors(pMod);
                    }

                    GUILayout.Space(5);

                    if (GUILayout.Button(new GUIContent("Save")) && EditorUtility.DisplayDialog("Save",
      "Are you sure you want to overwrite current texture data. ", "Yes", "No"))
                    {
                        SaveTextureChangesForThisObject(pMod);
                    }

                    if (GUILayout.Button(new GUIContent("Save Texture")) && EditorUtility.DisplayDialog("Save",
                        "Are you sure you want to overwrite current texture data. ", "Yes", "No"))
                    {
                        SaveTextureChanges(pMod, true);
                    }

                    if (GUILayout.Button(new GUIContent("Save Texture As", "")))
                    {
                        absolutePath = EditorUtility.SaveFilePanel("Save As", relativePath, textureName + ".png", "png");

                        if (absolutePath != "")
                            SaveAsNewTexture(pMod);
                    }
                    
                    if (pMod.debug)
                    {
                        GUILayout.Space(5);
                        Texture2D tempTex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

                        if (pMod.texGrid != null)
                            EditorGUILayout.HelpBox("Tex ref: " + tempTex.name, MessageType.None);
                        else
                            EditorGUILayout.HelpBox("Tex is null", MessageType.None);

                        if (pMod.texGrid != null)
                            EditorGUILayout.HelpBox("TG ref: " + pMod.texGrid.name, MessageType.None);
                        else
                            EditorGUILayout.HelpBox("TG is null", MessageType.None);

                        if (pMod.texGrid != null)
                            EditorGUILayout.HelpBox("Orig Tex ID: " + pMod.texGrid.originTexAtlas.GetInstanceID(), MessageType.None);
                        else
                            EditorGUILayout.HelpBox("Orig Tex is null", MessageType.None);
                    }
                }        
            });
        }

        /// <summary>
        /// Draws the Miscellaneous group of options in the custom inspector.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void DrawMiscellaneousTab(PaletteModifier pMod)
        {
            GUILayout.Space(-4);

            InspectorBox(10, () =>
            {
                EditorGUIUtility.labelWidth = 150;

                if (GUILayout.Button(new GUIContent("Break Color Sharing")))
                {
                    absolutePath = EditorUtility.SaveFilePanel("Save Image", relativePath, "NewMesh.asset", "asset");

                    if (absolutePath != "")
                        BreakColorSharing(pMod);
                }

                if (GUILayout.Button(new GUIContent("Rebuild PM Data")) && EditorUtility.DisplayDialog("Rebuild PM Data",
"This action will reset the inspector colors and remove custom palettes. "
+ "Use this option only when you made changes to the 3D model. For example, removed vertices or changed the UV positions.", "Yes", "No"))
                {
                    ClearSelection(pMod);
                    RebuildPMData(pMod);
                }

                if (pMod.palettesList.Count > 1)
                    pMod.colorNumbering = (ColorNumbering)EditorGUILayout.EnumPopup(new GUIContent("Color Numbering",
                        "Determines how the color numbers are displayed in the inspector."), pMod.colorNumbering);

                EditorGUI.BeginChangeCheck();
                pMod.highlightSelectedColor = EditorGUILayout.Toggle(new GUIContent("Highlight Selected Color", "If enabled, highlights in "
                    + "the scene view the colors that are selected in the inspector."), pMod.highlightSelectedColor);

                if (pMod.highlightSelectedColor)
                {
                    pMod.highlightColor = EditorGUILayout.ColorField(new GUIContent("Highlight Color", ""), pMod.highlightColor);
                    pMod.colorBlend = EditorGUILayout.Slider(new GUIContent("Blend Factor", ""), pMod.colorBlend, 0, 1);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    ApplyRemoveHighlightColor(pMod);
                }

                pMod.debug = EditorGUILayout.Toggle(new GUIContent("Debug", ""), pMod.debug);
            });
        }

        /// <summary>
        /// Used to determine if the current object uses texture patterns. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <returns> Returns true if the object has texture patterns, otherwise returns false. </returns>
        public bool HasTexturePattern(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.palettesList.Count; i++)
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if (pMod.palettesList[i].cellsList[j].isTexture)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Used to determine if a reorderable list color field is selected.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list element. </param>
        /// <returns> Returns true if color field is selected, otherwise returns false. </returns>
        private bool IsColorFieldSelected(PaletteModifier pMod, int i)
        {
            if (i > pMod.palettesList.Count - 1)
                return false;

            for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
            {
                if (pMod.palettesList[i].cellsList[j].isSelected)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates Reorderable lists of colors.  
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CreateReorderableLists(PaletteModifier pMod)
        {
            reorderableLists.Clear();

            if (pMod.palettesList.Count == 0)
                return;

            if (pMod.palettesList.Count == 1 && pMod.palettesList[0].cellsList.Count == 0)
                return;

            for (int i = 0; i < pMod.palettesList.Count; i++)    
                reorderableLists.Add(CreateReorderableList(pMod, i));    
        }

        /// <summary>
        /// Creates a Reorderable list of colors.  
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="listIndex"> The index of the list for which a reaordable list is created. </param>
        private ReorderableList CreateReorderableList(PaletteModifier pMod, int listIndex)
        {
            SerializedProperty paletteElement = palettesList.GetArrayElementAtIndex(listIndex);
            ReorderableList rList = new ReorderableList(serializedObject, paletteElement.FindPropertyRelative("cellsList"), true, true, false, false);

            rList.elementHeight = pMod.palettesList[listIndex].elementHeight;

            rList.drawHeaderCallback = (Rect rect) =>
            {
                headerText = pMod.palettesList[listIndex].paletteName;
                EditorGUI.LabelField(rect, new GUIContent(headerText), EditorStyles.boldLabel);
            };

            rList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = rList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUIUtility.labelWidth = 70;

                string fieldLabel = "";
                bool isTexture = element.FindPropertyRelative("isTexture").boolValue;
                int flatColorCount = 0;
                int texPatternCount = 0;

                if (pMod.colorNumbering == ColorNumbering.PerPalette)
                {
                    for (int i = 0; i <= index; i++)
                    {
                        if (pMod.palettesList[listIndex].cellsList[i].isTexture)
                            texPatternCount++;
                        else
                            flatColorCount++;
                    }
                }
                else
                {
                    for (int i = 0; i <= listIndex; i++)
                    {
                        int len;

                        if (i < listIndex)
                            len = pMod.palettesList[i].cellsList.Count - 1;
                        else
                            len = index;

                        for (int j = 0; j <= len; j++)
                        {
                            if (pMod.palettesList[i].cellsList[j].isTexture)
                                texPatternCount++;
                            else
                                flatColorCount++;
                        }
                    }
                }

                if (isTexture)
                    fieldLabel = "Tex " + texPatternCount;
                else
                    fieldLabel = "Color " + flatColorCount;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, pMod.palettesList[listIndex].propFieldHeight),
                    element.FindPropertyRelative("currentCellColor"), new GUIContent(fieldLabel));

                if (isFocused)
                    pMod.palettesList[listIndex].cellsList[index].isSelected = true;
                else
                    pMod.palettesList[listIndex].cellsList[index].isSelected = false;
            };

            return rList;
        }

        /// <summary>
        /// Makes sure none of the inspector colors are marked as selected.  
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void ClearSelection(PaletteModifier pMod)
        {
            if (pMod != null)
            {
                for (int i = 0; i < pMod.palettesList.Count; i++)
                {
                    for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                    {
                        if (pMod.palettesList[i].cellsList[j].isSelected)
                        {
                            pMod.palettesList[i].cellsList[j].isSelected = false;
                            UpdateTexture(pMod, i, j);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a CellData item to a storage list.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        /// <param name="j"> Index of a list item. </param>
        private void AddCellToStorage(PaletteModifier pMod, int i, int j)
        {
            pMod.cellStorage.Add(new CellData(pMod.palettesList[i].cellsList[j].currentCellColor,
                pMod.palettesList[i].cellsList[j].gridCell, pMod.palettesList[i].cellsList[j].isTexture));
            
            pMod.colorFieldsInInspector--;

            if (!pMod.palettesList[i].cellsList[j].isTexture)
                pMod.flatColorsInInspector--;

            pMod.palettesList[i].cellsList.RemoveAt(j);
        }

        /// <summary>
        /// Adds a group of CellData items to a storage list.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        private void AddCellsToStorage(PaletteModifier pMod, int i)
        {
            int len = pMod.palettesList[i].cellsList.Count;

            for (int j = 0; j < len; j++)
            {
                pMod.cellStorage.Add(new CellData(pMod.palettesList[i].cellsList[j].currentCellColor,
                    pMod.palettesList[i].cellsList[j].gridCell, pMod.palettesList[i].cellsList[j].isTexture));

                pMod.colorFieldsInInspector--;

                if (!pMod.palettesList[i].cellsList[j].isTexture)
                    pMod.flatColorsInInspector--;
            }

            pMod.palettesList.RemoveAt(i);
        }

        /// <summary>
        /// Gets a CellData item from a storage list.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        private void GetCellDataFromStorage(PaletteModifier pMod, int i)
        {
            int lastItemIndex = pMod.cellStorage.Count - 1;

            pMod.palettesList[i].cellsList.Add(new CellData(pMod.cellStorage[lastItemIndex].currentCellColor, 
                pMod.cellStorage[lastItemIndex].gridCell, pMod.cellStorage[lastItemIndex].isTexture));

            pMod.colorFieldsInInspector++;

            if (!pMod.cellStorage[lastItemIndex].isTexture)
                pMod.flatColorsInInspector++;

            pMod.cellStorage.RemoveAt(lastItemIndex);
        }

        /// <summary>
        /// Checks for color changes and updates the texture colors that must be updated.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="isAuto"> Used to determine if texture update mode is set to auto. </param>
        private void CheckForColorChanges(PaletteModifier pMod, bool isAuto)
        {
            int len = pMod.palettesList.Count;

            for (int i = 0; i < len; i++)
            {
                int len2 = pMod.palettesList[i].cellsList.Count;

                for (int j = 0; j < len2; j++)
                {
                    if (isAuto)
                    {
                        if(!PM_Utils.AreColorsEqual(pMod.palettesList[i].cellsList[j].currentCellColor, pMod.palettesList[i].cellsList[j].previousCellColor))
                        {
                            UpdateTexture(pMod, i, j);
                            pMod.palettesList[i].cellsList[j].previousCellColor = pMod.palettesList[i].cellsList[j].currentCellColor;
                        }
                    }

                    if (pMod.highlightSelectedColor && pMod.palettesList[i].cellsList[j].isSelected && !pMod.palettesList[i].cellsList[j].highlightColorApplied)
                    {
                        UpdateTexture(pMod, i, j);
                    }

                    if (!pMod.palettesList[i].cellsList[j].isSelected && pMod.palettesList[i].cellsList[j].highlightColorApplied)
                    {
                        UpdateTexture(pMod, i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Applies or removes highlight color from the texture. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void ApplyRemoveHighlightColor(PaletteModifier pMod)
        {
            int len = pMod.palettesList.Count;

            for (int i = 0; i < len; i++)
            {
                int len2 = pMod.palettesList[i].cellsList.Count;

                for (int j = 0; j < len2; j++)
                {
                    if (pMod.palettesList[i].cellsList[j].isSelected)
                        UpdateTexture(pMod, i, j);
                }
            }
        }

        /// <summary>
        /// Checks whether and undo or redo was performed. Updates the texture colors if one was performed. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CheckForUndoRedo(PaletteModifier pMod)
        {
            if (Event.current.type == EventType.ValidateCommand)
            {
                switch (Event.current.commandName)
                {
                    case "UndoRedoPerformed":
                    {
                        CreateReorderableLists(pMod);

                        for (int i = 0; i < pMod.palettesList.Count; i++)
                        {
                            for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                            {
                                 UpdateTexture(pMod, i, j);
                            }
                        }
                        serializedObject.Update();
                        Repaint();
 
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the data necessary to create a linear gradient.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void PopulateGradientData(PaletteModifier pMod)
        {
            colorGradient = new Gradient();

            colorKey = new GradientColorKey[2];
            alphaKey = new GradientAlphaKey[2];

            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            colorKey[0].color = pMod.gradStartColor;
            colorKey[0].time = 0.0f;
            colorKey[1].color = pMod.gradEndColor;
            colorKey[1].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)     
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 1.0f;
            alphaKey[1].time = 1.0f;

            colorGradient.SetKeys(colorKey, alphaKey);
        }

        /// <summary>
        /// Applies a linear gradient to a range of colors.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void ApplyGradient(PaletteModifier pMod)
        {
            Color color;

            int min = (pMod.gradientStart < pMod.gradientEnd ? pMod.gradientStart : pMod.gradientEnd);
            int max = pMod.gradientEnd > pMod.gradientStart ? pMod.gradientEnd : pMod.gradientStart;

            float d = 1f / Mathf.Abs(max - min);

            PopulateGradientData(pMod);

            int len = pMod.palettesList.Count;
            int count = 1;
            int count2 = 0;

            for (int i = 0; i < len; i++)
            {
                int len2 = pMod.palettesList[i].cellsList.Count;

                for (int j = 0; j < len2; j++)
                {
                    if (!pMod.palettesList[i].cellsList[j].isTexture)
                    {
                        if (count >= min && count <= max)
                        {
                            float time = d * count2;
                            count2++;
                            color = colorGradient.Evaluate(time);
                            pMod.palettesList[i].cellsList[j].currentCellColor = color;
                            pMod.palettesList[i].cellsList[j].previousCellColor = color;

                            UpdateTexture(pMod, i, j);
                        }
                        count++;
                    }

                    if (count > max)
                        break;
                }

                if (count > max)
                    break;
            }
        }

        /// <summary>
        /// Rebuilds all the PM data. The custom palettes are removed and the colors are reset to the current texture values. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void RebuildPMData(PaletteModifier pMod)
        {
            if (pMod.GetComponent<MeshFilter>() != null)
                mesh = pMod.GetComponent<MeshFilter>().sharedMesh;

            if (pMod.GetComponent<SkinnedMeshRenderer>() != null)
                mesh = pMod.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            UVs = mesh.uv;
            currentTexColors = tex.GetPixels32();
            GeneratePaletteModifierData(pMod);
            serializedObject.Update();

            if (HasTexturePattern(pMod))
            {
                SetTextureAndMaterialReferences(pMod);
                GetCellsColorFromCurrentTexture(pMod);
            }

            serializedObject.Update();
            CreateReorderableLists(pMod);
        }

        /// <summary>
        /// Generates all the data necessary for Palette Modifier to work correctly.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GeneratePaletteModifierData(PaletteModifier pMod)
        {
            pMod.palettesList.Clear();
            pMod.cellStorage.Clear();
            pMod.palettesList.Add(new Palette());

            GenerateCellsDataFromCustomGrids(pMod);
            GetCellsColorFromCurrentTexture(pMod);

            pMod.colorFieldsInInspector = pMod.palettesList[0].cellsList.Count;
            pMod.flatColorsOnObject = 0;
            pMod.texPatternsOnObject = 0;

            for (int i = 0; i < pMod.palettesList[0].cellsList.Count; i++)
            {
                if (pMod.palettesList[0].cellsList[i].isTexture)
                    pMod.texPatternsOnObject++;
                else
                    pMod.flatColorsOnObject++;
            }

            pMod.flatColorsInInspector = pMod.flatColorsOnObject;
        }

        /// <summary>
        /// Generates all the data necessary for Palette Modifier to work correctly.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GenerateCellsDataFromCustomGrids(PaletteModifier pMod)
        {
            Vector2Int texelCoord;
            Rect cell;
            bool colorFound = false;
            bool isPointInsideCG;
            int len = UVs.Length;
            int x, y, len2;
            int count = 0;

            if (pMod.texGrid == null || tex.width != pMod.texGrid.texAtlas.width || tex.height != pMod.texGrid.texAtlas.height)
                return;

            for (int i = 0; i < len; i++)
            {
                Vector2 UV =  Vector2.zero;

                if (UVs[i].y < 0)
                    continue;

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

                texelCoord = new Vector2Int(Mathf.CeilToInt(UV.x * tex.width - 1), Mathf.CeilToInt(UV.y * tex.height - 1));
                len2 = pMod.palettesList.Count;

                for (int j = 0; j < len2; j++)
                {
                    for (int k = 0; k < pMod.palettesList[j].cellsList.Count; k++)
                    {
                        if (PM_Utils.PointInsideRect(pMod.palettesList[j].cellsList[k].gridCell, texelCoord))
                        {
                            pMod.palettesList[j].cellsList[k].uvIndex.Add(i);
                            colorFound = true;
                            count++;
                            break;
                        }
                    }

                    if (colorFound)
                        break;
                }

                if (!colorFound)
                {
                    bool isTexture;
                    cell = PM_Utils.GetCellRect(pMod, texelCoord, out isPointInsideCG, out isTexture);

                    if(isPointInsideCG)
                    {
                        if (isTexture)
                        {
                            pMod.palettesList[0].cellsList.Add(new CellData(Color.white, cell, isTexture));
                        }
                        else
                        {
                            x = (int)(cell.x + cell.width * 0.5f);
                            y = (int)(cell.y + cell.height * 0.5f);

                            Color pixelColor = tex.GetPixel(x, y);
                            pMod.palettesList[0].cellsList.Add(new CellData(pixelColor, cell, isTexture));
                        }

                        int lastIndex = pMod.palettesList[0].cellsList.Count - 1;
                        pMod.palettesList[0].cellsList[lastIndex].uvIndex.Add(i);

                        count++;
                    }           
                }

                colorFound = false;
            }

            if (count < len)
                Debug.LogWarning("Not all the mesh UVs are inside the Texture Grid. Open the Texture Grid Editor and make " 
                    + "sure all flat colors and texture patterns have a Flat Color Grid/Texture Pattern Rect on top of them." 
                    + " Then go to Misc tab and press Rebuild PM Data button.");
        }

        /// <summary>
        /// Resets the inspector colors to their original values.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GetCellsColorFromCurrentTexture(PaletteModifier pMod)
        {
            int x, y, len;
            len = pMod.palettesList.Count;

            for (int i = 0; i < len; i++)
            {
                int len2 = pMod.palettesList[i].cellsList.Count;

                for (int j = 0; j < len2; j++)
                {            
                    x = (int)(pMod.palettesList[i].cellsList[j].gridCell.x + pMod.palettesList[i].cellsList[j].gridCell.width * 0.5f);
                    y = (int)(pMod.palettesList[i].cellsList[j].gridCell.y + pMod.palettesList[i].cellsList[j].gridCell.height * 0.5f);
                   
                    if (pMod.palettesList[i].cellsList[j].isTexture)
                    { 
                        Color tintColor = GetTintColorFromTextureGrid(pMod, i, j);

                        pMod.palettesList[i].cellsList[j].currentCellColor = tintColor;
                        pMod.palettesList[i].cellsList[j].previousCellColor = tintColor;
                    }
                    else
                    {
                        Color texelColor = tex.GetPixel(x, y);

                        pMod.palettesList[i].cellsList[j].currentCellColor = texelColor;
                        pMod.palettesList[i].cellsList[j].previousCellColor = texelColor;
                    }
                }
            }
        }

        /// <summary>
        /// Restores the tint colors.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        /// <param name="j"> Index of a list item. </param>
        /// <returns> Returns the color stored in a Custom Grid. </returns>
        private Color GetTintColorFromTextureGrid(PaletteModifier pMod, int  i, int j)
        {
            Color tintColor = Color.white;

            for (int k = 0; k < pMod.texGrid.gridsList.Count; k++)
            {
                Rect cellRect = pMod.palettesList[i].cellsList[j].gridCell;
                Vector2Int gridPos = pMod.texGrid.gridsList[k].gridPos;

                if (cellRect.x == gridPos.x && cellRect.y == gridPos.y && cellRect.width == pMod.texGrid.gridsList[k].gridWidth && cellRect.height == pMod.texGrid.gridsList[k].gridHeight)
                {
                    tintColor = pMod.texGrid.gridsList[k].tintColor;
                }
            }

            return tintColor;
        }

        /// <summary>
        /// Stores the tint colors in the Texture Grid asset Custom Grids.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void SaveTintColotInTextureGrid(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.palettesList.Count; i++)
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if(pMod.palettesList[i].cellsList[j].isTexture)
                    {
                        for (int k = 0; k < pMod.texGrid.gridsList.Count; k++)
                        {
                            Rect cellRect = pMod.palettesList[i].cellsList[j].gridCell;
                            Vector2Int gridPos = pMod.texGrid.gridsList[k].gridPos;

                            if (cellRect.x == gridPos.x && cellRect.y == gridPos.y && cellRect.width == pMod.texGrid.gridsList[k].gridWidth && cellRect.height == pMod.texGrid.gridsList[k].gridHeight)
                            {
                                pMod.texGrid.gridsList[k].tintColor = pMod.palettesList[i].cellsList[j].currentCellColor;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets all the saved tint colors to white.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void ClearTintColorInTextureGrid(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.texGrid.gridsList.Count; i++)
            {
                if (pMod.texGrid.gridsList[i].isTexPattern)
                    pMod.texGrid.gridsList[i].tintColor = Color.white;
            }
        }

        /// <summary>
        /// Changes the colors of a texture based on the values passed.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        /// <param name="j"> Index of a list item. </param>
        private void UpdateTexture(PaletteModifier pMod, int i, int j)
        {
            Color32 highlightColor = Color.yellow;

            if (pMod.highlightSelectedColor)
                highlightColor = Color32.Lerp(pMod.palettesList[i].cellsList[j].currentCellColor, pMod.highlightColor, pMod.colorBlend);

            if (pMod.palettesList[i].cellsList[j].isSelected && pMod.highlightSelectedColor)
            {
                UpdateTexPixelColors(pMod, highlightColor, pMod.palettesList[i].cellsList[j].isTexture, i, j);
                pMod.palettesList[i].cellsList[j].highlightColorApplied = true;
            }
            else
            {
                UpdateTexPixelColors(pMod, pMod.palettesList[i].cellsList[j].currentCellColor, pMod.palettesList[i].cellsList[j].isTexture, i, j);
                pMod.palettesList[i].cellsList[j].highlightColorApplied = false;
            }     
        }

        /// <summary>
        /// Updates the colors of the main texture.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        /// <param name="j"> Index of a list item. </param>
        private void UpdateTexPixelColors(PaletteModifier pMod, Color32 color, bool isTexture, int i, int j)
        {
            int minX, minY, maxX, maxY;
            int textHeight = tex.height;

            minX = (int)pMod.palettesList[i].cellsList[j].gridCell.x;
            minY = (int)pMod.palettesList[i].cellsList[j].gridCell.y;
            maxX = (int)(minX + pMod.palettesList[i].cellsList[j].gridCell.width - 1);
            maxY = (int)(minY + pMod.palettesList[i].cellsList[j].gridCell.height - 1);

            if (isTexture)
            {
                int width = (int)(pMod.palettesList[i].cellsList[j].gridCell.width);
                int height = (int)(pMod.palettesList[i].cellsList[j].gridCell.height);

                if ((QualitySettings.activeColorSpace == ColorSpace.Linear))
                    texPatternMaterial.SetFloat("_Linear", 1f);
                else
                    texPatternMaterial.SetFloat("_Linear", 0f);

                texPatternMaterial.SetColor("_TintColor", color);

                RenderTexture.active = texPatternRT;
                GL.Clear(true, true, Color.black);
                Graphics.Blit(origTexture, texPatternRT, texPatternMaterial);
                tex.ReadPixels(new Rect(minX, tex.height - maxY - 1, width, height), minX, minY);
                currentTexColors = tex.GetPixels32();
                RenderTexture.active = null;
            }
            else
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        currentTexColors[textHeight * y + x] = color;
                    }
                }
                tex.SetPixels32(currentTexColors);
            }

            tex.Apply();
        }

        /// <summary>
        /// Reimports a texture asset.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void ResetTextureColors(PaletteModifier pMod)
        {
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex), ImportAssetOptions.ForceUpdate);
            tex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            currentTexColors = tex.GetPixels32();
            pMod.texGrid.originTexAtlas.SetPixels32(currentTexColors);
            pMod.texGrid.originTexAtlas.Apply();
            origTexColors = tex.GetPixels32();

            if (pMod.texPatternsOnObject > 0)
            {
                SetTextureAndMaterialReferences(pMod);       
            }

            ClearTintColorInTextureGrid(pMod);
            GetCellsColorFromCurrentTexture(pMod);

            if(pMod.cellStorage.Count > 0)
            {
                int len = pMod.cellStorage.Count;

                for (int i = 0; i < len; i++)
                {
                    if(pMod.cellStorage[i].isTexture)
                    {
                        pMod.cellStorage[i].currentCellColor = Color.white;
                        pMod.cellStorage[i].previousCellColor = Color.white;
                    }
                    else
                    {
                        pMod.cellStorage[i] = PM_Utils.GetCellColorFromTexture(pMod, pMod.cellStorage[i], tex);
                    }
                }
            }
        }

        /// <summary>
        /// Breaks the color sharing between models. This is achieved by creating a copy 
        /// of the objects mesh and moving the UVs to a unused part of the texture. 
        /// The mesh is then saved as a new asset.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void BreakColorSharing(PaletteModifier pMod)
        {
            bool hasEmptyCells;

            emptyCells.Clear();
            emptyCells = PM_Utils.GetEmptyGridCells(pMod, tex, out hasEmptyCells);

            if (hasEmptyCells)
            {
                ClearSelection(pMod);
                Color32[] tempCurrentColors = tex.GetPixels32();
                currentTexColors = pMod.texGrid.originTexAtlas.GetPixels32();
                tex.SetPixels32(currentTexColors);
                tex.Apply();
                
                OffsetMeshUVs(pMod);
                mesh.uv = UVs;
                SaveMeshAsset();
                pMod.GetComponent<MeshFilter>().sharedMesh = mesh;

                UpdateTextureFlatColors(pMod);
                SaveTextureChanges(pMod, false);

                tex.SetPixels32(tempCurrentColors);
                tex.Apply();

                currentTexColors = tex.GetPixels32();
                UpdateTextureFlatColors(pMod);

                Debug.Log("Break Color Sharing operation is completed. A new mesh was created and added to the current GameObject.");
            }
            else
            {
                Debug.LogWarning("Break Color Sharing operation could not be completed. There is no more empty space on the texture atlas "
                    + "or the Empty Space Color field value is incorrect.");
            }
        }

        /// <summary>
        /// Saves the color changes for this object.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void SaveTextureChangesForThisObject(PaletteModifier pMod)
        {
            ClearSelection(pMod);
            Color32[] tempCurrentColors = tex.GetPixels32();
            currentTexColors = pMod.texGrid.originTexAtlas.GetPixels32();
            tex.SetPixels32(currentTexColors);
            tex.Apply();

            UpdateTextureColors(pMod);
            SaveTextureChanges(pMod, true);

            tex.SetPixels32(tempCurrentColors);
            tex.Apply();

            currentTexColors = tex.GetPixels32();
            UpdateTextureColors(pMod);
        }

        /// <summary>
        /// Updates the texture colors.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void UpdateTextureColors(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.palettesList.Count; i++)
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    UpdateTexture(pMod, i, j);
                }
            }
        }

        /// <summary>
        /// Updates the texture colors for those color fields that point to a flat color.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void UpdateTextureFlatColors(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.palettesList.Count; i++)
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if (!pMod.palettesList[i].cellsList[j].isTexture)
                        UpdateTexture(pMod, i, j);
                }
            }
        }

        /// <summary>
        /// Resets the tint colors to their default values. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void ResetTintColorFields(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.palettesList.Count; i++)
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if(pMod.palettesList[i].cellsList[j].isTexture)
                    {
                        pMod.palettesList[i].cellsList[j].currentCellColor = Color.white;
                        pMod.palettesList[i].cellsList[j].previousCellColor = Color.white;
                    }
                }
            }
        }

        /// <summary>
        /// Moves the mesh UVs to a part of the texture that is not used by other models. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void OffsetMeshUVs(PaletteModifier pMod)
        {
            int len1, len2, len3;
            int index = 0;

            len1 = pMod.palettesList.Count;

            if (pMod.GetComponent<MeshFilter>() != null)
                mesh = Instantiate(pMod.GetComponent<MeshFilter>().sharedMesh);

            if (pMod.GetComponent<SkinnedMeshRenderer>() != null)
                mesh = Instantiate(pMod.GetComponent<SkinnedMeshRenderer>().sharedMesh);

            UVs = mesh.uv;

            for (int i = 0; i < len1; i++)
            {
                len2 = pMod.palettesList[i].cellsList.Count;

                for (int j = 0; j < len2; j++)
                {
                    if (!pMod.palettesList[i].cellsList[j].isTexture)
                    {
                        pMod.palettesList[i].cellsList[j].gridCell = emptyCells[index];

                        float u = (emptyCells[index].x + emptyCells[index].width * 0.5f) / tex.width;
                        float v = (emptyCells[index].y + emptyCells[index].height * 0.5f) / tex.height;

                        len3 = pMod.palettesList[i].cellsList[j].uvIndex.Count;

                        for (int k = 0; k < len3; k++)
                        {
                            int n = pMod.palettesList[i].cellsList[j].uvIndex[k];
                            UVs[n] = new Vector2(u, v);
                        }

                        index++;
                    }
                }
            }
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
        /// Saves the changes made to the texture.
        /// </summary>
        private void SaveTextureChanges(PaletteModifier pMod, bool clearTinColors)
        {
            ClearSelection(pMod);

            if (clearTinColors)
                pMod.texGrid.ClearTintColors();

            var bytes = tex.EncodeToPNG();
            string path = Application.dataPath + "/../" + AssetDatabase.GetAssetPath(tex);
   
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex), ImportAssetOptions.ForceUpdate);

            OnTextureSave(pMod);
        }

        /// <summary>
        /// Saves texture changes as a new texture.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void SaveAsNewTexture(PaletteModifier pMod)
        {
            Texture2D newTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, true);
            newTex.wrapMode = tex.wrapMode;
            newTex.filterMode =  tex.filterMode;

            ClearSelection(pMod);
            pMod.texGrid.ClearTintColors();     

            newTex.SetPixels32(currentTexColors);
            newTex.Apply();

            var bytes = newTex.EncodeToPNG();
            DestroyImmediate(newTex);

            File.WriteAllBytes(absolutePath, bytes);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex), ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
            tex = (Texture2D)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Texture2D));

            SetTextureImporterOptions(tex);
            pMod.GetComponent<Renderer>().sharedMaterial.SetTexture(pMod.textureName, tex);

            TextureGrid tg = Instantiate(pMod.texGrid);
            tg.ClearTintColors();
            tg.name = "TG_" + tex.name;
            tg.texAtlas = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(tex), typeof(Texture2D));
            tg.originTexAtlas = new Texture2D(tex.width, tex.height, tex.format, true);
            tg.originTexAtlas.SetPixels32(currentTexColors);
            tg.originTexAtlas.Apply();

            relativePath = relativePath.Replace(tex.name + ".png", "");
            relativePath = relativePath + tg.name + ".asset";

            AssetDatabase.CreateAsset(tg, relativePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            pMod.texGrid = (TextureGrid)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(tg), typeof(TextureGrid));

            if (pMod.texPatternsOnObject > 0)
            {
                origTexture.SetPixels32(currentTexColors);
                origTexture.Apply();
            }

            GetCellsColorFromCurrentTexture(pMod);
        }

        /// <summary>
        /// Updates PM internal data after the texture atlas is saved.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void OnTextureSave(PaletteModifier pMod)
        {
            currentTexColors = tex.GetPixels32();
            origTexColors = tex.GetPixels32();

            pMod.texGrid.originTexAtlas = new Texture2D(tex.width, tex.height, tex.format, true);
            pMod.texGrid.originTexAtlas.SetPixels32(currentTexColors);
            pMod.texGrid.originTexAtlas.Apply();

            if (pMod.texPatternsOnObject > 0)
            {     
                origTexture.SetPixels32(origTexColors);
                origTexture.Apply();           
            }

            GetCellsColorFromCurrentTexture(pMod);
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
