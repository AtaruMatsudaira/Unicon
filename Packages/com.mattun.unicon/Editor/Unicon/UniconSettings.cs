using System;
using System.IO;
using UnityEngine;

namespace Unicon
{
    [Serializable]
    internal class UniconSettingsData
    {
        public bool enabled = false;
        public string iconPath = "";
        public bool useAutoColor = true;
        public float overlayColorR = 1.0f;
        public float overlayColorG = 1.0f;
        public float overlayColorB = 1.0f;
        public float overlayColorA = 0f;
        public string badgeText = "";
        public float badgeTextColorR = 1.0f;
        public float badgeTextColorG = 1.0f;
        public float badgeTextColorB = 1.0f;
        public float badgeTextColorA = 1.0f;
        public float badgeTextFontSizeMultiplier = 1.0f;
        public int badgeTextSource = 0;
        public string badgeLabelProviderTypeName = "";
    }

    internal enum BadgeTextSource
    {
        DirectText = 0,
        Provider = 1,
    }

    internal static class UniconSettings
    {
        private const string kSettingsPath = "UserSettings/DockIconSettings.json";
        private static UniconSettingsData s_data;
        private static bool s_loaded = false;

        public static bool Enabled
        {
            get
            {
                EnsureLoaded();
                return s_data.enabled;
            }
            set
            {
                EnsureLoaded();
                s_data.enabled = value;
            }
        }

        public static string IconPath
        {
            get
            {
                EnsureLoaded();
                return s_data.iconPath;
            }
            set
            {
                EnsureLoaded();
                s_data.iconPath = value;
            }
        }

        public static bool UseAutoColor
        {
            get
            {
                EnsureLoaded();
                return s_data.useAutoColor;
            }
            set
            {
                EnsureLoaded();
                s_data.useAutoColor = value;
            }
        }

        public static Color OverlayColor
        {
            get
            {
                EnsureLoaded();
                return new Color(s_data.overlayColorR, s_data.overlayColorG, s_data.overlayColorB, s_data.overlayColorA);
            }
            set
            {
                EnsureLoaded();
                s_data.overlayColorR = value.r;
                s_data.overlayColorG = value.g;
                s_data.overlayColorB = value.b;
                s_data.overlayColorA = value.a;
            }
        }

        public static string BadgeText
        {
            get
            {
                EnsureLoaded();
                return s_data.badgeText;
            }
            set
            {
                EnsureLoaded();
                s_data.badgeText = value;
            }
        }

        public static BadgeTextSource BadgeTextSource
        {
            get
            {
                EnsureLoaded();
                // Unknown values (hand-edited JSON) fall back to DirectText.
                return s_data.badgeTextSource == (int)BadgeTextSource.Provider
                    ? BadgeTextSource.Provider
                    : BadgeTextSource.DirectText;
            }
            set
            {
                EnsureLoaded();
                if (s_data.badgeTextSource != (int)value)
                {
                    s_data.badgeTextSource = (int)value;
                    UniconBadgeLabelResolver.Invalidate();
                }
            }
        }

        public static string BadgeLabelProviderTypeName
        {
            get
            {
                EnsureLoaded();
                return s_data.badgeLabelProviderTypeName;
            }
            set
            {
                EnsureLoaded();
                if (s_data.badgeLabelProviderTypeName != value)
                {
                    s_data.badgeLabelProviderTypeName = value;
                    UniconBadgeLabelResolver.Invalidate();
                }
            }
        }

        public static Color BadgeTextColor
        {
            get
            {
                EnsureLoaded();
                return new Color(s_data.badgeTextColorR, s_data.badgeTextColorG, s_data.badgeTextColorB, s_data.badgeTextColorA);
            }
            set
            {
                EnsureLoaded();
                s_data.badgeTextColorR = value.r;
                s_data.badgeTextColorG = value.g;
                s_data.badgeTextColorB = value.b;
                s_data.badgeTextColorA = value.a;
            }
        }

        public static float BadgeTextFontSizeMultiplier
        {
            get
            {
                EnsureLoaded();
                return s_data.badgeTextFontSizeMultiplier;
            }
            set
            {
                EnsureLoaded();
                s_data.badgeTextFontSizeMultiplier = value;
            }
        }

        private static void EnsureLoaded()
        {
            if (!s_loaded)
            {
                Load();
            }
        }

        public static void Load()
        {
            if (File.Exists(kSettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(kSettingsPath);
                    s_data = JsonUtility.FromJson<UniconSettingsData>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load UniconSettings: {ex.Message}. Using defaults.");
                    s_data = new UniconSettingsData();
                }
            }
            else
            {
                s_data = new UniconSettingsData();
            }

            s_loaded = true;
        }

        public static void Save()
        {
            try
            {
                EnsureLoaded();

                // Ensure UserSettings directory exists
                string directory = Path.GetDirectoryName(kSettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(s_data, true);
                File.WriteAllText(kSettingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save UniconSettings: {ex.Message}");
            }
        }

        public static Color GenerateColorFromProjectName(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                return new Color(1.0f, 0.5f, 0.0f, 0.3f); // Default: Orange with transparency
            }

            // Deterministic hash -> HSV. Does NOT touch UnityEngine.Random's global state.
            // (Previously called Random.InitState, which corrupted the editor-wide RNG on every periodic re-apply.)
            uint hash = StableHash(projectName);

            float h = (hash & 0xFFFF) / 65535f;                    // Hue: 0-1
            float s = 0.7f + ((hash >> 16) & 0xFF) / 255f * 0.3f;  // Saturation: 0.7-1.0
            float v = 0.8f + ((hash >> 24) & 0xFF) / 255f * 0.2f;  // Value: 0.8-1.0

            Color color = Color.HSVToRGB(h, s, v);
            color.a = 0.3f; // Semi-transparent overlay

            return color;
        }

        // FNV-1a 32-bit. Stable across runtimes/platforms
        // (string.GetHashCode() may be randomized per-process on modern .NET).
        private static uint StableHash(string s)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in s)
                {
                    hash ^= c;
                    hash *= 16777619u;
                }
                return hash;
            }
        }
    }
}
