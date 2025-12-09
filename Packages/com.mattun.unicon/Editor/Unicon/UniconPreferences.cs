using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unicon
{
    internal static class UniconPreferences
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Preferences/Unicon", SettingsScope.User)
            {
                label = "Unicon",
                guiHandler = DrawPreferencesGUI,
                keywords = new[] { "dock", "icon", "mac", "macos", "windows" }
            };

            return provider;
        }

        private static void DrawPreferencesGUI(string searchContext)
        {
            UniconSettings.Load();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Unicon Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Customize the Unity Editor dock icon on macOS and Windows. " +
                "You can either use a custom image or apply a color overlay to the default Unity icon.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Enable/Disable Toggle
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle("Enable Custom Dock Icon", UniconSettings.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                UniconSettings.Enabled = enabled;
                UniconSettings.Save();

                if (enabled)
                {
                    // Apply settings immediately when enabled
                    ApplyCurrentSettings();
                }
                else
                {
                    // Reset to default when disabled
                    NativeMethods.ResetIcon();
                    Debug.Log("Unicon: Disabled - Reset to default Unity icon");
                }
            }

            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(!enabled);

            // Custom Image Section
            EditorGUILayout.LabelField("Custom Image", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            string currentPath = UniconSettings.IconPath;
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
                    UniconSettings.IconPath = path;
                    UniconSettings.Enabled = true;
                    UniconSettings.Save();

                    // Apply immediately with unified API
                    ApplyCurrentSettings();
                }
            }

            if (!string.IsNullOrEmpty(currentPath) && GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                UniconSettings.IconPath = "";
                UniconSettings.Save();
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

            bool useAutoColor = EditorGUILayout.Toggle("Use Auto Color", UniconSettings.UseAutoColor);
            if (EditorGUI.EndChangeCheck())
            {
                UniconSettings.UseAutoColor = useAutoColor;
                UniconSettings.Save();
            }

            EditorGUILayout.HelpBox(
                useAutoColor
                    ? "Color is automatically generated from the project name."
                    : "Use the color picker below to set a custom overlay color.",
                MessageType.Info);

            EditorGUI.BeginDisabledGroup(useAutoColor);
            EditorGUI.BeginChangeCheck();

            Color overlayColor = EditorGUILayout.ColorField("Overlay Color", UniconSettings.OverlayColor);
            if (EditorGUI.EndChangeCheck())
            {
                UniconSettings.OverlayColor = overlayColor;
                UniconSettings.Save();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15);

            // Badge Text Section
            EditorGUILayout.LabelField("Badge Text", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            string badgeText = EditorGUILayout.TextField("Badge Text", UniconSettings.BadgeText);
            if (EditorGUI.EndChangeCheck())
            {
                UniconSettings.BadgeText = badgeText;
                UniconSettings.Save();
            }

            EditorGUILayout.HelpBox(
                "Display text on the dock icon (e.g., \"Win\", \"Dev\", \"1\", \"2\"). " +
                "Recommended: 1-4 characters for optimal visibility.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            Color badgeTextColor = EditorGUILayout.ColorField("Badge Text Color", UniconSettings.BadgeTextColor);
            if (EditorGUI.EndChangeCheck())
            {
                UniconSettings.BadgeTextColor = badgeTextColor;
                UniconSettings.Save();
            }

            EditorGUI.BeginChangeCheck();
            float fontSizeMultiplier = EditorGUILayout.Slider(
                "Font Size Multiplier",
                UniconSettings.BadgeTextFontSizeMultiplier,
                0.5f,
                2.0f
            );
            if (EditorGUI.EndChangeCheck())
            {
                UniconSettings.BadgeTextFontSizeMultiplier = fontSizeMultiplier;
                UniconSettings.Save();
            }

            EditorGUILayout.Space(10);

            // Apply and Reset Buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Current Settings"))
            {
                ApplyCurrentSettings();
            }

            if (GUILayout.Button("Reset to Default"))
            {
                UniconSettings.IconPath = "";
                UniconSettings.UseAutoColor = true;
                UniconSettings.OverlayColor = new Color(1.0f, 0.5f, 0.0f, 0.3f);
                UniconSettings.BadgeText = "";
                UniconSettings.BadgeTextColor = Color.white;
                UniconSettings.BadgeTextFontSizeMultiplier = 1.0f;
                UniconSettings.Save();

                if (NativeMethods.ResetIcon())
                {
                    Debug.Log("Unicon: Reset to default Unity icon");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup(); // End disabled group for main settings

            EditorGUILayout.Space(10);
        }

        private static void ApplyCurrentSettings()
        {
            UniconSettings.Load();

            // Prepare all parameters for unified API
            string imagePath = "";
            if (!string.IsNullOrEmpty(UniconSettings.IconPath) && File.Exists(UniconSettings.IconPath))
            {
                imagePath = UniconSettings.IconPath;
            }

            // Determine overlay color
            Color overlayColor = UniconSettings.UseAutoColor
                ? UniconSettings.GenerateColorFromProjectName(Application.productName)
                : UniconSettings.OverlayColor;

            // Get badge text settings
            string badgeText = UniconSettings.BadgeText ?? "";
            Color textColor = UniconSettings.BadgeTextColor;
            float fontSizeMultiplier = UniconSettings.BadgeTextFontSizeMultiplier;

            // Apply all settings with unified API
            if (NativeMethods.SetIconUnified(imagePath, overlayColor, badgeText, textColor, fontSizeMultiplier))
            {
                Debug.Log($"Unicon: Applied dock icon customization - " +
                          $"Image: {(string.IsNullOrEmpty(imagePath) ? "Default" : imagePath)}, " +
                          $"Overlay: {overlayColor}, " +
                          $"Badge: {(string.IsNullOrEmpty(badgeText) ? "None" : $"'{badgeText}'")}, " +
                          $"TextColor: {textColor}");
            }
        }
    }
}
