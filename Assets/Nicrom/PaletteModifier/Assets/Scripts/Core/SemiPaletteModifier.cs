using System.Collections.Generic;
using UnityEngine;
using System;

namespace SemiColours
{

    public class SemiPaletteModifier : MonoBehaviour
    {
        /// <summary>
        /// List of cell groups.  
        /// </summary>
        public List<CellData> cellsList = new List<CellData>();
        /// <summary>
        /// Reference to a Texture Grid asset.  
        /// </summary>
        public Nicrom.PM.TextureGrid texGrid;
        /// <summary>
        /// The name of the main texture.  
        /// </summary>
        public string textureName = "_MainTex";
 
    }

    [Serializable]
    public class CellData
    {
        /// <summary>
        /// List of UV indexes that are located inside a grid cell.  
        /// </summary>
        public List<int> uvIndex = new List<int>();

        public Color32 swatchCellColor;
        /// <summary>
        /// Position, width and height of a grid cell.  
        /// </summary>
        public Rect gridCell;  

        public CellData(Color32 colour, Rect cell)
        {
            swatchCellColor = colour;
            gridCell = cell;
        }
    }
}
