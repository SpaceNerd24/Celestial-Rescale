using UnityEngine;
using CelestialRescale.Utilis;

namespace CelestialRescale.API
{
    public static class CR_API
    {
        //private static readonly CelestialRescale CRInstance = new CelestialRescale();

        public static double GetScaleFactor()
        {
            CR_Rescale CRInstance = new CR_Rescale();
            double scaleFactor = CRInstance.scaleFactor;
            return scaleFactor;
        }

        public static float GetScaleFactor2()
        {
            CR_Rescale CRInstance = new CR_Rescale();
            float scaleFactor2 = CRInstance.scaleFactor2;
            return scaleFactor2;
        }

        // not working currently
        /* 
        public static void ChangeScaleFactor(double newScaleFactor)
        {
            CelestialRescale CRInstance = new CelestialRescale();
            CRInstance.scaleFactor = newScaleFactor;
        }
        */

        public static void ResetPlanets()
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
