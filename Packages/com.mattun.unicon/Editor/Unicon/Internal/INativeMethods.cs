using UnityEngine;

namespace Unicon
{
    internal interface INativeMethods
    {
        bool ResetIcon();
        bool SetIconUnified(string imagePath, Color overlayColor, string text, Color textColor);
    }
}