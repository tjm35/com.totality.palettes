using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Totality.Palettes
{
    [CreateAssetMenu(fileName = "MyPalette.asset", menuName = "Totality/Palette")]
    public class PaletteAsset : ScriptableObject
    {
        public enum ProximityMode
        {
            RGB,
            HSV,
            ValueOnly
        }
        
        [ColorUsage(false, false)]
        public Color[] Colors = Array.Empty<Color>();

        public Color FindClosest(Color lookup, ProximityMode mode = ProximityMode.RGB)
        {
            var lookupVec = ColToVec(lookup, mode);
            Color closest = Colors[0];
            float closestDistSq = float.MaxValue;
            foreach (var color in Colors)
            {
                var colorVec = ColToVec(color, mode);
                var distSq = Vector3.Distance(lookupVec, colorVec);
                if (distSq < closestDistSq)
                {
                    closest = color;
                    closestDistSq = distSq;
                }
            }

            return closest;
        }

        private Vector3 ColToVec(Color col, ProximityMode mode)
        {
            switch (mode)
            {
                case ProximityMode.RGB:
                    return new Vector3(col.r, col.g, col.b);
                case ProximityMode.HSV:
                {
                    Color.RGBToHSV(col, out float h, out float s, out float v);
                    return new Vector3(h, s, v);
                }
                case ProximityMode.ValueOnly:
                {
                    Color.RGBToHSV(col, out _, out _, out float v);
                    return v * Vector3.one;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), "Invalid proximity mode");
            }
        }
    }
}