using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace Nicrom.PM {
    public class UVInspector : EditorWindow {
        private const string windowName = "UV Inspector";
        private Material mat;
        private Texture2D texAtlas;
        private Texture2D texVertexPoint_On;
        private Texture2D texVertexPoint_Off;
        private Texture2D texGrid_On;
        private Texture2D texGrid_Off;
        private Texture2D texAtlas_On;
        private Texture2D texAtlas_Off;
        private Texture2D texAxes_On;
        private Texture2D texAxes_Off;
        private Texture2D bTex;
        /// <summary>
        /// List of UVs in canvas space.
        /// </summary>
        private List<Vector2> uvsOnCanvas = new List<Vector2>();
        /// <summary>
        /// Reference to the selected object.
        /// </summary>
        private GameObject selectedObject = null;
        /// <summary>
        /// Reference to the selected objects mesh.
        /// </summary>
        private Mesh objectMesh = null;
        /// <summary>
        /// Tool bar button style.  
        /// </summary>
        private GUIStyle bStyle;
        /// <summary>
        /// Canvas size offset.
        /// </summary>
        private Vector2Int sizeOffset = new Vector2Int(0, 0);
        /// <summary>
        /// The size of the MainTexture used by the selected object.
        /// </summary>
        private Vector2Int texAtlasSize = new Vector2Int(512, 512);
        /// <summary>
        /// Tool bar rect
        /// </summary>
        private Rect toolBarRect;
        /// <summary>
        /// Array of UVs.
        /// </summary>
        private Vector2[] uvs;
        /// <summary>
        /// Mouse position.
        /// </summary>
        private Vector2 mousePos = Vector2.zero;
        /// <summary>
        /// Canvas origin offset.
        /// </summary>
        private Vector2 originOffset = new Vector2(0, 0);
        /// <summary>
        /// Previous window size.
        /// </summary>
        private Vector2 prevWinSize;
        private const float SCROLL_MODIFIER = 1.2f;
        private const float MAX_CANVAS_SCALE_SCROLL = 20f;
        private const float MIN_CANVAS_SCALE = 0.001f;
        private const float MAX_CANVAS_SCALE = 250f;
        private float canvasScale = 1f;
        private string[] uvChannelsStr = new string[] { "UV 1", "UV 2", "UV 3", "UV 4" };
        /// <summary>
        /// Used to determine if the canvas should be centered.
        /// </summary>
        private static bool centerCanvasOnEnable = false;
        /// <summary>
        /// Used to determine if the background grid can be drawn.
        /// </summary>
        private bool canDrawBackgroundGrid = true;
        /// <summary>
        /// Used to determine if the MainTexture used by the selected object can be drawn on top of the canvas.
        /// </summary>
        private bool canDrawTexture = true;
        /// <summary>
        /// Used to determine if the UV points can be drawn.
        /// </summary>
        private bool canDrawVertexPoints = true;
        /// <summary>
        /// Used to determine if the tool bar can be drawn.
        /// </summary>
        private bool canDrawToolbar = true;
        /// <summary>
        /// Used to determine if the X and Y axes can be drawn.
        /// </summary>
        private bool canDrawAxes = true;
        /// <summary>
        /// Used to determine if the selected object has a mesh.
        /// </summary>
        private bool hasMesh = false;
        /// <summary>
        /// Used to determine if the selected objects mesh has UVs.
        /// </summary>
        private bool hasUVs = true;
        /// <summary>
        /// The UV channel that is currently active.
        /// </summary>
        private int activeUVChannel = 0;
        /// <summary>
        /// Padding to add when Fit onScreen is used.
        /// </summary>
        private int canvasPadding = 35;
        /// <summary>
        /// Padding to add to the tool bar buttons.
        /// </summary>
        private int buttonPadding = 4;
        /// <summary>
        /// UV channels.
        /// </summary>
        private int[] uvChannels = new int[] { 0, 1, 2, 3 };
        /// <summary>
        /// Triangles array.
        /// </summary>
        private int[] tris;

        [MenuItem("Window/" + windowName + "")]
        static void Init()
        {
            UVInspector window = (UVInspector)GetWindow(typeof(UVInspector));
            window.titleContent = new GUIContent(windowName);
            window.autoRepaintOnSceneChange = true;
            window.minSize = new Vector2(500, 500);
            centerCanvasOnEnable = true;       
        }

        private void OnEnable()
        {
            prevWinSize = minSize;
            GetMeshData();
            centerCanvasOnEnable = true;

            mat = new Material(Shader.Find("Hidden/Internal-Colored"));

            texVertexPoint_On = Resources.Load("Icons/VertPoints_On", typeof(Texture2D)) as Texture2D;
            texVertexPoint_Off = Resources.Load("Icons/VertPoints_Off", typeof(Texture2D)) as Texture2D;
            texGrid_On = Resources.Load("Icons/Grid_On", typeof(Texture2D)) as Texture2D;
            texGrid_Off = Resources.Load("Icons/Grid_Off", typeof(Texture2D)) as Texture2D;
            texAtlas_On = Resources.Load("Icons/TexIcon_On", typeof(Texture2D)) as Texture2D;
            texAtlas_Off = Resources.Load("Icons/TexIcon_Off", typeof(Texture2D)) as Texture2D;
            texAxes_On = Resources.Load("Icons/Axis_On", typeof(Texture2D)) as Texture2D;
            texAxes_Off = Resources.Load("Icons/Axis_Off", typeof(Texture2D)) as Texture2D;
        }

        private void OnSelectionChange()
        {
            ResetInternalData(true);
        }

        private void OnFocus()
        {
            if(selectedObject != null && selectedObject == Selection.activeGameObject)
                ResetInternalData(false);
            else
                ResetInternalData(true);
        }

        private void OnGUI()
        {
            if (centerCanvasOnEnable)
            {
                FitCanvasOnScreen();
                centerCanvasOnEnable = false;
                prevWinSize = position.size;
            }

            if (bStyle == null)
                bStyle = new GUIStyle(GUI.skin.button);

            GUI.backgroundColor = new Color(.15f, .15f, .15f, .7f);
            GUI.Box(new Rect(0, 0, position.width, position.height), "");
            GUI.backgroundColor = Color.white;

            if (canDrawBackgroundGrid)
                DrawBackgroundGrids();

            if (canDrawTexture)
            {
                EditorGUILayout.BeginVertical();
                DrawTextureAtlas();
                EditorGUILayout.EndVertical();
            }

            if (canDrawAxes)
                DrawAxes();
            DrawCanvasBounds();

            if (hasMesh && hasUVs && Event.current.type == EventType.Repaint)
                DrawUVLines();

            if (canDrawToolbar)
            { 
                toolBarRect = new Rect(buttonPadding, buttonPadding, position.width - buttonPadding * 2, 40);
                DrawToolBar(toolBarRect);
            }

            ProcessEvents(Event.current);

            if (GUI.changed)
                Repaint();
        }

        /// <summary>
        /// Draws the background grids.
        /// </summary>
        private void DrawBackgroundGrids()
        {
            DrawBackgroundGrid(10f, 0.4f, new Color(.12f, .12f, .12f, .7f));
            DrawBackgroundGrid(1f, 0.6f, new Color(.10f, .10f, .10f, .7f));
        }

        /// <summary>
        /// Draws a background grid.
        /// </summary>
        /// <param name="gridSize"> The number of grid lines. </param>
        /// <param name="gridOpacity"> The opacity of the grid lines. </param>
        /// <param name="gridColor"> The color of the grid lines. </param>
        private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
        {
            float scaleX = 1f;
            float scaleY = 1f;

            if (sizeOffset.x != 0)
                scaleX = (sizeOffset.x + texAtlasSize.x) / (float)texAtlasSize.x;

            if (sizeOffset.y != 0)
                scaleY = (sizeOffset.y + texAtlasSize.y) / (float)texAtlasSize.y;

            Vector2 grid = new Vector2((texAtlasSize.x / gridSize) * scaleX, (texAtlasSize.y / gridSize) * scaleY);

            int widthDivs = Mathf.CeilToInt(position.width / grid.x);
            int heightDivs = Mathf.CeilToInt(position.height / grid.y);

            Vector3 newOffset = new Vector3(originOffset.x % grid.x, originOffset.y % grid.y, 0);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            for (int i = 0; i < widthDivs + 2; i++)
            {
                Handles.DrawLine(new Vector3(grid.x * i, -grid.y, 0) + newOffset, new Vector3(grid.x * i + newOffset.x, position.size.y, 0f));
            }

            for (int j = 0; j < heightDivs + 2; j++)
            {
                Handles.DrawLine(new Vector3(-grid.x, grid.y * j, 0) + newOffset, new Vector3(position.width, grid.y * j + newOffset.y, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        /// Draws the texture atlas.
        /// </summary>
        private void DrawTextureAtlas()
        {
            if (texAtlas == null)
                return;

            EditorGUI.DrawPreviewTexture(new Rect(originOffset.x, originOffset.y, texAtlas.width + sizeOffset.x, texAtlas.height + sizeOffset.y), texAtlas);
        }

        /// <summary>
        /// Draws the toolbar.
        /// </summary>
        /// <param name="toolBarRect"> Rect that defines the toolbar position, width and height. </param>
        private void DrawToolBar(Rect toolBarRect)
        {
            GUI.BeginGroup(toolBarRect);

            int t_channel = activeUVChannel;
            Rect rect = new Rect(position.width - 80, 10, 60f, 25f);

            activeUVChannel = EditorGUI.IntPopup(rect, activeUVChannel, uvChannelsStr, uvChannels);
            if (activeUVChannel != t_channel && hasMesh)
                GetUVs();

            bTex = canDrawAxes ? texAxes_On : texAxes_Off;
            rect = new Rect(5, 5, 22f, 22f);
            bStyle.padding = new RectOffset(1, 1, 1, 1);
            if (GUI.Button(rect, new GUIContent("", bTex, "When toggled on, the X and Y axes are drawn."), bStyle))
                canDrawAxes = !canDrawAxes;

            bTex = canDrawBackgroundGrid ? texGrid_On : texGrid_Off;
            rect = new Rect(30, 5, 22f, 22f);
            bStyle.padding = new RectOffset(0, 0, 0, 0);
            if (GUI.Button(rect, new GUIContent("", bTex, "When toggled on, the background grid is drawn."), bStyle))
                canDrawBackgroundGrid = !canDrawBackgroundGrid;

            bTex = canDrawVertexPoints ? texVertexPoint_On : texVertexPoint_Off;
            rect = new Rect(55, 5, 22f, 22f);
            bStyle.padding = new RectOffset(1, 1, 1, 1);
            if (GUI.Button(rect, new GUIContent("", bTex, "When toggled on, the vertex points are drawn."), bStyle))
                canDrawVertexPoints = !canDrawVertexPoints;

            bTex = canDrawTexture ? texAtlas_On : texAtlas_Off;
            rect = new Rect(80, 5, 22f, 22f);
            bStyle.padding = new RectOffset(1, 1, 1, 1);
            if (GUI.Button(rect, new GUIContent("", bTex, "When toggled on, a preview of the MainTexture used by the selected object is drawn on the canvas."), bStyle))
                canDrawTexture = !canDrawTexture;

            GUI.EndGroup();
        }

        /// <summary>
        /// Draws the X and Y axes.
        /// </summary>
        private void DrawAxes()
        {
            Handles.BeginGUI();

            Handles.color = Color.red;
            Handles.DrawLine(
                new Vector3(originOffset.x, originOffset.y + texAtlasSize.y + sizeOffset.y, 0f), 
                new Vector3(originOffset.x + texAtlasSize.x + sizeOffset.x, originOffset.y + texAtlasSize.y + sizeOffset.y, 0f));

            Handles.color = Color.green;
            Handles.DrawLine(new Vector3(originOffset.x, originOffset.y, 0f), new Vector3(originOffset.x, originOffset.y + texAtlasSize.y + sizeOffset.y, 0f));

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        /// Draws the canvas bounds.
        /// </summary>
        private void DrawCanvasBounds()
        {
            Handles.BeginGUI();
            Handles.color = Color.white;

            Handles.DrawLine(new Vector3(originOffset.x + texAtlasSize.x + sizeOffset.x, originOffset.y, 0f), new Vector3(originOffset.x + texAtlasSize.x + sizeOffset.x, originOffset.y + texAtlasSize.y + sizeOffset.y, 0f));
            Handles.DrawLine(new Vector3(originOffset.x, originOffset.y, 0f), new Vector3(originOffset.x + texAtlasSize.x + sizeOffset.x, originOffset.y, 0f));

            if(!canDrawAxes)
            {
                Handles.DrawLine(new Vector3(originOffset.x, originOffset.y, 0f), new Vector3(originOffset.x, originOffset.y + texAtlasSize.y + sizeOffset.y, 0f));
                Handles.DrawLine(new Vector3(originOffset.x, originOffset.y + texAtlasSize.y + sizeOffset.y, 0f), new Vector3(originOffset.x + texAtlasSize.x + sizeOffset.x, originOffset.y + texAtlasSize.y + sizeOffset.y, 0f));
            }
            Handles.EndGUI();
        }

        /// <summary>
        /// Draws the UV lines.
        /// </summary>
        private void DrawUVLines()
        {
            if (uvsOnCanvas.Count == 0)
                return;

            Vector2 p1, p2, p3;

            GL.Begin(GL.LINES);
            GL.Color(Color.white);

            for (int i = 0; i < tris.Length; i += 3)
            {
                p1 = uvsOnCanvas[tris[i]] + originOffset;
                p2 = uvsOnCanvas[tris[i + 1]] + originOffset;
                p3 = uvsOnCanvas[tris[i + 2]] + originOffset;

                GL.Vertex3(p1.x, p1.y, 0f);
                GL.Vertex3(p2.x, p2.y, 0f);

                GL.Vertex3(p2.x, p2.y, 0f);
                GL.Vertex3(p3.x, p3.y, 0f);

                GL.Vertex3(p3.x, p3.y, 0f);
                GL.Vertex3(p1.x, p1.y, 0f);
            }

            GL.End();

            if (canDrawVertexPoints)
            {
                GL.PushMatrix();
                mat.SetPass(0);
                GL.LoadPixelMatrix();
                GL.Viewport(new Rect(0, 0, Screen.width, Screen.height));
                GL.Begin(GL.QUADS);
                GL.Color(Color.white);

                int size = 3;

                for (int i = 0; i < uvsOnCanvas.Count; i++)
                {
                    if (uvsOnCanvas[i].y + originOffset.y > 3) {
                        GL.Vertex3(uvsOnCanvas[i].x + originOffset.x - size, uvsOnCanvas[i].y + originOffset.y - size, 0);
                        GL.Vertex3(uvsOnCanvas[i].x + originOffset.x - size, uvsOnCanvas[i].y + originOffset.y + size, 0);
                        GL.Vertex3(uvsOnCanvas[i].x + originOffset.x + size, uvsOnCanvas[i].y + originOffset.y + size, 0);
                        GL.Vertex3(uvsOnCanvas[i].x + originOffset.x + size, uvsOnCanvas[i].y + originOffset.y - size, 0);
                    }
                }

                GL.End();
                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Processes mouse and keyboard events.
        /// </summary>
        public void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        mousePos = e.mousePosition;
                    }
                    if (e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 2 && e.mousePosition.x > 0)
                    {
                        OnCanvasDrag(e.delta);
                    }

                    if (e.alt && e.button == 0)
                    {
                        OnCanvasDrag(e.mousePosition - mousePos);
                        mousePos = e.mousePosition;
                    }
                    break;

                case EventType.ScrollWheel:
                        OnCanvasZoom(e.delta);
                    break;

                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.KeypadPlus || e.keyCode == KeyCode.Equals)
                    {
                        GUI.FocusControl(null);
                        OnCanvasZoom(new Vector2(0, -3));
                    }

                    if (e.keyCode == KeyCode.KeypadMinus || e.keyCode == KeyCode.Minus)
                    {
                        GUI.FocusControl(null);
                        OnCanvasZoom(new Vector2(0, 3));
                    }
                    break;
            }

            CenterCanvasOnWindowResize(position.size);
        }

        /// <summary>
        /// Centers the canvas when the window is resized.
        /// </summary>
        /// <param name="winSize"> The current size of the window. </param>
        private void CenterCanvasOnWindowResize(Vector2 winSize)
        {
            if (winSize.x != prevWinSize.x || winSize.y != prevWinSize.y)
            {
                originOffset.x = (int)((winSize.x) * 0.5f - (texAtlasSize.x + sizeOffset.x) * 0.5f);
                originOffset.y = (int)((winSize.y - (texAtlasSize.y + sizeOffset.y)) * 0.5f);
                prevWinSize = winSize;
                GUI.changed = true;
            }
        }

        /// <summary>
        /// Processes the context menu actions.
        /// </summary>
        /// <param name="mousePosition"> Current position of the mouse on the screen. </param>
        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Fit on Screen"), false, () => FitCanvasOnScreen());

            genericMenu.AddSeparator("");

            if (activeUVChannel == 0)
                genericMenu.AddItem(new GUIContent("UV Channels/UV 1"), true, () => SetUVChannel(0));
            else
                genericMenu.AddItem(new GUIContent("UV Channels/UV 1"), false, () => SetUVChannel(0));

            if (activeUVChannel == 1)
                genericMenu.AddItem(new GUIContent("UV Channels/UV 2"), true, () => SetUVChannel(1));
            else
                genericMenu.AddItem(new GUIContent("UV Channels/UV 2"), false, () => SetUVChannel(1));

            if (activeUVChannel == 2)
                genericMenu.AddItem(new GUIContent("UV Channels/UV 3"), true, () => SetUVChannel(2));
            else
                genericMenu.AddItem(new GUIContent("UV Channels/UV 3"), false, () => SetUVChannel(2));

            if (activeUVChannel == 3)
                genericMenu.AddItem(new GUIContent("UV Channels/UV 4"), true, () => SetUVChannel(3));
            else
                genericMenu.AddItem(new GUIContent("UV Channels/UV 4"), false, () => SetUVChannel(3));


            genericMenu.AddSeparator("");

            if (canDrawTexture)
                genericMenu.AddItem(new GUIContent("Texture Atlas"), true, () => EnableDisableTexturePreview(false));
            else
                genericMenu.AddItem(new GUIContent("Texture Atlas"), false, () => EnableDisableTexturePreview(true));

            if (canDrawVertexPoints)
                genericMenu.AddItem(new GUIContent("Vertex Points"), true, () => EnableDisableVertexPoints(false));
            else
                genericMenu.AddItem(new GUIContent("Vertex Points"), false, () => EnableDisableVertexPoints(true));

            if (canDrawBackgroundGrid)
                genericMenu.AddItem(new GUIContent("Grid"), true, () => EnableDisableBackgroundGrid(false));
            else
                genericMenu.AddItem(new GUIContent("Grid"), false, () => EnableDisableBackgroundGrid(true));

            if (canDrawAxes)
                genericMenu.AddItem(new GUIContent("Axes"), true, () => EnableDisableAxes(false));
            else
                genericMenu.AddItem(new GUIContent("Axes"), false, () => EnableDisableAxes(true));

            genericMenu.AddSeparator("");

            if (canDrawToolbar)
                genericMenu.AddItem(new GUIContent("Toolbar"), true, () => EnableDisableToolbar(false));
            else
                genericMenu.AddItem(new GUIContent("Toolbar"), false, () => EnableDisableToolbar(true));

            genericMenu.ShowAsContext();
        }

        private void EnableDisableTexturePreview(bool value)
        {
            canDrawTexture = value;
            GUI.changed = true;
        }

        private void EnableDisableVertexPoints(bool value)
        {
            canDrawVertexPoints = value;
            GUI.changed = true;
        }

        private void EnableDisableBackgroundGrid(bool value)
        {
            canDrawBackgroundGrid = value;
            GUI.changed = true;
        }

        private void EnableDisableAxes(bool value)
        {
            canDrawAxes = value;
            GUI.changed = true;
        }

        private void EnableDisableToolbar(bool value)
        {
            canDrawToolbar = value;
            GUI.changed = true;
        }

        /// <summary>
        /// Sets the active UV channel.
        /// </summary>
        private void SetUVChannel(int channel)
        {
            activeUVChannel = channel;
            GetUVs();
        }

        /// <summary>
        /// Calculates the origin offset of the canvas.
        /// </summary>
        /// <param name="delta"> Offset. </param>
        private void OnCanvasDrag(Vector2 delta)
        {
            float offset = 0;
            Vector2 winSize = position.size;

            originOffset += delta;
            offset = (32) - (texAtlasSize.x + sizeOffset.x);

            if (originOffset.x < offset)
                originOffset.x = offset;

            if (originOffset.x > winSize.x - 32)
                originOffset.x = winSize.x - 32;

            offset = -(texAtlasSize.y + sizeOffset.y) + 32;
            if (originOffset.y < offset)
                originOffset.y = offset;

            if (originOffset.y > winSize.y - 32)
                originOffset.y = winSize.y - 32;

            GUI.changed = true;
        }

        /// <summary>
        /// Calculates the size offset of the canvas.
        /// </summary>
        /// <param name="delta"> Offset. </param>
        private void OnCanvasZoom(Vector2 delta)
        {
            Vector2 sOffset = new Vector2Int(0, 0);
            float newScale = 1f;
            float zoom = canvasScale - delta.y * ((canvasScale / MAX_CANVAS_SCALE_SCROLL) * SCROLL_MODIFIER);

            newScale = Mathf.Clamp(zoom, MIN_CANVAS_SCALE, MAX_CANVAS_SCALE);

            sOffset.x = texAtlasSize.x * newScale - texAtlasSize.x;
            sOffset.y = (sOffset.x * texAtlasSize.y) / texAtlasSize.x;

            if ((texAtlasSize.x + sOffset.x) > 64 && (texAtlasSize.y + sOffset.y) > 64)
            {
                canvasScale = newScale;
                originOffset = SetOriginOffsetOnCanvasZoom(sizeOffset.x - sOffset.x);

                sizeOffset.x = Mathf.RoundToInt(sOffset.x);
                sizeOffset.y = Mathf.RoundToInt(sOffset.y);

                if (hasMesh)
                    UVsToCanvas();

                GUI.changed = true;
            }
        }

        /// <summary>
        /// Calculates the origin offset of the canvas.
        /// </summary>
        /// <param name="offset"> Amount by which to change the origin offset value. </param>
        public Vector2 SetOriginOffsetOnCanvasZoom(float offset)
        {
            Vector2 origOffset = Vector2.zero;
            Vector2 mousePosition = Event.current.mousePosition;

            mousePosition = ClampMousePosToCanvas(mousePosition);

            float xScale = (mousePosition.x - originOffset.x) / (texAtlasSize.x + sizeOffset.x);
            float yScale = (mousePosition.y - originOffset.y) / (texAtlasSize.y + sizeOffset.y);

            origOffset = originOffset + new Vector2(offset * xScale, ((offset * texAtlasSize.y) / texAtlasSize.x) * yScale);

            origOffset.x = Mathf.RoundToInt(origOffset.x);
            origOffset.y = Mathf.RoundToInt(origOffset.y);

            return origOffset;
        }

        /// <summary>
        /// Clamps the mouse position to the canvas space.
        /// </summary>
        /// <param name="pos"> Position of a grid. </param>
        public Vector2 ClampMousePosToCanvas(Vector2 pos)
        {
            if (pos.x < originOffset.x)
                pos.x = originOffset.x;

            if (pos.y < originOffset.y)
                pos.y = originOffset.y;

            if (pos.x > (texAtlasSize.x + sizeOffset.x) + originOffset.x)
                pos.x = (texAtlasSize.x + sizeOffset.x) + originOffset.x;

            if (pos.y > (texAtlasSize.y + sizeOffset.y) + originOffset.y)
                pos.y = (texAtlasSize.y + sizeOffset.y) + originOffset.y;

            return pos;
        }

        /// <summary>
        /// Centers the canvas to the current window size.
        /// </summary>
        private void FitCanvasOnScreen()
        {
            Vector2 winSize = position.size;

            if (winSize.x > winSize.y)
            {
                if (texAtlasSize.x > texAtlasSize.y)
                {
                    if (texAtlasSize.x / texAtlasSize.y > (winSize.x) / winSize.y)
                        FitCanvasToXAxis(winSize);
                    else
                        FitCanvasToYAxis(winSize);
                }
                else
                {
                    FitCanvasToYAxis(winSize);
                }
            }
            else
            {
                if (texAtlasSize.x > texAtlasSize.y)
                {
                    FitCanvasToXAxis(winSize);
                }
                else
                {
                    if (texAtlasSize.y / texAtlasSize.x > winSize.y / (winSize.x))
                        FitCanvasToYAxis(winSize);
                    else
                        FitCanvasToXAxis(winSize);
                }
            }

            if (hasMesh)
                UVsToCanvas();
            GUI.changed = true;
        }

        /// <summary>
        /// Resizes the size of the canvas and its position, relative to the Y axis.
        /// </summary>
        /// <param name="winSize"> The current size of the window. </param>
        private void FitCanvasToYAxis(Vector2 winSize)
        {
            int x, y, newWidth, newHeight;

            sizeOffset = new Vector2Int(0, 0);

            newHeight = (int)(winSize.y - canvasPadding * 2);
            newWidth = newHeight * texAtlasSize.x / texAtlasSize.y;

            sizeOffset.x = newWidth - texAtlasSize.x;
            sizeOffset.y = newHeight - texAtlasSize.y;

            x = (int)((winSize.x) * 0.5f - newWidth * 0.5f);
            y = canvasPadding;

            originOffset.x = x;
            originOffset.y = y;

            canvasScale = (texAtlasSize.x + sizeOffset.x) / (float)texAtlasSize.x;
        }

        /// <summary>
        /// Resizes the size of the canvas and its position, relative to the X axis.
        /// </summary>
        /// <param name="winSize"> The current size of the window. </param>
        private void FitCanvasToXAxis(Vector2 winSize)
        {
            int x, y, newWidth, newHeight;

            newWidth = (int)(winSize.x - canvasPadding * 2);
            newHeight = newWidth * texAtlasSize.y / texAtlasSize.x;

            sizeOffset.x = newWidth - texAtlasSize.x;
            sizeOffset.y = newHeight - texAtlasSize.y;

            x = (int)(winSize.x * 0.5f - newWidth * 0.5f);
            y = (int)((winSize.y - newHeight) * 0.5f);

            originOffset.x = x;
            originOffset.y = y;

            canvasScale = (texAtlasSize.x + sizeOffset.x) / (float)texAtlasSize.x;
        }

        /// <summary>
        /// Resets the internal data that is used to draw the UVs.
        /// </summary>
        /// <param name="fitOnScreen"> Should the canvas be scaled and positioned in the center of the window. </param>
        private void ResetInternalData(bool fitOnScreen)
        {
            if (fitOnScreen)
                activeUVChannel = 0;

            GetMeshData();

            if (hasMesh)
                UVsToCanvas();

            if (fitOnScreen)
                FitCanvasOnScreen();

            Repaint();
        }

        /// <summary>
        /// Updates the fields that store the mesh data.
        /// </summary>
        private void GetMeshData()
        {
            selectedObject = Selection.activeGameObject;

            objectMesh = null;
            texAtlas = null;
            uvsOnCanvas.Clear();
            tris = null;
            uvs = null;

            if (selectedObject != null)
            {
                if (selectedObject.GetComponent<SkinnedMeshRenderer>() == null)
                {
                    if (selectedObject.GetComponent<MeshFilter>() != null)
                        objectMesh = selectedObject.GetComponent<MeshFilter>().sharedMesh;
                }
                else
                {
                    if (selectedObject.GetComponent<SkinnedMeshRenderer>() != null)
                        objectMesh = selectedObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                }

                if (objectMesh != null)
                {
                    tris = objectMesh.triangles;
                    GetUVs();
                    hasMesh = true;

                    if (selectedObject.GetComponent<Renderer>() != null && selectedObject.GetComponent<Renderer>().sharedMaterial != null)
                    {
                        if (selectedObject.GetComponent<Renderer>().sharedMaterial.HasProperty("_MainTex"))
                            texAtlas = selectedObject.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
                    }

                    if (texAtlas != null)
                        texAtlasSize = new Vector2Int(texAtlas.width, texAtlas.height);
                }
                else
                {
                    hasMesh = false;
                    hasUVs = false;
                }
            }
            else
            {
                hasMesh = false;
                hasUVs = false;
            }

            if (mat == null)
                mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        }

        /// <summary>
        /// Updates the UVs array.
        /// </summary>
        private void GetUVs()
        {
            uvs = null;
            uvsOnCanvas.Clear();

            if (activeUVChannel == 0)
            {
                if (objectMesh.uv != null)
                    uvs = objectMesh.uv;
            }
            else if (activeUVChannel == 1)
            {
                if (objectMesh.uv2 != null)
                    uvs = objectMesh.uv2;
            }
            else if (activeUVChannel == 2)
            {
                if (objectMesh.uv3 != null)
                    uvs = objectMesh.uv3;
            }
            else
            {
                if (objectMesh.uv4 != null)
                    uvs = objectMesh.uv4;
            }

            if (uvs != null)
            {
                UVsToCanvas();
                hasUVs = true;
            }
            else
                hasUVs = false;
        }

        /// <summary>
        /// Converts UV positions to canvas space positions.
        /// </summary>
        private void UVsToCanvas()
        {
            uvsOnCanvas.Clear();
            Vector2 onCanvas = Vector2.zero;

            for (int i = 0; i < uvs.Length; i++)
            {
                onCanvas.x = uvs[i].x * (texAtlasSize.x + sizeOffset.x);
                onCanvas.y = (1f - uvs[i].y) * (texAtlasSize.y + sizeOffset.y);

                uvsOnCanvas.Add(onCanvas);
            }
        }
    }
}
