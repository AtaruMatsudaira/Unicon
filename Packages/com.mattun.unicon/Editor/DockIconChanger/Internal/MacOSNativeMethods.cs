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
        private static extern void SetDockIconFromPath(string imagePath);

        [DllImport(PluginName)]
        private static extern void SetDockIconWithColorOverlay(float r, float g, float b, float a);

        [DllImport(PluginName)]
        private static extern void ResetDockIcon();

        [DllImport(PluginName)]
        private static extern void SetDockIconWithText(string text, float r, float g, float b, float a);

        [DllImport(PluginName)]
        private static extern void SetDockIconUnified(
            string imagePath,
            float overlayR, float overlayG, float overlayB, float overlayA,
            string text,
            float textR, float textG, float textB, float textA
        );

        public bool SetIconFromPath(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    Debug.LogWarning("DockIconChanger: Image path is null or empty");
                    return false;
                }

                if (!System.IO.File.Exists(imagePath))
                {
                    Debug.LogWarning($"DockIconChanger: Image file not found: {imagePath}");
                    return false;
                }

                SetDockIconFromPath(imagePath);
                return true;
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogWarning($"DockIconChanger: Plugin not found. Make sure DockIconPlugin.bundle is in Assets/Plugins/Editor/macOS/. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DockIconChanger: Failed to set dock icon from path: {ex.Message}");
                return false;
            }
        }

        public bool SetIconWithColorOverlay(Color color)
        {
            try
            {
                SetDockIconWithColorOverlay(color.r, color.g, color.b, color.a);
                return true;
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogWarning($"DockIconChanger: Plugin not found. Make sure DockIconPlugin.bundle is in Assets/Plugins/Editor/macOS/. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DockIconChanger: Failed to set dock icon with color overlay: {ex.Message}");
                return false;
            }
        }

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

        public bool SetIconWithText(string text, Color textColor)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    Debug.LogWarning("DockIconChanger: Text is null or empty");
                    return false;
                }

                SetDockIconWithText(text, textColor.r, textColor.g, textColor.b, textColor.a);
                return true;
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogWarning($"DockIconChanger: Plugin not found. Make sure DockIconPlugin.bundle is in Plugins/Editor/macOS/. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DockIconChanger: Failed to set dock icon with text: {ex.Message}");
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