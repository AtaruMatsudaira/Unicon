#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using UnityEditor;

namespace DockIconChanger
{
    internal sealed class WindowsNativeMethods : INativeMethods
    {
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
        
        private IntPtr _hIconSmall = IntPtr.Zero;
        private IntPtr _hIconBig = IntPtr.Zero;

        public bool SetIconFromPath(string imagePath)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var hWnd = process.MainWindowHandle;
                if (hWnd == IntPtr.Zero)
                {
                    UnityEngine.Debug.LogWarning("DockIconChanger: Unable to retrieve main window handle.");
                    return false;
                }

                using var bmp = new Bitmap(imagePath);
                var hIcon = bmp.GetHicon();
                UpdateIcon(hWnd, hIcon, hIcon);
                
                SetAppId(hWnd, CreateAppId(process.Id));
                
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"DockIconChanger: Error: {e.Message}");
                return false;
            }
        }

        public bool SetIconWithColorOverlay(UnityEngine.Color color)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var hWnd = process.MainWindowHandle;

                if (hWnd == IntPtr.Zero)
                {
                    UnityEngine.Debug.LogWarning("DockIconChanger: Unable to retrieve main window handle.");
                    return false;
                }

                var exePath = process.MainModule.FileName;
                var hIcon = ExtractIconFromFile(exePath);
                if (hIcon == IntPtr.Zero)
                {
                    UnityEngine.Debug.LogWarning("DockIconChanger: Unable to extract icon from executable.");
                    return false;
                }
                
                var hIconNew = WindowsOverlayIconCreator.Create(hIcon, color);
                
                DestroyIcon(hIcon);
                
                UpdateIcon(hWnd, hIconNew, hIconNew);
                
                SetAppId(hWnd, CreateAppId(process.Id));

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"DockIconChanger: Failed to set overlay icon: {e.Message}");
                return false;
            }
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
    }
}
#endif