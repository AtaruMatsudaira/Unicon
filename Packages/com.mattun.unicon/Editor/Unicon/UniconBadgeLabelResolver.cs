using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unicon
{
    internal static class UniconBadgeLabelResolver
    {
        private static string s_cachedLabel;
        private static bool s_cacheValid;
        private static string s_lastError;

        // Non-null while the last provider resolution failed; shown in Preferences.
        public static string LastError
        {
            get { return s_lastError; }
        }

        public static string GetBadgeLabel()
        {
            if (UniconSettings.BadgeTextSource == BadgeTextSource.DirectText)
            {
                return UniconSettings.BadgeText ?? "";
            }

            // Provider results (including failures) are cached so that GetLabel()
            // and warning logs do not run on the periodic re-apply loop.
            if (!s_cacheValid)
            {
                s_cachedLabel = ResolveFromProvider();
                s_cacheValid = true;
            }

            return s_cachedLabel;
        }

        public static void Invalidate()
        {
            s_cacheValid = false;
            s_cachedLabel = null;
            s_lastError = null;
        }

        public static List<Type> GetProviderTypes()
        {
            var result = new List<Type>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IUniconLabelWrappable>())
            {
                if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    continue;
                }

                result.Add(type);
            }

            // TypeCache order is undefined; sort for a stable popup and deterministic resolution.
            result.Sort((a, b) => string.CompareOrdinal(a.FullName, b.FullName));
            return result;
        }

        private static string ResolveFromProvider()
        {
            string typeName = UniconSettings.BadgeLabelProviderTypeName;
            if (string.IsNullOrEmpty(typeName))
            {
                s_lastError = "No label provider selected.";
                Debug.LogWarning($"Unicon: {s_lastError} Badge text will be empty.");
                return "";
            }

            Type providerType = null;
            foreach (Type type in GetProviderTypes())
            {
                if (type.FullName == typeName)
                {
                    providerType = type;
                    break;
                }
            }

            if (providerType == null)
            {
                s_lastError = $"Label provider type '{typeName}' was not found.";
                Debug.LogWarning($"Unicon: {s_lastError} Badge text will be empty.");
                return "";
            }

            try
            {
                var provider = (IUniconLabelWrappable)Activator.CreateInstance(providerType);
                string label = provider.GetLabel();
                s_lastError = null;
                return label ?? "";
            }
            catch (Exception ex)
            {
                s_lastError = $"Label provider '{typeName}' threw an exception: {ex.Message}";
                Debug.LogWarning($"Unicon: {s_lastError} Badge text will be empty.");
                return "";
            }
        }
    }
}
