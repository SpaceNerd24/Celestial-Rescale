using KSP.UI.Screens.Flight;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class CelestialRescale : MonoBehaviour
    {

        public double scaleFactor = 2;
        public float scaleFactor2 = 2;

        public void ConfigLoader()
        {
            string filePath = "GameData/CelestialRescale/CelestialRescaleLocalSettings.cfg";

            Debug.Log("Loading settings from " + filePath);

            Dictionary<string, string> config = new Dictionary<string, string>();

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split('=');

                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    if (key == "scaleFactor" && value != null)
                    {
                        Debug.Log("scaleFactor: " + value + " " + scaleFactor);
                        scaleFactor = float.Parse(value);
                    }
                    else if (key == "scaleFactor2" && value != null)
                    {
                        Debug.Log("scaleFactor2 : " + value + " " + scaleFactor2);
                        scaleFactor2 = float.Parse(value);
                    }
                    else if (value != null)
                    {
                        Debug.Log("Unknown key: " + key + " " + value);
                    }
                    else
                    {
                        Debug.Log("why did this break? " + key + " " + value);
                    }
                }
            }
        }

        public void Start()
        {
            ConfigLoader();
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Celestial Rescale",
            "Celestial Rescale v0.3.0",
            "Starting the Config Loader", "IDK", true, HighLogic.UISkin,
            true, string.Empty);

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
                        if (pqsMod != null && pqsMod.sphere != null) // Additional null check
                        {
                            pqsMod.sphere.radius = body.pqsController.radius;
                            pqsMod.RebuildSphere();
                        }
                    }

                    if (body.isHomeWorld == true)
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
                if (body.afg != null)
                {
                    //body.afg.g *= scaleFactor2;
                    //body.afg.innerRadius *= scaleFactor2;
                    //body.afg.innerRadius2 *= scaleFactor2;
                    body.afg.outerRadius *= scaleFactor2;
                    body.afg.outerRadius2 *= scaleFactor2;
                    //body.afg.Km = scaleFactor2;
                    //body.afg.scale = scaleFactor2;
                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + "AFG node found");
                    body.afg.UpdateAtmosphere(body);
                }
                else
                {
                    Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + "No AFG node found");
                }

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
                        Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + pqsMod.sphere.radius + " pqs sphere radius");
                    }
                }
            }

            // Old experimentation that did nothing but make me confused

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
        public void Update()
        {
            bool isToggling = GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.C) && HighLogic.LoadedScene == GameScenes.MAINMENU;
            if (isToggling)
            {
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new MultiOptionDialog("",
                        "v0.1.4",
                        "Celestial Rescale",
                        HighLogic.UISkin,
                        new Rect(0.5f, 0.5f, 150f, 60f),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIVerticalLayout(new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Rescale Planets (Warning: will break your game)",
                                delegate
                                {
                                    Debug.Log("[CelestialRescale]" + " Rescaling planets through user input");
                                    Start();
                                }, 140.0f, 30.0f, false),
                            new DialogGUIButton("Close",
                                delegate
                                {
                                    Debug.Log("[CelestialRescale]" + " Closing Ui");
                                }, 140.0f, 30.0f, true)
                            )),
                    false,
                    HighLogic.UISkin); ;
            }
        }
    }
}