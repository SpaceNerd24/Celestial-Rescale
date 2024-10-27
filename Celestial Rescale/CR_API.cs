using UnityEngine;
using CelestialRescale.Utilis;

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

        public static float GetScaleFactor2()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
            {
                if (double.TryParse(node.GetValue("scaleFactor1"), out double parsedValue))
                {
                    return ((float)parsedValue);
                }
            }
            return 1;
        }

        // not working currently
        /* 
        public static void ChangeScaleFactor(double newScaleFactor)
        {
            CelestialRescale CRInstance = new CelestialRescale();
            CRInstance.scaleFactor = newScaleFactor;
        }
        */

        internal static void ResetPlanets()
        {
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                CR_Utilis.ResetBody(body);
                Debug.Log("[CelestialRescale] Reseting Body: " + body);
            }
        }

        public static bool GetDebug()
        {
            CR_Rescale CRInstance = new CR_Rescale();
            return CRInstance.isDebug;
        }
    }
}
