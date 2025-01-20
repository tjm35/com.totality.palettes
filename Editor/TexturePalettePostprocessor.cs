using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Totality.Palettes.Editor
{
    public class TexturePalettePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (ShouldProcessAsPalette)
            {
                var textureImporter = assetImporter as TextureImporter;
                Debug.Assert(textureImporter != null);
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.textureShape = TextureImporterShape.Texture2D;
                textureImporter.sRGBTexture = false;
                textureImporter.alphaSource = TextureImporterAlphaSource.None;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                textureImporter.mipmapEnabled = false;
                textureImporter.filterMode = FilterMode.Point;
            }
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            if (ShouldProcessAsPalette)
            {
                ImportPaletteFromTexture(texture, context);
            }
        }

        private bool ShouldProcessAsPalette => Path.GetFileName(assetPath)
            .StartsWith("PAL_", StringComparison.InvariantCultureIgnoreCase);

        private static void ImportPaletteFromTexture(Texture2D texture, AssetImportContext ctx)
        {
            var palette = ScriptableObject.CreateInstance<PaletteAsset>();
            palette.Colors = texture.GetPixels().Select(c => (Color32)c).Distinct().Select(c => (Color)c).ToArray();
            palette.name = GetPaletteNameFromPath(ctx.assetPath);
            ctx.AddObjectToAsset(palette.name, palette, PaletteIconBuilder.Build(palette));
            
            var palette2 = Object.Instantiate(palette);
            palette2.name = GetPaletteNameFromPath(ctx.assetPath);
            ctx.AddObjectToAsset(palette.name + "_PaletteData", palette2, PaletteIconBuilder.Build(palette));
        }

        private static string GetPaletteNameFromPath(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.StartsWith("PAL_", StringComparison.InvariantCultureIgnoreCase))
            {
                return new string(fileName.Skip(4).ToArray());
            }

            return fileName;
        }
    }
}
