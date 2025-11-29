#if UNITY_EDITOR_WIN
using System;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;

namespace DockIconChanger
{
    public static class WindowsOverlayIconCreator
    {
        public static IntPtr Create(IntPtr hIcon, UnityEngine.Color color)
        {
            using var originalBmp = Bitmap.FromHicon(hIcon);
            using var workingBmp = new Bitmap(originalBmp.Width, originalBmp.Height, PixelFormat.Format32bppArgb);
            
            using (var g = System.Drawing.Graphics.FromImage(workingBmp))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.DrawImage(originalBmp, 0, 0);
            }

            var bmpData = workingBmp.LockBits(
                new Rectangle(0, 0, workingBmp.Width, workingBmp.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );
                
            unsafe
            {
                var ptr = (byte*)bmpData.Scan0;
                var height = workingBmp.Height;
                var width = workingBmp.Width;
                var stride = bmpData.Stride;
                
                var overlayA = Mathf.Clamp01(color.a);
                var overlayR = (int)(Mathf.Clamp01(color.r) * overlayA * 255);
                var overlayG = (int)(Mathf.Clamp01(color.g) * overlayA * 255);
                var overlayB = (int)(Mathf.Clamp01(color.b) * overlayA * 255);

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

                        if (alpha == 0)
                        {
                            continue;
                        }
                        
                        var addR = overlayR * alpha / 255;
                        var addG = overlayG * alpha / 255;
                        var addB = overlayB * alpha / 255;
                            
                        var b = row[bIndex] + addB;
                        row[bIndex] = (byte)Math.Min(b, 255);

                        var g = row[gIndex] + addG;
                        row[gIndex] = (byte)Math.Min(g, 255);

                        var r = row[rIndex] + addR;
                        row[rIndex] = (byte)Math.Min(r, 255);
                    }
                }
            }

            workingBmp.UnlockBits(bmpData);

           return workingBmp.GetHicon();
        }
    }
}
#endif