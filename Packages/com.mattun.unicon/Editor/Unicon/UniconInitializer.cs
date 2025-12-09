using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Unicon
{
    [InitializeOnLoad]
    internal static class UniconInitializer
    {
        private static int _frameCount;
        private static double _lastApplyTime;
        private const int FrameCheckInterval = 60;
        private const double ApplyInterval = 1.0;

        static UniconInitializer()
        {
#if UNITY_EDITOR_WIN
            EditorApplication.delayCall += ApplyDockIcon;
#elif UNITY_EDITOR_OSX
            // Register periodic update to maintain dock icon
            // macOS may reset the icon, so we need to re-apply periodically
            EditorApplication.update += PeriodicApply;
#endif
        }

        private static void PeriodicApply()
        {
            // Only check time every N frames to reduce overhead
            _frameCount++;
            if (_frameCount < FrameCheckInterval)
            {
                return;
            }

            _frameCount = 0;

            // Check if enough time has passed since last application
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - _lastApplyTime < ApplyInterval)
            {
                return;
            }

            _lastApplyTime = currentTime;
            ApplyDockIcon();
        }

        [DidReloadScripts]
        private static void ApplyDockIcon()
        {
            try
            {
                UniconSettings.Load();

                // Skip if not enabled
                if (!UniconSettings.Enabled)
                {
                    return;
                }

                // Prepare all parameters for unified API
                string imagePath = "";
                if (!string.IsNullOrEmpty(UniconSettings.IconPath) && File.Exists(UniconSettings.IconPath))
                {
                    imagePath = UniconSettings.IconPath;
                }

                // Determine overlay color
                Color overlayColor;
                if (UniconSettings.UseAutoColor)
                {
                    overlayColor = UniconSettings.GenerateColorFromProjectName(Application.productName);
                }
                else
                {
                    overlayColor = UniconSettings.OverlayColor;
                }

                // Get badge text settings
                string badgeText = UniconSettings.BadgeText ?? "";
                Color textColor = UniconSettings.BadgeTextColor;
                float fontSizeMultiplier = UniconSettings.BadgeTextFontSizeMultiplier;

                // Apply all settings with unified API
                NativeMethods.SetIconUnified(imagePath, overlayColor, badgeText, textColor, fontSizeMultiplier);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Unicon: Failed to apply dock icon: {ex.Message}");
            }
        }
    }
}