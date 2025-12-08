#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using UnityEditor;
using UnityEngine;

namespace Unicon
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
                    UnityEngine.Debug.LogWarning("Unicon: Unable to retrieve current icon.");
                    return false;
                }
                
                UpdateIcon(hWnd, IntPtr.Zero, IntPtr.Zero);
                
                ClearAppId(hWnd);
                
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Unicon: Reset error: {e.Message}");
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
                    UnityEngine.Debug.LogWarning("Unicon: Unable to retrieve main window handle.");
                    return false;
                }
                
                var hIcon = iconBitmap.GetHicon();
                UpdateIcon(hWnd, hIcon, hIcon);
                SetAppId(hWnd, CreateAppId(process.Id));

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Unicon: SetIconUnified Error: name: {e.GetType().Name}, message: {e.Message}");
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
                UnityEngine.Debug.LogWarning("Unicon: Unable to retrieve main window handle.");
                return null;
            }

            var exePath = process.MainModule.FileName;
            var hIcon = ExtractIconFromFile(exePath);
            if (hIcon == IntPtr.Zero)
            {
                UnityEngine.Debug.LogWarning("Unicon: Unable to extract icon from executable.");
                return null;
            }

            var bmp = Bitmap.FromHicon(hIcon);
            WindowsBitmapModifier.ModifyOverlayColor(bmp, color);
                
            DestroyIcon(hIcon);
            
            return bmp;
        }

        private void UpdateIcon(IntPtr hWnd, IntPtr hIconSmall, IntPtr hIconBig)
        {
            SendMessage(hWnd, WM_SETICON, new IntPtr(ICON_SMALL), hIconSmall);
            SendMessage(hWnd, WM_SETICON, new IntPtr(ICON_BIG), hIconBig);
            
            if (_hIconSmall != IntPtr.Zero)
            {
                DestroyIcon(_hIconSmall);
                _hIconSmall = IntPtr.Zero;
            }
            
            if (_hIconBig != IntPtr.Zero)
            {
                DestroyIcon(_hIconBig);
                _hIconBig = IntPtr.Zero;
            }

            _hIconSmall = hIconSmall;
            _hIconBig = hIconBig;
        }
        
        private IntPtr ExtractIconFromFile(string filePath)
        {
            const int size = 256;
            var hIcons = new IntPtr[1];
            var iconIds = new IntPtr[1];
            
            var count = PrivateExtractIcons(filePath, 0, size, size, hIcons, iconIds, 1, 0);
            if (count > 0 && hIcons[0] != IntPtr.Zero)
            {
                return hIcons[0];
            }
            
            return IntPtr.Zero;
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
                DestroyIcon(_hIconSmall);
                _hIconSmall = IntPtr.Zero;
            }

            if (_hIconBig != IntPtr.Zero)
            {
                DestroyIcon(_hIconBig);
                _hIconBig = IntPtr.Zero;
            }
            
            SessionState.EraseString(SmallIconKey);
            SessionState.EraseString(BigIconKey);
            
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            Application.quitting -= OnQuitting;
        }
        
        // P/Invoke declarations
        
        private const uint WM_SETICON = 0x0080;
        
        /// <summary>
        /// 16x16 (ex: Title bar icon)
        /// </summary>
        private const int ICON_SMALL = 0;
        
        /// <summary>
        /// 32x32~256x256 (ex: Taskbar icon)
        /// </summary>
        private const int ICON_BIG = 1;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint PrivateExtractIcons(
            string lpszFile, 
            int nIconIndex, 
            int cxIcon, 
            int cyIcon, 
            IntPtr[] phicon, 
            IntPtr[] piconid, 
            uint nIcons, 
            uint flags
        );

        [DllImport("DockIconPluginForWindows.dll", CharSet = CharSet.Auto)]
        private static extern void SetAppId(IntPtr hWnd, string appId);
        
        [DllImport("DockIconPluginForWindows.dll", CharSet = CharSet.Auto)]
        private static extern void ClearAppId(IntPtr hWnd);
    }
}
#endif