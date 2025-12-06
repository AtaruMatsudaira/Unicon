#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using UnityEditor;
using UnityEngine;

namespace DockIconChanger
{
    internal sealed class WindowsNativeMethods : INativeMethods
    {
        private const string SmallIconKey = "com.mattun.unicon.windows.smallicon";
        private const string BigIconKey = "com.mattun.unicon.windows.bigicon";
        
        private IntPtr _hIconSmall = IntPtr.Zero;
        private IntPtr _hIconBig = IntPtr.Zero;

        public WindowsNativeMethods()
        {
            var smallIconPtrStr = SessionState.GetString(SmallIconKey, string.Empty);
            if (!string.IsNullOrEmpty(smallIconPtrStr))
            {
                if (long.TryParse(smallIconPtrStr, out var ptrValue))
                {
                    _hIconSmall = new IntPtr(ptrValue);
                }
            }
            
            var bigIconPtrStr = SessionState.GetString(BigIconKey, string.Empty);
            if (!string.IsNullOrEmpty(bigIconPtrStr))
            {
                if (long.TryParse(bigIconPtrStr, out var ptrValue))
                {
                    _hIconBig = new IntPtr(ptrValue);
                }
            }
            
            SessionState.EraseString(SmallIconKey);
            SessionState.EraseString(BigIconKey);
            
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnQuitting;
        }

        public bool ResetIcon()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var hWnd = process.MainWindowHandle;
                if (hWnd == IntPtr.Zero)
                {
                    UnityEngine.Debug.LogWarning("DockIconChanger: Unable to retrieve current icon.");
                    return false;
                }
                
                UpdateIcon(hWnd, IntPtr.Zero, IntPtr.Zero);
                
                ClearAppId(hWnd);
                
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"DockIconChanger: Reset error: {e.Message}");
                return false;
            }
        }
        
        public bool SetIconUnified(string imagePath, UnityEngine.Color overlayColor, string text, UnityEngine.Color textColor)
        {
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
                    WindowsBitmapModifier.ModifyBadgeText(iconBitmap, text, textColor);
                }
                
                var process = Process.GetCurrentProcess();
                var hWnd = process.MainWindowHandle;
                if (hWnd == IntPtr.Zero)
                {
                    UnityEngine.Debug.LogWarning("DockIconChanger: Unable to retrieve main window handle.");
                    return false;
                }
                
                var hIcon = iconBitmap.GetHicon();
                UpdateIcon(hWnd, hIcon, hIcon);
                SetAppId(hWnd, CreateAppId(process.Id));

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"DockIconChanger: SetIconUnified Error: name: {e.GetType().Name}, message: {e.Message}");
                return false;
            }
        }
        
        private Bitmap CreateFileIconBitmap(string imagePath)
        {
            return new Bitmap(imagePath);
        }
        
        private Bitmap CreateColoredIconBitmap(UnityEngine.Color color)
        {
            var process = Process.GetCurrentProcess();
            var hWnd = process.MainWindowHandle;

            if (hWnd == IntPtr.Zero)
            {
                UnityEngine.Debug.LogWarning("DockIconChanger: Unable to retrieve main window handle.");
                return null;
            }

            var exePath = process.MainModule.FileName;
            var hIcon = ExtractIconFromFile(exePath);
            if (hIcon == IntPtr.Zero)
            {
                UnityEngine.Debug.LogWarning("DockIconChanger: Unable to extract icon from executable.");
                return null;
            }

            var bmp = Bitmap.FromHicon(hIcon);
            WindowsBitmapModifier.ModifyOverlayColor(bmp, color);
                
            DestroyIcon(hIcon);
            
            return bmp;
        }

        private void UpdateIcon(IntPtr hWnd, IntPtr hIconSmall, IntPtr hIconBig)
        {
            SetIcon(hWnd, hIconSmall, hIconBig);
            
            if (_hIconSmall != IntPtr.Zero)
            {
                DeleteIcon(_hIconSmall);
                _hIconSmall = IntPtr.Zero;
            }
            
            if (_hIconBig != IntPtr.Zero)
            {
                DeleteIcon(_hIconBig);
                _hIconBig = IntPtr.Zero;
            }

            _hIconSmall = hIconSmall;
            _hIconBig = hIconBig;
        }

        private string CreateAppId(int processId)
        {
            return $"{PlayerSettings.productName}.{processId}";
        }
        
        private void OnBeforeAssemblyReload()
        {
            if (_hIconSmall != IntPtr.Zero)
            {
                SessionState.SetString(SmallIconKey, _hIconSmall.ToInt64().ToString());
            }

            if (_hIconBig != IntPtr.Zero)
            {
                SessionState.SetString(BigIconKey, _hIconBig.ToInt64().ToString());
            }
            
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            Application.quitting -= OnQuitting;
        }
        
        private void OnQuitting()
        {
            if (_hIconSmall != IntPtr.Zero)
            {
                DeleteIcon(_hIconSmall);
                _hIconSmall = IntPtr.Zero;
            }

            if (_hIconBig != IntPtr.Zero)
            {
                DeleteIcon(_hIconBig);
                _hIconBig = IntPtr.Zero;
            }
            
            SessionState.EraseString(SmallIconKey);
            SessionState.EraseString(BigIconKey);
            
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            Application.quitting -= OnQuitting;
        }
        
        // P/Invoke declarations
        
        private const string DllName = "DockIconPluginForWindows.dll";
        
        [DllImport(DllName, CharSet = CharSet.Auto)]
        private static extern void SetIcon(IntPtr hWnd, IntPtr hSmallIcon, IntPtr hBigIcon);

        [DllImport(DllName, CharSet = CharSet.Auto)]
        private static extern void DeleteIcon(IntPtr hIcon);

        [DllImport(DllName, CharSet = CharSet.Auto)]
        private static extern void SetAppId(IntPtr hWnd, string appId);
        
        [DllImport(DllName, CharSet = CharSet.Auto)]
        private static extern void ClearAppId(IntPtr hWnd);
        
        [DllImport(DllName, CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIconFromPath(string filePath, int size);
    }
}
#endif