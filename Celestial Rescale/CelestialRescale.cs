﻿using KSP.UI.Screens.Flight;
using UnityEngine;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class CelestialRescale : MonoBehaviour
    {
        double scaleFactor = 2;
        float scaleFactor2 = 2;

        public void Start()
        {
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body != null && body.pqsController != null)
                {
                    body.pqsController.ResetSphere();

                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.name);

                    double originalRadius = body.Radius;
                    double targetRadius = body.Radius * scaleFactor;

                    // Adjust other properties pt 1

                    body.Radius *= scaleFactor;
                    body.pqsController.radius *= scaleFactor;
                    body.pqsController.radiusDelta *= scaleFactor;
                    body.pqsController.radiusSquared *= scaleFactor;
                    body.pqsController.radiusMax *= scaleFactor;
                    body.pqsController.radiusMin *= scaleFactor;
                    body.Mass *= scaleFactor;

                    // Random stuff that probably does not work

                    //body.pqsController.radiusMax *= scaleFactor;
                    //body.pqsController.radiusMin *= scaleFactor;
                    if (body.pqsController != null) // Additional null check
                    {
                        body.pqsController.mapMaxHeight *= scaleFactor;
                    }

                    foreach (PQSMod pqsMod in body.pqsController.GetComponentsInChildren<PQSMod>())
                    {
                        if (pqsMod != null) // Additional null check
                        {
                            pqsMod.RebuildSphere();
                        }
                    }

                    if (body.name == "Kerbin" && body.isHomeWorld == true)
                    {
                        Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + " is home world");
                    }

                    // Update the Body?
                    body.pqsController.RebuildSphere();

                    // Log the new radius (If there is a new one)
                    if (body.Radius == originalRadius)
                    {
                        Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + body.name);
                    }
                    else if (body.Radius == targetRadius)
                    {
                        Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.Radius);
                    }
                    ResizeAtmosphere(body);
                    FixScaledSpace(body);
                    ResizeOceans(body);
                    ResizeOrbits(body);
                }
            }
        }

        private void ResizeAtmosphere(CelestialBody body)
        {
            double originalMaxAltitude = body.atmosphereDepth;
            double newMaxAltitude = body.atmosphereDepth * scaleFactor;

            if (body != null && body.atmosphere == true) // Additional null check
            {
                body.atmosphereAdiabaticIndex *= scaleFactor;
                body.atmospherePressureSeaLevel *= scaleFactor;
                body.atmosphereTemperatureSeaLevel *= scaleFactor;
                body.atmosphereDepth *= scaleFactor;
                Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.atmosphereDepth);
                body.pqsController.RebuildSphere();

                if (body.atmosphereDepth == originalMaxAltitude)
                {
                    Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + "No change in max altitude" + body.name);
                }
                else if (body.atmosphereDepth == newMaxAltitude)
                {
                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.atmosphereDepth + " max altitude fixed?");
                }
            }

            if (body != null && body.atmosphere)
            {
                foreach (LinearAtmosphereGauge atmosphereGauge in body.GetComponents<LinearAtmosphereGauge>())
                {
                    atmosphereGauge.gauge.maxValue *= scaleFactor2;
                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + "Atmo Guage thingy found");
                }


                if (body.afg != null)
                {
                    //body.afg.g *= scaleFactor2;
                    //body.afg.innerRadius *= scaleFactor2;
                    //body.afg.innerRadius2 *= scaleFactor2;
                    body.afg.outerRadius *= scaleFactor2;
                    body.afg.outerRadius2 *= scaleFactor2;
                    //body.afg.Km = scaleFactor2;
                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + "AFG node found");
                    body.afg.UpdateAtmosphere(body);
                }
                else
                {
                    Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + "No AFG node found");
                }

                FloatCurve curve = body.atmospherePressureCurve;
                body.atmospherePressureCurve = curve;
                body.maxAxialDot *= scaleFactor;

                // testing stuff

                body.CBUpdate();
                body.pqsController.RebuildSphere();
            }
        }

        private void ResizeOceans(CelestialBody body)
        {
            if (body != null && body.ocean) // Additional null check
            {
                body.oceanFogDensityPQSMult *= scaleFactor2;
                body.oceanFogPQSDepth *= scaleFactor;
                Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.oceanFogPQSDepth);
                body.pqsController.RebuildSphere();
            }

            
            foreach (PQSMod pqsMod in body.pqsController.GetComponentsInChildren<PQSMod>())
            {
                if (pqsMod != null && body != null && body.pqsController != null && pqsMod.sphere != null) // Additional null check
                {
                    pqsMod.RebuildSphere();

                    if (pqsMod.sphere != null && pqsMod.sphere.radius != body.pqsController.radius)
                    {
                        pqsMod.sphere.radius = body.pqsController.radius;
                    }
                }
            }

            // Old experimentation that did nothing but make me confused
            /*
            foreach (PQSCity pqsCity in body.pqsController.GetComponentsInChildren<PQSCity>())
            {
                if (pqsCity != null && body != null && body.pqsController != null) // Additional null check
                {
                    pqsCity.sphere.radius = body.Radius;
                    pqsCity.sphere.RebuildSphere();
                    //pqsCity.Orientate();
                }
            }

            foreach (PQSCity2 pqsCity2 in body.pqsController.GetComponentsInChildren<PQSCity2>())
            {
                if (pqsCity2 != null && body != null && body.pqsController != null) // Additional null check
                {
                    pqsCity2.sphere.radius = body.Radius;
                    pqsCity2.sphere.RebuildSphere();
                    //pqsCity.Orientate();
                }
            }
            */
        }

        private void ResizeOrbits(CelestialBody body)
        {
            if (body != null && body.orbit != null && body.orbitDriver != null)
            {
                double originalSemiMajorAxis = body.orbit.semiMajorAxis;

                body.orbit.semiMajorAxis *= scaleFactor;
                body.orbit.UpdateFromUT(Planetarium.GetUniversalTime());

                if (body.orbit.semiMajorAxis != originalSemiMajorAxis)
                {
                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.orbit.semiMajorAxis + " " + body.name + "semi-major axis");
                }
                else
                {
                    Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + "No change in semi-major axis" + body.name);
                }
            }
        }

        private void FixScaledSpace(CelestialBody body)
        {
            if (body != null && body.scaledBody != null) // Additional null check
            {
                ScaledSpaceFader[] faders = Resources.FindObjectsOfTypeAll<ScaledSpaceFader>();

                foreach (ScaledSpaceFader fader in faders)
                {
                    if (fader != null && fader.celestialBody == body) // Additional null check
                    {
                        // Modify the properties of the fader as needed
                        fader.fadeStart *= scaleFactor2;
                        fader.fadeEnd *= scaleFactor2;
                        break;
                    }
                }

                body.scaledBody.transform.localScale *= scaleFactor2;
            }
        }
    }
}