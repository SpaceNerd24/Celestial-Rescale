using UnityEngine;
using Celestial_Rescale.Utilis;

namespace Celestial_Rescale.API
{
    public static class CR_API
    {
        //private static readonly CelestialRescale CRInstance = new CelestialRescale();

        public static double GetScaleFactor()
        {
            CelestialRescale CRInstance = new CelestialRescale();
            double scaleFactor = CRInstance.scaleFactor;
            return scaleFactor;
        }

        public static float GetScaleFactor2()
        {
            CelestialRescale CRInstance = new CelestialRescale();
            float scaleFactor2 = CRInstance.scaleFactor2;
            return scaleFactor2;
        }

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

        public static FloatCurve GetAtmoCurve(CelestialBody body, bool temp)
        {
            if (temp)
            {
                return body.atmosphereTemperatureCurve;
            } else
            {
                return body.atmospherePressureCurve;
            }
        }
    }
}
