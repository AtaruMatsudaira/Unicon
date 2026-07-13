namespace Unicon
{
    /// <summary>
    /// Implement this interface in editor code of a consuming project to supply
    /// the badge text drawn on the Unity Editor dock/taskbar icon.
    /// Select the implementation in Edit > Preferences > Unicon via "Badge Text Source".
    /// </summary>
    /// <remarks>
    /// Implementations must be non-abstract classes with a public parameterless
    /// constructor, compiled into an editor assembly (an Editor folder, or an
    /// editor-only assembly referencing Unicon.Editor).
    /// GetLabel() runs on the editor main thread and its result is cached: it is
    /// re-evaluated once per script reload, when the selection changes in
    /// Preferences, and when "Apply Current Settings" is clicked.
    /// If GetLabel() throws, Unicon logs a warning once and draws no badge.
    /// </remarks>
    public interface IUniconLabelWrappable
    {
        /// <summary>
        /// Returns the badge text to draw on the dock icon.
        /// Return null or an empty string to draw no badge.
        /// Keep it short (1-4 characters recommended) and fast.
        /// </summary>
        string GetLabel();
    }
}
