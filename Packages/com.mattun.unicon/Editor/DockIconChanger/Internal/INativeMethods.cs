using UnityEngine;

namespace DockIconChanger
{
    internal interface INativeMethods
    {
        bool ResetIcon();
        bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor);
    }
}