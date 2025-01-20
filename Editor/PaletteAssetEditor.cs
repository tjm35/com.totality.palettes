using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using File = UnityEngine.Windows.File;

namespace Totality.Palettes.Editor
{
    [CustomEditor(typeof(PaletteAsset))]
    [CanEditMultipleObjects]
    public class PaletteAssetEditor : UnityEditor.Editor
    {
        public void Awake()
        {
            UpdateIcon();
        }

        public override void OnInspectorGUI()
        {
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");

                var oldEnabled = GUI.enabled;
                GUI.enabled = true;
                if (GUILayout.Button("Export Color Preset Library"))
                {
                    foreach (PaletteAsset paletteAsset in targets)
                    {
                        ExportColorPresetLibrary(paletteAsset);
                    }
                }

                if (targets.Length == 1 && GUILayout.Button("Export LUT Image..."))
                {
                    CreateLUTWizard.CreateWizard((PaletteAsset)target);
                }

                if (oldEnabled == false && GUILayout.Button("Save Editable Copy"))
                {
                    foreach (PaletteAsset paletteAsset in targets)
                    {
                        SaveEditableCopy(paletteAsset);
                    }
                }

                GUI.enabled = oldEnabled;

                if (cc.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    UpdateIcon();
                }
            }
        }

        private void UpdateIcon()
        {
            foreach (PaletteAsset paletteAsset in targets)
            {
                EditorGUIUtility.SetIconForObject(paletteAsset, PaletteIconBuilder.Build(paletteAsset));
            }
        }

        private void ExportColorPresetLibrary(PaletteAsset palette)
        {
            var cpl = ScriptableObject.CreateInstance("ColorPresetLibrary");
            cpl.name = palette.name;
            SetPresetLibraryFromPalette(cpl, palette);
            Directory.CreateDirectory("Assets\\Editor");
            AssetDatabase.CreateAsset(cpl, "Assets\\Editor\\" + cpl.name + ".colors");
        }
        
        private void SetPresetLibraryFromPalette(ScriptableObject presetLibrary, PaletteAsset palette)
        {
            var so = new SerializedObject(presetLibrary);
            var presets = so.FindProperty("m_Presets");
            presets.arraySize = palette.Colors.Length;

            for (int i = 0; i < palette.Colors.Length; ++i)
            {
                var element= presets.GetArrayElementAtIndex(i);
                var colorProp = element.FindPropertyRelative("m_Color");
                colorProp.colorValue = palette.Colors[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private void SaveEditableCopy(PaletteAsset palette)
        {
            var sourcePath = AssetDatabase.GetAssetPath(palette);
            if (sourcePath != null)
            {
                var targetPath = Path.Combine(Path.GetDirectoryName(sourcePath),
                    palette.name + " Copy.asset");
                var newPalette = Instantiate(palette);
                newPalette.name = palette.name + " Copy";
                AssetDatabase.CreateAsset(newPalette, targetPath);
                AssetDatabase.Refresh();
            }
        }
    }
}
