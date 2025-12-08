#if UNITY_EDITOR_WIN
using System;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;

namespace Unicon
{
    public static class WindowsBitmapModifier
    {
        public static void ModifyOverlayColor(Bitmap bmp, UnityEngine.Color color)
        {
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );
                
            unsafe
            {
                var ptr = (byte*)bmpData.Scan0;
                var height = bmp.Height;
                var width = bmp.Width;
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

            bmp.UnlockBits(bmpData);
        }
        
        public static void ModifyBadgeText(Bitmap bmp, string text, UnityEngine.Color textColor)
        {
            using var graphics = System.Drawing.Graphics.FromImage(bmp);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            var stringFormat = new StringFormat
            {
                FormatFlags = StringFormatFlags.NoWrap,
                Trimming =  StringTrimming.None,
            };
            
            var fontSize = bmp.Width / 4;
            using var font = new System.Drawing.Font("Arial", fontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
            
            var textSize = graphics.MeasureString(text, font);
            var position = new PointF(
                bmp.Width - textSize.Width - 3,
                bmp.Height - textSize.Height - 3
            );
            
            using var graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
            graphicsPath.AddString(
                text,
                font.FontFamily,
                (int)font.Style,
                fontSize,
                position,
                stringFormat
            );

            var textColorDrawing = System.Drawing.Color.FromArgb(
                (int)(Mathf.Clamp01(textColor.a) * 255),
                (int)(Mathf.Clamp01(textColor.r) * 255),
                (int)(Mathf.Clamp01(textColor.g) * 255),
                (int)(Mathf.Clamp01(textColor.b) * 255)
            );
            using var brush = new SolidBrush(textColorDrawing);
            using var pen = new Pen(System.Drawing.Color.Black, fontSize / 20f);
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            graphics.DrawPath(pen, graphicsPath);
            graphics.FillPath(brush, graphicsPath);
        }
    }
}
#endif