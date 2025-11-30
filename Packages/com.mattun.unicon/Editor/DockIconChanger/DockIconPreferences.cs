using System.IO;
using UnityEditor;
using UnityEngine;

namespace DockIconChanger
{
    internal static class DockIconPreferences
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Preferences/Dock Icon Changer", SettingsScope.User)
            {
                label = "Dock Icon Changer",
                guiHandler = DrawPreferencesGUI,
                keywords = new[] { "dock", "icon", "mac", "macos", "windows" }
            };

            return provider;
        }

        private static void DrawPreferencesGUI(string searchContext)
        {
            DockIconSettings.Load();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Dock Icon Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Customize the Unity Editor dock icon on macOS and Windows." +
                "You can either use a custom image or apply a color overlay to the default Unity icon.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Enable/Disable Toggle
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle("Enable Custom Dock Icon", DockIconSettings.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                DockIconSettings.Enabled = enabled;
                DockIconSettings.Save();

                if (enabled)
                {
                    // Apply settings immediately when enabled
                    ApplyCurrentSettings();
                }
                else
                {
                    // Reset to default when disabled
                    NativeMethods.ResetIcon();
                    Debug.Log("DockIconChanger: Disabled - Reset to default Unity icon");
                }
            }

            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(!enabled);

            // Custom Image Section
            EditorGUILayout.LabelField("Custom Image", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            string currentPath = DockIconSettings.IconPath;
            string displayPath = string.IsNullOrEmpty(currentPath) ? "No image selected" : Path.GetFileName(currentPath);
            EditorGUILayout.TextField("Icon Path", displayPath);

            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFilePanel(
                    "Select Dock Icon Image",
                    "",
                    "png,jpg,jpeg,tiff,tif,gif,bmp,icns,pdf,heic"
                );

                if (!string.IsNullOrEmpty(path))
                {
                    DockIconSettings.IconPath = path;
                    DockIconSettings.Enabled = true;
                    DockIconSettings.Save();

                    // Apply immediately
                    if (NativeMethods.SetIconFromPath(path))
                    {
                        Debug.Log($"DockIconChanger: Applied custom icon: {path}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentPath) && GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                DockIconSettings.IconPath = "";
                DockIconSettings.Save();
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(currentPath))
            {
                EditorGUILayout.HelpBox($"Full path: {currentPath}", MessageType.None);
            }

            EditorGUILayout.Space(15);

            // Color Overlay Section
            EditorGUILayout.LabelField("Color Overlay", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            bool useAutoColor = EditorGUILayout.Toggle("Use Auto Color", DockIconSettings.UseAutoColor);
            if (EditorGUI.EndChangeCheck())
            {
                DockIconSettings.UseAutoColor = useAutoColor;
                DockIconSettings.Save();
            }

            EditorGUILayout.HelpBox(
                useAutoColor
                    ? "Color is automatically generated from the project name."
                    : "Use the color picker below to set a custom overlay color.",
                MessageType.Info);

            EditorGUI.BeginDisabledGroup(useAutoColor);
            EditorGUI.BeginChangeCheck();

            Color overlayColor = EditorGUILayout.ColorField("Overlay Color", DockIconSettings.OverlayColor);
            if (EditorGUI.EndChangeCheck())
            {
                DockIconSettings.OverlayColor = overlayColor;
                DockIconSettings.Save();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            // Apply and Reset Buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Current Settings"))
            {
                ApplyCurrentSettings();
            }

            if (GUILayout.Button("Reset to Default"))
            {
                DockIconSettings.IconPath = "";
                DockIconSettings.UseAutoColor = true;
                DockIconSettings.OverlayColor = new Color(1.0f, 0.5f, 0.0f, 0.3f);
                DockIconSettings.Save();

                if (NativeMethods.ResetIcon())
                {
                    Debug.Log("DockIconChanger: Reset to default Unity icon");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup(); // End disabled group for main settings

            EditorGUILayout.Space(10);

            // Platform warning for non-macOS and non-Windows
#if !UNITY_EDITOR_OSX && !UNITY_EDITOR_WIN
            EditorGUILayout.HelpBox("This feature is only available on macOS and Windows", MessageType.Warning);
#endif
        }

        private static void ApplyCurrentSettings()
        {
            DockIconSettings.Load();

            if (!string.IsNullOrEmpty(DockIconSettings.IconPath) && File.Exists(DockIconSettings.IconPath))
            {
                if (NativeMethods.SetIconFromPath(DockIconSettings.IconPath))
                {
                    Debug.Log($"DockIconChanger: Applied custom icon: {DockIconSettings.IconPath}");
                }
            }
            else
            {
                Color color = DockIconSettings.UseAutoColor
                    ? DockIconSettings.GenerateColorFromProjectName(Application.productName)
                    : DockIconSettings.OverlayColor;

                if (NativeMethods.SetIconWithColorOverlay(color))
                {
                    Debug.Log($"DockIconChanger: Applied color overlay: {color}");
                }
            }
        }
    }
}
