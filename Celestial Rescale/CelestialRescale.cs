using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class CelestialRescale : MonoBehaviour
    {
        float scaleFactor2 = 1;
        double scaleFactor = 1;

        public static bool isDoingAtmospheres = true; // make this true during release and most times if it works
        public static bool usingBrokenWay = true; // make this false unless testing or it suddenly works

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
            AtmosphereStart();
            //this is dumb
            /*
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Celestial Rescale",
            "Celestial Rescale v0.3.0",
            "Starting the Config Loader", "IDK", true, HighLogic.UISkin,
            true, string.Empty);
            */

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body != null && body.isStar == true)
                {
                    body.Mass *= scaleFactor;
                    body.Radius *= scaleFactor;
                    ResizeOrbits(body);
                    FixScaledSpace(body);
                }

                if (body != null && scaleFactor <= 100 && body.isStar)
                {

                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.name);

                    double originalRadius = body.Radius;
                    double targetRadius = body.Radius * scaleFactor;

                    // Adjust other properties pt 1

                    body.Radius *= scaleFactor;
                    body.Mass *= scaleFactor;

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
                    ResizeOrbits(body);
                }

                if (body != null && body.pqsController != null && scaleFactor <= 100)
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

                        foreach (PQSCity pqsCity in body.pqsController.GetComponentsInChildren<PQSCity>())
                        {
                            if (pqsCity != null && body != null && body.pqsController != null) // Additional null check
                            {
                                pqsCity.sphere.radius = body.Radius;
                                pqsCity.sphere.RebuildSphere();
                                pqsCity.Orientate();
                            }
                        }

                        foreach (PQSCity2 pqsCity2 in body.pqsController.GetComponentsInChildren<PQSCity2>())
                        {
                            if (pqsCity2 != null && body != null && body.pqsController != null) // Additional null check
                            {
                                pqsCity2.sphere.radius = body.Radius;
                                pqsCity2.sphere.RebuildSphere();
                                pqsCity2.Orientate();
                            }
                        }
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
                //body.pqsController.RebuildSphere();

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
                if (body.afg != null && body.isStar == false)
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
                        "Version v0.3.0",
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
                            new DialogGUIButton("Reload Configs",
                                delegate
                                {
                                    Debug.Log("[CelestialRescale]" + " Reloading Config");
                                    ConfigLoader();
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

        void AtmosphereStart()
        {
            Debug.Log("did I break the game");
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body != null && body.atmosphere && isDoingAtmospheres == true && usingBrokenWay == true)
                {
                    Debug.Log("Using Broken Mode");
                    PrintCurve(body, "First Curves");

                    /*
                    // please work EDIT: nevermind its broken
                    FloatCurve scaledpresurecurve = ScaleCurve(body.atmospherePressureCurve, body);
                    body.atmospherePressureCurve = scaledpresurecurve;
                    */

                    ExtendAtmo1(body.atmospherePressureCurve);
                    ExtendAtmo1(body.atmosphereTemperatureCurve);

                    ExtendAtmo2(body.atmospherePressureCurve, body);
                    ExtendAtmo2(body.atmosphereTemperatureCurve, body);

                    PrintCurve(body, "Final Curves");
                }
                if (body != null && body.atmosphere && isDoingAtmospheres == true)
                {
                    Debug.Log("Using Slightly Less Broken");
                    body.atmospherePressureCurveIsNormalized = true;
                    Debug.Log("New stuff " + body.atmospherePressureCurveIsNormalized + " Name: " + body.displayName);
                }
            }
        }

        public void ExtendAtmo1(FloatCurve curve)
        {
            List<double[]> list = new List<double[]>();
            list = ReadCurve(curve);
            foreach (double[] key in list)
            {
                key[0] *= scaleFactor;
                key[2] /= scaleFactor;
                key[3] /= scaleFactor;
            }
            curve.Load(WriteCurve(list));
        }


        FloatCurve ScaleCurve(FloatCurve originalcurve, CelestialBody body)
        {
            Debug.Log("originalcurve: " + originalcurve + " Name: " + body.name);
            FloatCurve newcurve = originalcurve;
            ConfigNode config = new ConfigNode();
            newcurve.Save(config);

            string[] array = config.GetValues("key");
            for (int i = 0; i < array.Length; i++)
            {
                string key = array[i];
                Debug.Log("CelestialRescale.ScaleCurve - original key = " + key + " Name: " + body.name);

                double doublekey;
                if (double.TryParse(key, out doublekey))
                {
                    doublekey *= scaleFactor;
                    key = doublekey.ToString();
                    Debug.Log("CelestialRescale.ScaleCurve - final key = " + key + " Name: " + body.name);
                }
                else
                {
                    Debug.LogError("CelestialRescale.ScaleCurve - Invalid key: " + key + " Body: " + body.name);
                }
            }

            return newcurve;
        }

        void ExtendAtmo2(FloatCurve curve, CelestialBody body)
        {
            double atmotop = body.atmosphereDepth;
            List<double[]> list = new List<double[]>();
            list = ReadCurve(curve);

            double newAltitude = list.Last()[0];
            double dX = list.Last()[0] - list[list.Count - 2][0];
            double[] K = getK(list);

            for (int i = 0; newAltitude < atmotop; i++)
            {
                newAltitude += dX;
                double newPressure = getY(newAltitude, list.Last(), K);
                double tangent = (newPressure - getY(newAltitude - dX * 0.01, list.Last(), K)) / (dX * 0.01);

                double[] newKey = { newAltitude, newPressure, tangent, tangent };

                if (newKey[1] < 0)
                {
                    if (list.Last()[1] == 0)
                        break;
                    else
                        newKey[1] = 0;
                }

                list.Add(newKey);
            }

            // Debug
            PrintCurve(list, "Extend");
        }


        List<double[]> ReadCurve(FloatCurve curve)
        {
            ConfigNode config = new ConfigNode();
            List<double[]> list = new List<double[]>();
            ParserBS<double> value = new ParserBS<double>();
            

            curve.Save(config);

            foreach (string k in config.GetValues("key"))
            {
                value.SetFromString(k);
                list.Add(value.Value.ToArray());
            }

            return list;
        }

        ConfigNode WriteCurve(List<double[]> list)
        {
            ConfigNode config = new ConfigNode();

            foreach (double[] values in list)
            {
                string key = "";
                foreach (double value in values)
                {
                    key += value + " ";
                }
                config.AddValue("key", key);
            }

            return config;
        }

        void PrintCurve(CelestialBody body, string name)
        {
            Debug.Log("Doing Curve Stuff " + name + " for body " + body.name);
            FinalCurveStuff(body.atmospherePressureCurve, "pressureCurve");
            FinalCurveStuff(body.atmosphereTemperatureCurve, "temperatureCurve");
            FinalCurveStuff(body.atmosphereTemperatureSunMultCurve, "temperatureSunMultCurve");
            Debug.Log("curve1");
        }

        void PrintCurve(List<double[]> list, string name)
        {
            ConfigCurveStuff(WriteCurve(list), name);
            Debug.Log("curve2");
        }

        void ConfigCurveStuff(ConfigNode config, string name)
        {
            FloatCurve curve = new FloatCurve();
            curve.Load(config);
            FinalCurveStuff(curve, name);
            Debug.Log("curve3");
        }

        void FinalCurveStuff(FloatCurve curve, string name)
        {
            ConfigNode config = new ConfigNode();
            curve.Save(config);
            Debug.Log("CelestialRescale.AtmosphereFixer " + name);
            Debug.Log("curve4");
            foreach (string key in config.GetValues("key"))
            {
                Debug.Log("CelestialRescale.AtmosphereFixer " + "key = " + key);
            }
        }

        enum Ktype
        {
            Exponential,
            Logarithmic,
            Polynomial
        }
        Ktype curve = Ktype.Exponential;

        double[] getK(List<double[]> list)
        {
            double[] K = { };
            if (list.Count == 2)
            {
                // Polynomial Curve:    dY = dX * ( K0 * (X0 + X1) + K1 )
                K = new double[] { 0, (list[1][1] - list[0][1]) / (list[1][0] - list[0][0]) };
                curve = Ktype.Polynomial;
                return K;
            }
            double[] dY = { list[list.Count - 2][1] - list[list.Count - 3][1], list[list.Count - 1][1] - list[list.Count - 2][1] };
            double[] dX = { list[list.Count - 2][0] - list[list.Count - 3][0], list[list.Count - 1][0] - list[list.Count - 2][0] };
            double curvature = (dY[1] / dX[1] - dY[0] / dX[0]) / ((dX[0] + dX[1]) / 2);

            if (curvature > 0)
            {
                // Exponential Curve:   Y1/Y0 = EXP(dX * K);
                K = new double[] { Math.Log(list.Last()[1] / list[list.Count - 2][1]) / dX[1] };
                curve = Ktype.Exponential;
            }
            else if (curvature < 0 && dY[1] >= 0)
            {
                // Logarithmic Curve:   dY = K * LN(X1/X0);
                K = new double[] { dY[1] / Math.Log(list.Last()[0] / list[list.Count - 2][0]) };
                curve = Ktype.Logarithmic;
            }
            else
            {
                // Polynomial Curve:    dY = dX * ( K0 * (X0 + X1) + K1 )
                K = new double[] { curvature / 2, dY[1] / dX[1] - (list.Last()[0] + list[list.Count - 2][0]) * curvature / 2 };
                curve = Ktype.Polynomial;
            }

            return K;
        }

        double getY(double X, double[] prevKey, double[] K)
        {
            double dX = X - prevKey[0];

            if (curve == Ktype.Exponential)
            {
                // Exponential Curve:   Y1/Y0 = EXP(dX * K);
                return prevKey[1] * Math.Exp(dX * K[0]);
            }
            else if (curve == Ktype.Logarithmic)
            {
                // Logarithmic Curve:   dY = K * LN(X1/X0);
                return K[0] * Math.Log(X / prevKey[0]) + prevKey[1];
            }
            else
            {
                // Polynomial Curve:    dY = dX * ( K0 * (X0 + X1) + K1 )
                return dX * (K[0] * (X + prevKey[0]) + K[1]) + prevKey[1];
            }
        }
    }
}