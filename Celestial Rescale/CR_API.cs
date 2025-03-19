using UnityEngine;
using CelestialRescale.UI;

namespace CelestialRescale.API
{
    public static class CR_API
    {
        private static double GetConfigValue(string key)
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
            {
                if (double.TryParse(node.GetValue(key), out double parsedValue))
                {
                    return parsedValue;
                }
            }
            return 0;
        }

        private static bool GetConfigValue(string key, bool IsBoolean)
        {
            if (IsBoolean)
            {
                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
                {
                    if (bool.TryParse(node.GetValue(key), out bool parsedValue))
                    {
                        return parsedValue;
                    }
                }
            }
            return false;
        }

        public static double GetScaleFactor()
        {
            return GetConfigValue("scaleFactor1");
        }
        public static float GetScaleFactor2()
        {
            return ((float)GetConfigValue("scaleFactor1"));
        }

        public static double GetStarFactor()
        {
            return GetConfigValue("scaleFactor1") / 1.75;
        }

        public static double GetAtmoFactor()
        {
            return GetConfigValue("atmoFactor1");
        }

        public static double GetOffsetFactor()
        {
            return GetConfigValue("offsetFactor1");
        }

        public static bool GetDebug()
        {
            return GetConfigValue("isDebug", true);
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
