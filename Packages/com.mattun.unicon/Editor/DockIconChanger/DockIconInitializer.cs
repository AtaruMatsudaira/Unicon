using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DockIconChanger
{
    [InitializeOnLoad]
    internal static class DockIconInitializer
    {
        static DockIconInitializer()
        {
            // Skip if entering play mode or compiling
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            // Delay call to ensure Unity is fully initialized
            EditorApplication.delayCall += ApplyDockIcon;
        }

        [DidReloadScripts]
        private static void ApplyDockIcon()
        {
            try
            {
                DockIconSettings.Load();

                // Skip if not enabled
                if (!DockIconSettings.Enabled)
                {
                    return;
                }

                // Prepare all parameters for unified API
                string imagePath = "";
                if (!string.IsNullOrEmpty(DockIconSettings.IconPath) && File.Exists(DockIconSettings.IconPath))
                {
                    imagePath = DockIconSettings.IconPath;
                }

                // Determine overlay color
                Color overlayColor;
                if (DockIconSettings.UseAutoColor)
                {
                    overlayColor = DockIconSettings.GenerateColorFromProjectName(Application.productName);
                }
                else
                {
                    overlayColor = DockIconSettings.OverlayColor;
                }

                // Get badge text settings
                string badgeText = DockIconSettings.BadgeText ?? "";
                Color textColor = DockIconSettings.BadgeTextColor;

                // Apply all settings with unified API
                bool success = NativeMethods.SetIconUnified(imagePath, overlayColor, badgeText, textColor);
                if (success)
                {
                    Debug.Log($"DockIconChanger: Applied dock icon customization - " +
                              $"Image: {(string.IsNullOrEmpty(imagePath) ? "Default" : imagePath)}, " +
                              $"Overlay: {overlayColor}, " +
                              $"Badge: {(string.IsNullOrEmpty(badgeText) ? "None" : $"'{badgeText}'")}, " +
                              $"TextColor: {textColor}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"DockIconChanger: Failed to apply dock icon: {ex.Message}");
            }
        }
    }
}
