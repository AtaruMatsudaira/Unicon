#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Drawing;
using System.Drawing.Imaging;

namespace DockIconChanger
{
    internal sealed class WindowsNativeMethods : INativeMethods
    {
        private const uint WM_SETICON = 0x0080;
        private const uint WM_GETICON = 0x007F;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;
        private const int GCLP_HICON = -14;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        private static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 4 
                ? new IntPtr(GetClassLong32(hWnd, nIndex)) 
                : GetClassLong64(hWnd, nIndex);
        }
        
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
                UnityEngine.Debug.Log("DockIconChanger: Setting icon with color overlay is Windows Editor is experimental and may not work as expected.");
                
                var process = Process.GetCurrentProcess();
                var hWnd = process.MainWindowHandle;

                if (hWnd == IntPtr.Zero)
                {
                    UnityEngine.Debug.LogWarning("DockIconChanger: Unable to retrieve main window handle.");
                    return false;
                }

                var currentHIcon = SendMessage(hWnd, WM_GETICON, new IntPtr(ICON_BIG), IntPtr.Zero);
                if (currentHIcon == IntPtr.Zero)
                {
                    currentHIcon = GetClassLongPtr(hWnd, GCLP_HICON);
                }
                
                if (currentHIcon == IntPtr.Zero)
                {
                    try
                    {
                        var icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
                        if (icon != null)
                        {
                            currentHIcon = icon.Handle;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (currentHIcon == IntPtr.Zero)
                {
                    UnityEngine.Debug.LogWarning("DockIconChanger: Unable to retrieve current icon.");
                    return false;
                }

                using var originalIcon = Icon.FromHandle(currentHIcon);
                using var originalBmp = originalIcon.ToBitmap();
                
                using var workingBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);
                using (var g = System.Drawing.Graphics.FromImage(workingBmp))
                {
                    g.DrawImage(originalBmp, 0, 0);
                }

                var bmpData = workingBmp.LockBits(
                    new Rectangle(0, 0, workingBmp.Width, workingBmp.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb
                );

                var addR = (byte)(Mathf.Clamp01(color.r) * 255);
                var addG = (byte)(Mathf.Clamp01(color.g) * 255);
                var addB = (byte)(Mathf.Clamp01(color.b) * 255);

                unsafe
                {
                    var ptr = (byte*)bmpData.Scan0;
                    var height = workingBmp.Height;
                    var width = workingBmp.Width;
                    var stride = bmpData.Stride;

                    for (var y = 0; y < height; y++)
                    {
                        var row = ptr + y * stride;

                        for (var x = 0; x < width; x++)
                        {
                            var bIndex = x * 4;
                            var gIndex = bIndex + 1;
                            var rIndex = bIndex + 2;
                            var aIndex = bIndex + 3;

                            var alpha = row[aIndex];

                            if (alpha > 0)
                            {
                                var b = row[bIndex] + addB;
                                row[bIndex] = (byte)Math.Min(b, 255);

                                var g = row[gIndex] + addG;
                                row[gIndex] = (byte)Math.Min(g, 255);

                                var r = row[rIndex] + addR;
                                row[rIndex] = (byte)Math.Min(r, 255);
                            }
                        }
                    }
                }

                workingBmp.UnlockBits(bmpData);

                var hIconNew = workingBmp.GetHicon();
                UpdateIcon(hWnd, hIconNew, hIconNew);

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
                
            SendMessage(hWnd, WM_SETICON, new IntPtr(ICON_SMALL), _hIconSmall);
            SendMessage(hWnd, WM_SETICON, new IntPtr(ICON_BIG), _hIconBig);
        }
    }
}
#endif