#if UNITY_EDITOR_OSX
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DockIconChanger
{
    internal sealed class MacOSNativeMethods : INativeMethods
    {
        private const string PluginName = "DockIconPlugin";

        [DllImport(PluginName)]
        private static extern void ResetDockIcon();

        [DllImport(PluginName)]
        private static extern void SetDockIconUnified(
            string imagePath,
            float overlayR, float overlayG, float overlayB, float overlayA,
            string text,
            float textR, float textG, float textB, float textA
        );

        public bool ResetIcon()
        {
            try
            {
                ResetDockIcon();
                return true;
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogWarning($"DockIconChanger: Plugin not found. Make sure DockIconPlugin.bundle is in Assets/Plugins/Editor/macOS/. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DockIconChanger: Failed to reset dock icon: {ex.Message}");
                return false;
            }
        }

        public bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor)
        {
            try
            {
                SetDockIconUnified(
                    imagePath ?? "",
                    overlayColor.r, overlayColor.g, overlayColor.b, overlayColor.a,
                    text ?? "",
                    textColor.r, textColor.g, textColor.b, textColor.a
                );
                return true;
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogWarning($"DockIconChanger: Plugin not found. Make sure DockIconPlugin.bundle is in Plugins/Editor/macOS/. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DockIconChanger: Failed to set unified dock icon: {ex.Message}");
                return false;
            }
        }
    }
}
#endif
