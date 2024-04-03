namespace Celestial_Rescale
{
    public static class CR_API
    {
        private static readonly CelestialRescale CRInstance = new CelestialRescale();

        public static double GetScaleFactor()
        {
            return CRInstance.scaleFactor;
        }

        public static float GetScaleFactor2()
        {
            return CRInstance.scaleFactor2;
        }

        public static void ChangeScaleFactor(double newScaleFactor)
        {
            CRInstance.scaleFactor = newScaleFactor;
        }

        public static void ChangeScaleFactor2(float newScaleFactor)
        {
            CRInstance.scaleFactor2 = newScaleFactor;
        }

        public static void ResetPlanets()
        {
                        
        }
    }
}
