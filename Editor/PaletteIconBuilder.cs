using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Totality.Palettes.Editor
{
    public static class PaletteIconBuilder
    {
        public static Texture2D Build(PaletteAsset palette)
        {
            const int baseSize = 128;
            int paletteSquareSize = Mathf.CeilToInt(Mathf.Sqrt(palette.Colors.Length));
            int paletteSquareEntryRes = Mathf.CeilToInt(((float)baseSize) / (float)paletteSquareSize);
            int paletteSquareRes = Mathf.CeilToInt(((float)baseSize) / (float)paletteSquareSize) * paletteSquareSize;

            Texture2D icon = new Texture2D(paletteSquareRes, paletteSquareRes, DefaultFormat.LDR, TextureCreationFlags.None);

            var palIndex = 0;
            for (int y = paletteSquareSize - 1; y >= 0; y--)
            for (var x = 0; x < paletteSquareSize; x++)
            {
                var color = palIndex < palette.Colors.Length ? palette.Colors[palIndex] : Color.clear;
                var colors = Enumerable.Repeat(color, paletteSquareEntryRes * paletteSquareEntryRes).ToArray();
                icon.SetPixels(x * paletteSquareEntryRes, y * paletteSquareEntryRes, paletteSquareEntryRes, paletteSquareEntryRes, colors);

                palIndex++;
            }

            icon.filterMode = FilterMode.Point;
            icon.Apply();
            return icon;
        }
    }
}
