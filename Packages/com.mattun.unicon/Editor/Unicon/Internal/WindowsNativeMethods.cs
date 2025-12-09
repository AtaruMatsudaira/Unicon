#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using UnityEditor;

namespace Unicon
{
    internal sealed class WindowsNativeMethods : INativeMethods
    {
        private const string IsInitializedPluginKey = "com.mattun.unicon.windows.is_initialized_plugin";

        private static bool IsInitializedPlugin
        {
            get => SessionState.GetBool(IsInitializedPluginKey, false);
            set => SessionState.SetBool(IsInitializedPluginKey, value);
        }
        
        public WindowsNativeMethods()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnQuitting;
        }

        public bool ResetIcon()
        {
            try
            {
                DestroyIfNeeded();
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Unicon: Reset error: {e.Message}");
                return false;
            }
        }
        
        public bool SetIconUnified(string imagePath, UnityEngine.Color overlayColor, string text, UnityEngine.Color textColor, float fontSizeMultiplier)
        {
            // fontSizeMultiplier parameter is ignored on Windows (macOS only feature)
            try
            {
                using var iconBitmap = string.IsNullOrEmpty(imagePath)
                    ? CreateColoredIconBitmap(overlayColor)
                    : CreateFileIconBitmap(imagePath);

                if (iconBitmap == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(text))
                {
                    WindowsBitmapModifier.ModifyBadgeText(iconBitmap, text, textColor, fontSizeMultiplier);
                }
                
                var hIcon = iconBitmap.GetHicon();
                UpdateIcon(hIcon, hIcon);

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Unicon: SetIconUnified Error: name: {e.GetType().Name}, message: {e.Message}");
                return false;
            }
        }
        
        private static void InitializeIfNeeded()
        {
            if (IsInitializedPlugin)
            {
                return;
            }
            
            InitializeUniconPlugin();
            IsInitializedPlugin = true;
        }
        
        private static void DestroyIfNeeded()
        {
            if (!IsInitializedPlugin)
            {
                return;
            }
            
            DestroyUniconPlugin();
            IsInitializedPlugin = false;
        }
        
        private static Bitmap CreateFileIconBitmap(string imagePath)
        {
            return new Bitmap(imagePath);
        }
        
        private static Bitmap CreateColoredIconBitmap(UnityEngine.Color color)
        {
            var process = Process.GetCurrentProcess();
            if (process.MainModule == null)
            {
                UnityEngine.Debug.LogWarning("Unicon: Unable to get current process information.");
                return null;
            }
            
            var exePath = process.MainModule.FileName;
            var hIcon = ExtractIconFromPath(exePath, 256);
            if (hIcon == IntPtr.Zero)
            {
                UnityEngine.Debug.LogWarning("Unicon: Unable to extract icon from executable.");
                return null;
            }

            var bmp = Bitmap.FromHicon(hIcon);
            WindowsBitmapModifier.ModifyOverlayColor(bmp, color);
                
            DeleteIcon(hIcon);
            
            return bmp;
        }

        private static void UpdateIcon(IntPtr hIconSmall, IntPtr hIconBig)
        {
            InitializeIfNeeded();
            
            var processId = Process.GetCurrentProcess().Id;
            var appId = CreateAppId(processId);
            
            ApplyToProcessWindows(processId, appId, hIconSmall, hIconBig);
        }

        private static string CreateAppId(int processId)
        {
            return $"{PlayerSettings.productName}.{processId}";
        }
        
        private static void OnBeforeAssemblyReload()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.quitting -= OnQuitting;
        }
        
        private static void OnQuitting()
        {
            DestroyIfNeeded();
            
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.quitting -= OnQuitting;
        }
        
        #region P/Invoke Declarations

        private const string DllName = "UniconPluginForWindows.dll";
        
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        private static extern IntPtr ExtractIconFromPath(string filePath, int size);
        
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        private static extern void ApplyToProcessWindows(int processId, string appId, IntPtr hIconSmall, IntPtr hIconBig);
        
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        private static extern void DeleteIcon(IntPtr hIcon);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        private static extern void InitializeUniconPlugin();
        
        [DllImport(DllName, CharSet = CharSet.Unicode)]
        private static extern void DestroyUniconPlugin();
        
        #endregion
    }
}
#endif