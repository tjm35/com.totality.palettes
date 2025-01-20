using UnityEngine;

namespace Totality.Palettes
{
    public static class ColorExtensions
    {
        public static bool Approximately(this Color i_a, Color i_b)
        {
            return
                Mathf.Approximately(i_a.r, i_b.r) &&
                Mathf.Approximately(i_a.g, i_b.g) &&
                Mathf.Approximately(i_a.b, i_b.b) &&
                Mathf.Approximately(i_a.a, i_b.a);
        }
    }
}