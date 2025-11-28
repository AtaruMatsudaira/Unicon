using System.IO;
using UnityEditor;
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

                // Check if custom image path is set and file exists
                if (!string.IsNullOrEmpty(DockIconSettings.IconPath) && File.Exists(DockIconSettings.IconPath))
                {
                    bool success = NativeMethods.SetIconFromPath(DockIconSettings.IconPath);
                    if (success)
                    {
                        Debug.Log($"DockIconChanger: Applied custom icon from: {DockIconSettings.IconPath}");
                    }
                }
                else
                {
                    // Use color overlay
                    Color color;
                    if (DockIconSettings.UseAutoColor)
                    {
                        color = DockIconSettings.GenerateColorFromProjectName(Application.productName);
                        Debug.Log($"DockIconChanger: Auto-generated color for project '{Application.productName}': {color}");
                    }
                    else
                    {
                        color = DockIconSettings.OverlayColor;
                        Debug.Log($"DockIconChanger: Using custom overlay color: {color}");
                    }

                    bool success = NativeMethods.SetIconWithColorOverlay(color);
                    if (success)
                    {
                        Debug.Log("DockIconChanger: Applied color overlay to dock icon");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"DockIconChanger: Failed to apply dock icon: {ex.Message}");
            }
        }
    }
}
