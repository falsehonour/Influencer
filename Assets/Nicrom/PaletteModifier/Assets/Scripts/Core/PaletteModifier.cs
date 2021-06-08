using System.Collections.Generic;
using UnityEngine;
using System;

namespace Nicrom.PM
{
    /// <summary>
    /// Color numbering modes.  
    /// </summary>
    public enum ColorNumbering
    {
        /// <summary>
        /// The color numbering is continuous across palettes.  
        /// </summary>
        Continuous,
        /// <summary>
        /// The color numbering is restarted for every palette.  
        /// </summary>
        PerPalette
    }

    /// <summary>
    /// Determines how the texture colors are updated.  
    /// </summary>
    public enum TextureUpdate{
        /// <summary>
        /// The texture colors are updated automatically.
        /// </summary>
        Auto,
        /// <summary>
        /// The texture colors are updated only when the user presses a button.
        /// </summary>
        Manual
    }

    public class PaletteModifier : MonoBehaviour
    {
        public ColorNumbering colorNumbering = ColorNumbering.Continuous;
        public TextureUpdate textureUpdate = TextureUpdate.Auto;
        /// <summary>
        /// List of cell groups.  
        /// </summary>
        public List<Palette> palettesList = new List<Palette>();   
        /// <summary>
        /// Reference to a Texture Grid asset.  
        /// </summary>
        public TextureGrid texGrid;
        /// <summary>
        /// Start color of the gradient.  
        /// </summary>
        public Color32 gradStartColor = Color.white;
        /// <summary>
        /// End color of the gradient.  
        /// </summary>
        public Color32 gradEndColor = Color.grey;
        /// <summary>
        /// The color used to highlight a selected color field.  
        /// </summary>
        public Color32 highlightColor = Color.yellow;
        /// <summary>
        /// Used to determine if the color field that is selected in the inspector should be highlighted in the Editor window.  
        /// </summary>
        public bool highlightSelectedColor = true;
        /// <summary>
        /// Used to determine if some internal data should be drawn in the inspector.  
        /// </summary>
        public bool debug = false;
        /// <summary>
        /// Used to determine if the cells data should be recalculated.  
        /// </summary>
        public bool generatePaletteModifierData = true;
        [Range(0, 1)]
        /// <summary>
        /// The name of the main texture.  
        /// </summary>
        public string textureName = "_MainTex";
        /// <summary>
        /// Blend factor between the highlight color and the color of the texture.  
        /// </summary>
        public float colorBlend = 1.0f;
        /// <summary>
        /// First flat color to apply a gradient to.  
        /// </summary>
        public int gradientStart = 1;
        /// <summary>
        /// Last flat color to apply a gradient to.  
        /// </summary>
        public int gradientEnd = 3;
        /// <summary>
        /// The total number of flat colors the current object uses.  
        /// </summary>
        public int flatColorsOnObject = 0;
        /// <summary>
        /// The total number of texture patterns the current object uses.  
        /// </summary>
        public int texPatternsOnObject = 0;
        /// <summary>
        /// The total number of color fields that are displayed in the inspector.  
        /// </summary>
        public int colorFieldsInInspector = 0;
        /// <summary>
        /// The current number of flat colors that are displayed in the inspector.  
        /// </summary>
        public int flatColorsInInspector = 0;
        /// <summary>
        /// Tool bar titels.  
        /// </summary>
        public string[] toolBarTitles = { "Texture", "Gradient", "Misc", "Colour Swatch" };
        /// <summary>
        /// The ID of the selected tool bar.  
        /// </summary>
        public int selectedToolBar = 0;
        public List<CellData> cellStorage = new List<CellData>();
    }

    [Serializable]
    public class Palette
    {
        /// <summary>
        /// List of CellData instances.  
        /// </summary>
        public List<CellData> cellsList = new List<CellData>();
        /// <summary>
        /// Palette name.  
        /// </summary>
        public string paletteName = "Palette";
        /// <summary>
        /// Used to determine if a palette name should be drawn as a text field in the inspector.  
        /// </summary>
        public bool editPaletteName = false;
        /// <summary>
        /// Used to determine if a list of colors should be drawn collapsed or expanded.
        /// </summary>
        public bool isColorListExpanded = true;
        /// <summary>
        /// The height of a reorderable list element.
        /// </summary>
        public int elementHeight = 25;
        /// <summary>
        /// The height of a color field in the reorderable list.
        /// </summary>
        public int propFieldHeight = 16;
    }

    [Serializable]
    public class CellData
    {
        /// <summary>
        /// List of UV indexes that are located inside a grid cell.  
        /// </summary>
        public List<int> uvIndex = new List<int>();
        /// <summary>
        /// Current texture color of a grid cell.  
        /// </summary>
        public Color32 currentCellColor;
        /// <summary>
        /// Previous texture color of a grid cell.  
        /// </summary>
        public Color32 previousCellColor;

        public Color32 swatchCellColor;

        /// <summary>
        /// Position, width and height of a grid cell.  
        /// </summary>
        public Rect gridCell;
        /// <summary>
        /// Used to determine if the current color is selected in the inspector.  
        /// </summary>
        public bool isSelected = false;
        /// <summary>
        /// Used to determine if a highlight color is applied to the texture.  
        /// </summary>
        public bool highlightColorApplied = false;
        /// <summary>
        /// Used to determine if the current grid cell points to a segment
        /// of the texture atlas that has a texture pattern.  
        /// </summary>
        public bool isTexture = false;

        public CellData(Color32 colour, Rect cell, bool tColour)
        {
            currentCellColor = colour;
            previousCellColor = colour;
            swatchCellColor = colour;
            gridCell = cell;
            isTexture = tColour;
        }
    }
}
