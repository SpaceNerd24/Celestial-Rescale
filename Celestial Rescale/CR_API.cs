using UnityEngine;
using CelestialRescale.UI;

namespace CelestialRescale.API
{
    public static class CR_API
    {
        private const double DefaultScaleFactor = 1;
        private const double DefaultStarFactor = 1;
        private const double DefaultAtmoFactor = 0;
        private const double DefaultOffsetFactor = 1;
        private const bool DefaultDebug = false;

        private static double GetConfigValue(string key, double defaultValue)
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
            {
                if (double.TryParse(node.GetValue(key), out double parsedValue))
                {
                    return parsedValue;
                }
            }
            return defaultValue;
        }

        private static bool GetConfigValue(string key, bool defaultValue)
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
            {
                if (bool.TryParse(node.GetValue(key), out bool parsedValue))
                {
                    return parsedValue;
                }
            }
            return defaultValue;
        }

        public static double GetScaleFactor()
        {
            return GetConfigValue("scaleFactor1", DefaultScaleFactor);
        }
        public static float GetScaleFactor2()
        {
            return ((float)GetConfigValue("scaleFactor1", DefaultScaleFactor));
        }

        public static double GetStarFactor()
        {
            return GetConfigValue("scaleFactor1", DefaultStarFactor) / 1.75;
        }

        public static double GetAtmoFactor()
        {
            return GetConfigValue("atmoFactor1", DefaultAtmoFactor);
        }

        public static double GetOffsetFactor()
        {
            return GetConfigValue("offsetFactor1", DefaultOffsetFactor);
        }

        public static bool GetDebug()
        {
            return GetConfigValue("isDebug", DefaultDebug);
        }

        public static void ToggleUI()
        {
            if (CR_UI.UICanvas == null)
            {
                CR_UI.ShowGUI();
                Debug.Log("[CelestialRescale] Showing UI");
            }
            else
            {
                CR_UI.Destroy();
                Debug.Log("[CelestialRescale] Closing UI");
            }
        }
    }
}
