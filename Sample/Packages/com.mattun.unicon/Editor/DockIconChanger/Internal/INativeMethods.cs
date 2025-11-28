using UnityEngine;

namespace DockIconChanger
{
    internal interface INativeMethods
    {
        bool SetIconFromPath(string imagePath);
        bool SetIconWithColorOverlay(Color color);
        bool ResetIcon();
    }
}