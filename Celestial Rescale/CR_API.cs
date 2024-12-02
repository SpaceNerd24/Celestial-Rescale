using UnityEngine;
using CelestialRescale.Utilis;
using CelestialRescale.UI;

namespace CelestialRescale.API
{
    public static class CR_API
    {
        //private static readonly CelestialRescale CRInstance = new CelestialRescale();

        public static double GetScaleFactor()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
            {
                if (double.TryParse(node.GetValue("scaleFactor1"), out double parsedValue))
                {
                    return parsedValue;
                }
            }
            return 1;
        }

        public static bool GetDebug()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
            {
                if (bool.TryParse(node.GetValue("isDebug"), out bool parsedValue))
                {
                    return parsedValue;
                }
            }
            return false;
        }

        public static void toggleUI()
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
