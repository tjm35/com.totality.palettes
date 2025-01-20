using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Totality.Palettes.Editor
{
    public class CreateLUTWizard : EditorWindow
    {
        public enum LUTType
        {
            URP,
            PostProcessing_1024x32,
            PostProcessing_256x16
        }
        
        public PaletteAsset m_palette;
        public LUTType m_lutType = LUTType.URP;
        public PaletteAsset.ProximityMode m_matchingMode = PaletteAsset.ProximityMode.RGB;
        public bool m_smoothColours = false;
        public string m_outputPath = "Assets\\MyLUT.png";
        
        public static void CreateWizard(PaletteAsset palette)
        {
            var clw = CreateWindow<CreateLUTWizard>("Create LUT");
            clw.m_palette = palette;
        }

        public void OnGUI()
        {
            m_palette = (PaletteAsset)EditorGUILayout.ObjectField("Palette", m_palette, typeof(PaletteAsset), true);
            m_lutType = (LUTType)EditorGUILayout.EnumPopup("LUT Type", m_lutType);
            m_matchingMode = (PaletteAsset.ProximityMode)EditorGUILayout.EnumPopup("Matching Mode", m_matchingMode);
            m_smoothColours = EditorGUILayout.Toggle("Smooth Colours", m_smoothColours);
            using (new GUILayout.HorizontalScope())
            {
                m_outputPath = EditorGUILayout.TextField("Output Path", m_outputPath);
                if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(m_outputPath);
                        m_outputPath = EditorUtility.SaveFilePanel("Save LUT file", Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath), "png");
                    }
                    catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
                    {
                        m_outputPath = EditorUtility.SaveFilePanel("Save LUT file", "Assets\\", "MyLUT.png", "png");
                    }
                }
            }

            GUILayout.FlexibleSpace();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create"))
                {
                    ExportLUTImage();
                    Close();
                }
            }
        }
        
        private void ExportLUTImage()
        {
            int baseSize = GetLUTBaseSize(m_lutType);
            
            float colorStep = 1.0f / (float)baseSize;
            var texture = new Texture2D(baseSize * baseSize, baseSize, DefaultFormat.LDR, TextureCreationFlags.None);

            for (int r = 0; r < baseSize; ++r)
            for (int g = 0; g < baseSize; ++g)
            for (int b = 0; b < baseSize; ++b)
            {
                var lookupColor = new Color(colorStep * (float)r, colorStep * (float)g, colorStep * (float)b);
                var color = m_palette.FindClosest(lookupColor, m_matchingMode);
                var y = m_lutType switch
                {
                    LUTType.URP => g,
                    LUTType.PostProcessing_1024x32 or LUTType.PostProcessing_256x16 => baseSize - g - 1,
                    _ => throw new ArgumentOutOfRangeException(nameof(m_lutType), "Invalid LUT Type")
                };
                texture.SetPixel(b*baseSize + r, y, color);
            }

            var pngData = texture.EncodeToPNG();
            File.WriteAllBytes(m_outputPath, pngData);
            AssetDatabase.Refresh();

            var outputPath = m_outputPath;
            var smoothColours = m_smoothColours;
            var lutType = m_lutType;
            EditorApplication.CallbackFunction updateCallback = null;
            updateCallback = () =>
            {
                var importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
                if (importer != null)
                {
                    TextureImporterSettings settings = new();
                    importer.ReadTextureSettings(settings);
                    settings.textureType = TextureImporterType.Default;
                    settings.textureShape = lutType == LUTType.URP ? TextureImporterShape.Texture2D : TextureImporterShape.Texture3D;
                    settings.flipbookColumns = GetLUTBaseSize(lutType);
                    settings.sRGBTexture = false;
                    settings.filterMode = smoothColours ? FilterMode.Bilinear : FilterMode.Point;
                    settings.mipmapEnabled = false;
                    importer.SetTextureSettings(settings);
                    AssetDatabase.ImportAsset(outputPath);
                    AssetDatabase.Refresh();
                    EditorApplication.update = Delegate.Remove(EditorApplication.update, updateCallback) as EditorApplication.CallbackFunction;
                }
            };
            EditorApplication.update = Delegate.Combine(EditorApplication.update, updateCallback) as EditorApplication.CallbackFunction;
        }
        
        private int GetLUTBaseSize(LUTType lutType) => lutType switch
        {
            LUTType.URP or LUTType.PostProcessing_1024x32 => 32,
            LUTType.PostProcessing_256x16 => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(m_lutType), "Invalid LUT Type")
        };
    }
}
