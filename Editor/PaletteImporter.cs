using System;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Totality.Palettes.Editor
{
    [ScriptedImporter(version: 1, exts: new [] { "pal", "gpl" })]
    public class PaletteImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var ext = Path.GetExtension(ctx.assetPath);
            if (ext.Equals(".pal", StringComparison.InvariantCultureIgnoreCase))
            {
                EmitPalette(ImportPALAsset(ctx), ctx);
            }
            if (ext.Equals(".gpl", StringComparison.InvariantCultureIgnoreCase))
            {
                EmitPalette(ImportGPLAsset(ctx), ctx);
            }
        }
        
        private PaletteAsset ImportPALAsset(AssetImportContext ctx)
        {
            var palette = ScriptableObject.CreateInstance<PaletteAsset>();

            var palLines = File.ReadLines(ctx.assetPath).ToArray();

            if (palLines[0] != "JASC-PAL")
            {
                Debug.LogError($"PaletteImporter: {ctx.assetPath} is not a valid JASC PAL file.");
            }

            if (palLines[1] != "0100")
            {
                Debug.LogWarning($"Palette Importer: Unexpected version number {palLines[1]} in {ctx.assetPath}");
            }

            if (!uint.TryParse(palLines[2], out uint palCount))
            {
                Debug.LogError($"PaletteImporter: {ctx.assetPath} is not a valid JASC PAL file.");
            }

            if (palCount + 3 > palLines.Length)
            {
                Debug.LogError($"PaletteImporter: Not enough colour entries in {ctx.assetPath} (Found {palLines.Length - 3}, expected {palCount}).");
                return null;
            }

            if (palCount + 3 < palLines.Length)
            {
                Debug.LogWarning($"PaletteImporter: Too many colour entries in {ctx.assetPath} (Found {palLines.Length - 3}, expected {palCount}).");
            }

            palette.Colors = new Color[palCount];

            for (var i = 0; i < palCount; ++i)
            {
                var colourLine = palLines[i + 3];
                string[] values = colourLine.Split(" ");
                if (values.Length != 3 || values.Any(v => int.TryParse(v, out _) == false))
                {
                    Debug.LogError($"PaletteImporter: Malformed color {i} in {ctx.assetPath}: \"{colourLine}\"");
                    palette.Colors[i] = Color.clear;
                    continue;
                }

                int[] cols = values.Select(int.Parse).ToArray();
                palette.Colors[i] = new Color
                (
                    ((float)cols[0]) / 255.0f, 
                    ((float)cols[1]) / 255.0f,
                    ((float)cols[2]) / 255.0f
                );
            }

            return palette;
        }
        
        private PaletteAsset ImportGPLAsset(AssetImportContext ctx)
        {
            var palette = ScriptableObject.CreateInstance<PaletteAsset>();

            var palLines = File.ReadLines(ctx.assetPath)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Where(l => !l.StartsWith("#"))
                .ToArray();

            if (!palLines[0].StartsWith("GIMP Palette", StringComparison.InvariantCulture))
            {
                Debug.LogError($"PaletteImporter: {ctx.assetPath} is not a valid GIMP Palette file.");
            }

            var lineOffset = 1;

            if (palLines[lineOffset].StartsWith("Name: ", StringComparison.InvariantCulture))
            {
                lineOffset++;
                if (palLines[lineOffset].StartsWith("Columns: ", StringComparison.InvariantCulture))
                {
                    lineOffset++;
                }
            }

            var palCount = palLines.Length - lineOffset;

            palette.Colors = new Color[palCount];

            for (var i = 0; i < palCount; ++i)
            {
                var colourLine = palLines[i + lineOffset];
                var values = colourLine.Split(' ', '\t').Where(v => !string.IsNullOrWhiteSpace(v));
                if (values.Count() < 3 || values.Take(3).Any(v => int.TryParse(v, out _) == false))
                {
                    Debug.LogError($"PaletteImporter: Malformed color {i} in {ctx.assetPath}: \"{colourLine}\"");
                    palette.Colors[i] = Color.clear;
                    continue;
                }

                int[] cols = values.Take(3).Select(int.Parse).ToArray();
                palette.Colors[i] = new Color
                (
                    ((float)cols[0]) / 255.0f, 
                    ((float)cols[1]) / 255.0f,
                    ((float)cols[2]) / 255.0f
                );
            }

            return palette;
        }

        private void EmitPalette(PaletteAsset palette, AssetImportContext ctx)
        {
            if (palette != null)
            {
                palette.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
                ctx.AddObjectToAsset("PaletteData", palette, PaletteIconBuilder.Build(palette));
                ctx.SetMainObject(palette);
            }
        }

    }
}
