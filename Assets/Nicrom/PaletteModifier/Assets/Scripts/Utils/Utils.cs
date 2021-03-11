#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using Nicrom.PM;

namespace SemiColours
{
    public static class Utils {
#if UNITY_EDITOR
        /// <summary>
        /// Used to determine if a texture is readable.
        /// </summary>
        /// <param name="texture"> Reference to a texture. </param>
        /// <returns> Returns true if texture is readable, otherwise returns false. </returns>
        public static bool IsTextureReadable(Texture2D texture)
        {
#if UNITY_2018_3_OR_NEWER
          if(texture.isReadable)
                return true;
            else
                return false;
#else
            string texturePath = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);

            if (textureImporter.isReadable)
                return true;
            else
                return false;
#endif
        }

        /// <summary>
        /// Used to determine if a texture has ARGB32, RGBA32, or RGB24 format.
        /// </summary>
        /// <param name="texture"> Reference to a texture. </param>
        /// <returns> Returns true if the texture has ARGB32, RGBA32, or RGB24 format, otherwise returns false. </returns>
        public static bool HasSuportedTextureFormat(Texture2D texture)
        {
            if (texture.format == TextureFormat.ARGB32 || texture.format == TextureFormat.RGBA32 || texture.format == TextureFormat.RGB24)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Calculates the position, width and height of a custom grid cell.
        /// </summary>
        /// <param name="pMod"> Reference to a PaletteModifier script. </param>
        /// <param name="point"> The coordinates of a texture texel. </param>
        /// <param name="isPointInsideCG"> Used to determine if a point in inside a custom grid. </param>
        /// <param name="isTexPattern"> Used to determine if the custom grid points to a texture pattern on the texture atlas. </param>
        /// <returns> Returns the position, width and height of the color cell. </returns>
        public static Rect GetCellRect(SemiPaletteModifier pMod, Vector2Int point, out bool isPointInsideCG)
        {
            isPointInsideCG = false;
            Rect cellRect = new Rect(0, 0, 0, 0);
            int gridOriginX, gridOriginY, width, height;

            for (int i = 0; i < pMod.texGrid.gridsList.Count; i++)
            {
                CustomGrid grid = pMod.texGrid.gridsList[i];
                gridOriginX = grid.gridPos.x;
                gridOriginY = grid.gridPos.y;

                width =  grid.gridWidth;
                height = grid.gridHeight;

                if (PointInsideRect(new Rect(gridOriginX, gridOriginY, width - 1, height - 1), point))
                {
                    if (grid.isTexPattern)
                    {
                        cellRect = new Rect(gridOriginX, gridOriginY, width, height);
                        isPointInsideCG = true;
                        break;
                    }

                    int len = grid.vLinesOnTexGrid.Count;
                    int min, max;

                    for (int j = 0; j < len - 1; j++)
                    {
                        min = gridOriginX + grid.vLinesOnTexGrid[j];
                        max = gridOriginX + grid.vLinesOnTexGrid[j + 1];

                        if (point.x >= min && point.x < max) //&& point.x <= max)
                        {
                            cellRect.x = min;
                            cellRect.width = max - min;
                            break;
                        }
                    }

                    len = grid.hLinesOnTexGrid.Count;

                    for (int j = 0; j < len - 1; j++)
                    {
                        min = gridOriginY + grid.hLinesOnTexGrid[j];
                        max = gridOriginY + grid.hLinesOnTexGrid[j + 1];

                        if (point.y >= min && point.y < max) //point.y <= max)
                        {
                            cellRect.y = min;
                            cellRect.height = max - min;
                            break;
                        }
                    }

                    isPointInsideCG = true;          
                }

                if (isPointInsideCG)
                {
                    break;
                }
            }

            return cellRect;
        }

        public static Vector2 GetMonoColorUV(SemiPaletteModifier pMod, Texture2D texture)
        {
            Vector2 uv = new Vector2();
            if (pMod != null && pMod.texGrid != null && pMod.texGrid.gridsList != null)
            {
                Rect cellRect = new Rect(0, 0, 0, 0);
                CustomGrid grid = pMod.texGrid.gridsList[0];

                //int x, y;//, width, height;
                /* width = grid.gridWidth;
                 height = grid.gridHeight;*/
                int x = grid.gridPos.x;
                int y = grid.gridPos.y;

                int min, max;

                min = x + grid.vLinesOnTexGrid[0];
                max = x + grid.vLinesOnTexGrid[1];
                cellRect.x = min;
                cellRect.width = max - min;

                min = y + grid.hLinesOnTexGrid[0];
                max = y + grid.hLinesOnTexGrid[1];
                cellRect.y = min;
                cellRect.height = max - min;


                float u = ((cellRect.x + cellRect.width * 0.5f) / texture.width);
                float v = ((cellRect.y + cellRect.height * 0.5f) / texture.height);
                uv = new Vector2(u, v);

            }
            else
            {
                Debug.LogError("Something's null!");
            }
            return uv;
        }

        /// <summary>
        /// Checks whether a point is inside a rectangle.
        /// </summary>
        /// <param name="rect"> A rectangle. </param>
        /// <param name="point"> A 2D point. </param>
        /// <returns> Returns true if the point is inside the rectangle, otherwise returns false. </returns>
        public static bool PointInsideRect(Rect rect, Vector2Int point)
        {
            if (point.x < rect.x)
                return false;
            if (point.x > rect.x + rect.width)
                return false;

            if (point.y < rect.y)
                return false;
            if (point.y > rect.y + rect.height)
                return false;

            return true;
        }


        public static List<Rect> GetUsedGridCells(SemiPaletteModifier pMod, Texture2D tex)
        {
            int x, y;
            int texelX, texelY;
            int usedCellsCount = 0;
            int minX, maxX, minY, maxY;
            Color texelColor;
            List<Rect> usedCells = new List<Rect>();

            for (int i = 0; i < pMod.texGrid.gridsList.Count; i++)
            {
                CustomGrid grid = pMod.texGrid.gridsList[i];
                x = grid.gridPos.x;
                y = grid.gridPos.y;
               
                for (int r = 0; r < grid.gridColumns; r++)
                {
                    for (int c = 0; c < grid.gridRows; c++)
                    {
                        minX = x + grid.vLinesOnTexGrid[c];
                        maxX = x + grid.vLinesOnTexGrid[c + 1];
                                   
                        minY = y + grid.hLinesOnTexGrid[r];
                        maxY = y + grid.hLinesOnTexGrid[r + 1];
               
                        texelX = Mathf.CeilToInt(minX + (maxX - minX) * 0.5f);
                        texelY = Mathf.CeilToInt(minY + (maxY - minY) * 0.5f);
               
                        texelColor = tex.GetPixel(texelX, texelY);
               
                        if (texelColor != grid.emptySpaceColor)
                        {
                            usedCells.Add(new Rect(minX, minY, maxX - minX, maxY - minY));
                            usedCellsCount++;
                        }
                    }
                }
            }
            return usedCells;
        }

        public static Rect GetBestApproximateCell(SemiPaletteModifier pMod, Texture2D texture,CellData cellData)// ref Color32 colour)
        {
            int x, y;
            int texelX, texelY;
            int minX, maxX, minY, maxY;
            //Vector2 bestUV = new Vector2();
            Rect bestRect = new Rect();
            int bestRGBDifferene = int.MaxValue;

            Color32 texelColour;
            Color32 newColour = new Color32();

            for (int i = 0; i < pMod.texGrid.gridsList.Count; i++)
            {
                CustomGrid grid = pMod.texGrid.gridsList[i];
                x = grid.gridPos.x;
                y = grid.gridPos.y;

                for (int r = 0; r < grid.gridColumns; r++)
                {
                    for (int c = 0; c < grid.gridRows; c++)
                    {
                        minX = x + grid.vLinesOnTexGrid[c];
                        maxX = x + grid.vLinesOnTexGrid[c + 1];

                        minY = y + grid.hLinesOnTexGrid[r];
                        maxY = y + grid.hLinesOnTexGrid[r + 1];

                        texelX = Mathf.CeilToInt(minX + (maxX - minX) * 0.5f);
                        texelY = Mathf.CeilToInt(minY + (maxY - minY) * 0.5f);

                        texelColour = texture.GetPixel(texelX, texelY);

                        //if (texelColor != grid.emptySpaceColor)
                        {
                            int RGBDifference = GetRGBDifference(ref texelColour,ref cellData.swatchCellColor);
                            if(RGBDifference < bestRGBDifferene)
                            {
                                bestRGBDifferene = RGBDifference;
                                newColour = texelColour;
                                bestRect = new Rect(minX, minY, maxX - minX, maxY - minY);
                                //bestUV = new Vector2((float)texelX / texture.width, (float)texelY / texture.height);
                            }
                        }
                    }
                }
            }
            cellData.swatchCellColor = newColour;
            return bestRect;
        }

        private static int GetRGBDifference(ref Color32 colourA, ref Color32 colourB)
        {
            int r = Mathf.Abs(colourA.r - colourB.r);
            int g = Mathf.Abs(colourA.g - colourB.g);
            int b = Mathf.Abs(colourA.b - colourB.b);

            int average = r + g + b;
            return average;
        }

        public static bool SetTextureGridReference(SemiPaletteModifier pMod, Texture2D tex)
        {
            List<TextureGrid> tgList = FindAssetsByType<TextureGrid>();
            bool tgFound = false;

            for (int i = 0; i < tgList.Count; i++)
            {
                if (tex == tgList[i].texAtlas)
                {
                    pMod.texGrid = (TextureGrid)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(tgList[i]), typeof(TextureGrid));
                    tgFound = true;
                    break;
                }
            }

            return tgFound;
        }

        public static List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (asset != null)
                    assets.Add(asset);            
            }

            return assets;
        }
#endif
    }
}
