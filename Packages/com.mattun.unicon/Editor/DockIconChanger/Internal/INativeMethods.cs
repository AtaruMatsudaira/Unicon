using UnityEngine;

namespace DockIconChanger
{
    internal interface INativeMethods
    {
        bool SetIconFromPath(string imagePath);
        bool SetIconWithColorOverlay(Color color);
        bool ResetIcon();
        bool SetIconWithText(string text, Color textColor);
        bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor);
    }
}