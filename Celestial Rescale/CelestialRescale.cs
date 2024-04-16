using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using Celestial_Rescale.API;
using Celestial_Rescale.Utilis;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class CelestialRescale : MonoBehaviour
    {
        internal float scaleFactor2 = 1;
        internal double scaleFactor = 1;

        public static bool isDebug = true;
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
                    body.sphereOfInfluence *= scaleFactor;

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
                    
                    AtmosphereStart(body);
                    FixScaledSpace(body);
                    ResizeOceans(body);
                    ResizeOrbits(body);
                    BetterTerrainRescaling(body);
                }
            }

            //CR_API.ResetPlanets();
        }

        /*
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
        */

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

                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    if (body.isHomeWorld)
                    {

                        bool isChecking = Input.GetKeyDown(KeyCode.D);
                        if (isChecking)
                        {
                            List<double[]> list = ReadCurve(body.atmospherePressureCurve);
                            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Celestial Rescale",
                            "Celestial Rescale v0.3.0",
                               Convert.ToString(list)
                            , "Close", true, HighLogic.UISkin,
                            true, string.Empty);
                        }
                    }
                }
            }
        }

        public void AtmosphereStart(CelestialBody body)
        {
            Debug.Log("did I break the game");
            if (body != null && body.atmosphere && isDoingAtmospheres == true && usingBrokenWay == true)
            {
                double topLayer = body.atmosphereDepth * scaleFactor;
                Debug.Log("Using Broken Mode");
                PrintCurve(body, "First Curves");

                body.afg.outerRadius *= scaleFactor2;
                body.afg.UpdateAtmosphere(body);

                Normalize(body, body.atmosphereDepth);

                FixAtmoPresure(body, topLayer);

                QuickFix(body.atmosphereTemperatureCurve, topLayer);

                QuickFix(body.atmosphereTemperatureSunMultCurve, topLayer);

                FixMaxAltitude(body, topLayer);

                Normalize(body, 1 / body.atmosphereDepth);

                PrintCurve(body, "Final Curves");
            }
            if (body != null && body.atmosphere && isDoingAtmospheres == true && usingBrokenWay == false)
            {
                Debug.Log("Using Slightly Less Broken");
                body.atmospherePressureCurveIsNormalized = true;
                Debug.Log("New stuff " + body.atmospherePressureCurveIsNormalized + " Name: " + body.displayName);
            }
        }

        public void FixMaxAltitude(CelestialBody body, double topLayer)
        {
            body.atmosphereDepth = topLayer;
        }

        public void FixAtmoPresure(CelestialBody body, double top)
        {
            FloatCurve curve = body.atmospherePressureCurve;
            List<double[]> list = ReadCurve(curve);
            double maxAltitude = list.Last()[0];
            bool smoothEnd = list.Last()[1] == 0 && list.Count > 2;

            if (body.transform.name == "Kerbin" && list.Count > 0) { list.RemoveAt(0); }
            
            if (top > body.atmosphereDepth)
            {
                Extend(list, top);
            } else
            {
                Debug.Log("IDK this broke: " + " TOP: " + top + " atmodDepth: " + body.atmosphereDepth + " Max alt: " + maxAltitude);
            }

            if (smoothEnd) { list.RemoveAt(list.Count - 1); }

            if (top < maxAltitude)
            {
                TrimAtmo(list, top);
            }

            if (smoothEnd)
            {
                Smooth(list);
            }

            if (body.transform.name == "Kerbin") { list.Insert(0, new[] { 0, 101.325, 0, 0, }); }

            curve.Load(WriteCurve(list));
        }

        void Smooth(List<double[]> list)
        {
            FloatCurve curve = new FloatCurve();
            curve.Load(WriteCurve(list));
            double minPressure = list.First()[1];
            double maxPressure = list.First()[1];

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i][1] < minPressure)
                    minPressure = list[i][1];
                if (list[i][1] > maxPressure)
                    maxPressure = list[i][1];
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i][1] = (list[i][1] - minPressure) * maxPressure / (maxPressure - minPressure);

                if (i > 0)
                {
                    double dX = 0.01 * (list[i][0] - list[i - 1][0]);
                    double dY = list[i][1] - ((curve.Evaluate((float)(list[i][0] - dX)) - minPressure) * maxPressure / (maxPressure - minPressure));
                    list[i][2] = dY / dX;
                    list[i][3] = dY / dX;
                }
            }

            list.Last()[2] = 0;
            list.Last()[3] = 0;

            // Debug
            PrintCurve(list, "Smooth");
        }

        public void Normalize(CelestialBody body, double altitude)
        {
            if (body.atmospherePressureCurveIsNormalized)
                ExtendAtmo1(body.atmospherePressureCurve, altitude);

            if (body.atmosphereTemperatureCurveIsNormalized)
                ExtendAtmo1(body.atmosphereTemperatureCurve, altitude);
        }

        void ExtendAtmo1(FloatCurve curve, double multiplier)
        {
            List<double[]> list = new List<double[]>();
            list = ReadCurve(curve);
            foreach (double[] key in list)
            {
                key[0] *= multiplier;
                key[2] /= multiplier;
                key[3] /= multiplier;
            }
            curve.Load(WriteCurve(list));
        }

        void TrimAtmo(List<double[]> list, double topLayer)
        {
            FloatCurve curve = new FloatCurve();
            curve.Load(WriteCurve(list));

            double[] lastKey = { topLayer, curve.Evaluate((float)topLayer) };

            for (int i = list.Count; i > 0; i--)
            {
                if (list[i - 2][0] < topLayer)
                {
                    double dX = 0.01 * (lastKey[0] - list[i - 2][0]);
                    double dY = lastKey[1] - curve.Evaluate((float)(lastKey[0] - dX));
                    double tangent = dY / dX;

                    list.RemoveAt(i - 1);

                    list.Add(new double[] { lastKey[0], lastKey[1], tangent, tangent });
                    break;
                }
                else
                {
                    list.RemoveAt(i - 1);
                }
            }

            // Debug
            PrintCurve(list, "Trim");
        }

        void Extend(List<double[]> list, double topLayer)
        {
            double newAltitude = list.Last()[0];
            double dX = list.Last()[0] - list[list.Count - 2][0];
            double[] K = Ketk(list);

            for (int i = 0; newAltitude < topLayer; i++)
            {
                newAltitude += dX;
                double newPressure = KetY(newAltitude, list.Last(), K);
                double tangent = (newPressure - KetY(newAltitude - dX * 0.01, list.Last(), K)) / (dX * 0.01);

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
            MainParser<double> value = new MainParser<double>();
            

            curve.Save(config);

            foreach (string k in config.GetValues("key"))
            {
                value.SetFromString(k);
                list.Add(value.Value.ToArray());
            }

            return list;
        }

        public void QuickFix(FloatCurve curve, double topLayer)
        {
            if (topLayer > curve.maxTime)
            {
                List<double[]> list = ReadCurve(curve); /* Avoid Bad Curves ==> */ if (list.Count == 0) { Debug.Log("AtmosphereTopLayer.QuickFix" + " This curve is pointless."); return; }
                list.Last()[3] = 0;
                list.Add(new double[] { topLayer, list.Last()[1], 0, 0 });
                curve.Load(WriteCurve(list));
            }
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
            //FinalCurveStuff(body.atmosphereTemperatureCurve, "temperatureCurve");
            //FinalCurveStuff(body.atmosphereTemperatureSunMultCurve, "temperatureSunMultCurve");
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

        enum IDK
        {
            Exponential,
            Logarithmic,
            Polynomial
        }
        IDK curve = IDK.Exponential;

        double[] Ketk(List<double[]> list)
        {
            double[] K = { };
            if (list.Count == 2)
            {
                K = new double[] { 0, (list[1][1] - list[0][1]) / (list[1][0] - list[0][0]) };
                curve = IDK.Polynomial;
                return K;
            }
            double[] dY = { list[list.Count - 2][1] - list[list.Count - 3][1], list[list.Count - 1][1] - list[list.Count - 2][1] };
            double[] dX = { list[list.Count - 2][0] - list[list.Count - 3][0], list[list.Count - 1][0] - list[list.Count - 2][0] };
            double curvature = (dY[1] / dX[1] - dY[0] / dX[0]) / ((dX[0] + dX[1]) / 2);

            if (curvature > 0)
            {
                K = new double[] { Math.Log(list.Last()[1] / list[list.Count - 2][1]) / dX[1] };
                curve = IDK.Exponential;
            }
            else if (curvature < 0 && dY[1] >= 0)
            {
                K = new double[] { dY[1] / Math.Log(list.Last()[0] / list[list.Count - 2][0]) };
                curve = IDK.Logarithmic;
            }
            else
            {
                K = new double[] { curvature / 2, dY[1] / dX[1] - (list.Last()[0] + list[list.Count - 2][0]) * curvature / 2 };
                curve = IDK.Polynomial;
            }

            return K;
        }

        double KetY(double X, double[] prevKey, double[] K)
        {
            double dX = X - prevKey[0];

            if (curve == IDK.Exponential)
            {
                return prevKey[1] * Math.Exp(dX * K[0]);
            }
            else if (curve == IDK.Logarithmic)
            {
                return K[0] * Math.Log(X / prevKey[0]) + prevKey[1];
            }
            else
            {
                return dX * (K[0] * (X + prevKey[0]) + K[1]) + prevKey[1];
            }
        }

        internal void BetterTerrainRescaling(CelestialBody body)
        {
            if (body == null && body.pqsController != null)
            {
                foreach (PQSCity pqs in body.pqsController.GetComponentsInChildren<PQSCity>())
                {
                    pqs.repositionToSphereSurface = true;
                    pqs.reorientToSphere = true;

                    pqs.RebuildSphere();
                }

                foreach (PQSCity2 pqs in body.pqsController.GetComponentsInChildren<PQSCity2>())
                {
                    pqs.RebuildSphere();
                }
            }
        }
    }
}