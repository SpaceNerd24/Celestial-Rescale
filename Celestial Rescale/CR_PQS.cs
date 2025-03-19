using System.Reflection;
using UnityEngine;

namespace CelestialRescale.PQS
{
    internal class CR_PQS
    {
        public void FixPQS(CelestialBody body, double scaleFactor)
        {
            if (body == null)
            {
                Debug.Log("[Celestial Rescale] No body to fix PQS on");
                return;
            }

            foreach (PQSMod mod in body.GetComponentsInChildren<PQSMod>())
            {
                if (mod != null)
                {
                    scaleMod(scaleFactor, mod);
                }
            }

            // Old Code from CR_Rescale.cs
            if (body != null && body.ocean)
            {
                body.oceanFogDensityPQSMult *= ((float)scaleFactor);
                body.oceanFogPQSDepth *= scaleFactor;
                Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.oceanFogPQSDepth);
            }

            body.pqsController.RebuildSphere();
        }

        internal void scaleMod(double scaleFactor, PQSMod mod)
        {
            FieldInfo[] fields = mod.GetType().GetFields();

            // Check for radius
            foreach (FieldInfo field in fields)
            {
                if (field.Name == "radius")
                {
                    double newRadius = (double)field.GetValue(mod) * scaleFactor;
                    field.SetValue(mod, newRadius);
                }
            }
        }
    }
}
