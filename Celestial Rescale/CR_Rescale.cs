using CelestialRescale.PQS;
using CelestialRescale.Utilis;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CelestialRescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class CR_Rescale : MonoBehaviour
    {
        internal double scaleFactor = 1;
        internal double starFactor;
        internal double atmoFactor;
        internal double offsetFactor;
        internal string version = "1.0.0";

        internal bool isDebug;

        public void ConfigLoader()
        {
            Debug.Log("[CelestialRescale] Celestial Rescale version " + version + " starting");
            Debug.Log("[CelestialRescale] Starting the Config Loader");

            // main overall config
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
            {
                if (node != null)
                {
                    if (double.TryParse(node.GetValue("scaleFactor1"), out double parsedValue))
                    {
                        scaleFactor = parsedValue;
                    }

                    if (double.TryParse(node.GetValue("atmoFactor1"), out double parsedValue2))
                    {
                        if (parsedValue2 == 0)
                        {
                            atmoFactor = scaleFactor * 0.75;
                        }
                        else
                        {
                            atmoFactor = parsedValue2;
                        }
                    }

                    if (double.TryParse(node.GetValue("offsetFactor1"), out double parsedValue3))
                    {
                        offsetFactor = parsedValue3;
                    }

                    if (bool.TryParse(node.GetValue("isDebug"), out bool parsedValue4))
                    {
                        isDebug = parsedValue4;
                    }
                } else
                {
                    Debug.LogError("[CelestialRescale] Config node is null");
                }
            }
        }

        public void Start()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            ConfigLoader();
            starFactor = scaleFactor / 1.75;

            PlanetariumCamera mapCam = PlanetariumCamera.fetch;

            if (mapCam != null)
            {
                mapCam.maxDistance *= ((float)scaleFactor);
            }

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body != null && body.isStar == true)
                {
                    // Star

                    body.Mass *= starFactor;
                    body.Radius *= starFactor;

                    ResizeOrbits(body);
                    FixScaledSpace(body);
                }
                else if (body != null && scaleFactor <= 100 && body.pqsController == null)
                {
                    // Gas Giant?

                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.name);

                    double originalRadius = body.Radius;
                    double targetRadius = body.Radius * scaleFactor;
                    body.Radius *= scaleFactor;
                    body.Mass *= scaleFactor;

                    // Log the new radis (If there is a new one)
                    if (body.Radius == originalRadius)
                    {
                        Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + body.name + " is not working");
                        return;
                    }
                    else if (body.Radius == targetRadius)
                    {
                        Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.Radius + " new radius");
                    }

                    ResizeOrbits(body);
                    FixScaledSpace(body);

                    if (body.atmosphere)
                    {
                        ResizeAtmosphere(body, atmoFactor);
                    }
                }
                else if (body != null && body.pqsController != null && scaleFactor <= 100)
                {
                    // Normal

                    body.pqsController.ResetSphere();

                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.name);

                    double originalRadius = body.Radius;
                    double targetRadius = body.Radius * scaleFactor;

                    body.Radius *= scaleFactor;
                    body.pqsController.radius *= scaleFactor;
                    body.pqsController.radiusDelta *= scaleFactor;
                    body.pqsController.radiusSquared *= scaleFactor;
                    body.pqsController.radiusMax *= scaleFactor;
                    body.pqsController.radiusMin *= scaleFactor;
                    body.Mass *= scaleFactor;
                    body.sphereOfInfluence *= scaleFactor;

                    if (body.pqsController != null)
                    {
                        body.pqsController.mapMaxHeight *= scaleFactor;
                    }

                    foreach (PQSMod pqsMod in body.pqsController.GetComponentsInChildren<PQSMod>())
                    {
                        if (pqsMod != null && pqsMod.sphere != null)
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
                            if (pqsCity != null && body != null && body.pqsController != null)
                            {
                                pqsCity.sphere.radius = body.Radius;
                                pqsCity.sphere.RebuildSphere();
                                pqsCity.Orientate();
                            }
                        }

                        foreach (PQSCity2 pqsCity2 in body.pqsController.GetComponentsInChildren<PQSCity2>())
                        {
                            if (pqsCity2 != null && body != null && body.pqsController != null)
                            {
                                pqsCity2.sphere.radius = body.Radius;
                                pqsCity2.sphere.RebuildSphere();
                                pqsCity2.Orientate();
                            }
                        }
                    }

                    //body.pqsController.RebuildSphere();

                    // Log the new radius (If there is a new one)
                    if (body.Radius == originalRadius)
                    {
                        Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + "Radius scaled incorrectly");
                    }
                    else if (body.Radius == targetRadius)
                    {
                        Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + "Radius scaled correctly");
                    }

                    ResizeAtmosphere(body, atmoFactor);
                    ResizeOrbits(body);
                    FixScaledSpace(body);

                    CR_PQS pqsFix = new CR_PQS();
                    pqsFix.FixPQS(body, scaleFactor);
                }
            }

            stopwatch.Stop();
            Debug.Log($"[CelestialRescale] Completed in {stopwatch.Elapsed.TotalSeconds} Seconds");
        }

        private void ResizeOrbits(CelestialBody body)
        {
            if (body != null && body.orbit != null && body.orbitDriver != null && !body.isStar)
            {
                double originalSemiMajorAxis = body.orbit.semiMajorAxis;

                if (body.isStar)
                {
                    body.orbit.semiMajorAxis *= starFactor;
                }
                else
                {
                    body.orbit.semiMajorAxis *= scaleFactor;
                }

                body.orbit.UpdateFromUT(Planetarium.GetUniversalTime());

                if (body.orbit.semiMajorAxis != originalSemiMajorAxis)
                {
                    Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.orbit.semiMajorAxis + " " + body.name + " new semi-major axis");
                }
                else
                {
                    Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + "No change in semi-major axis" + body.name);
                }
            }
        }

        private void FixScaledSpace(CelestialBody body)
        {
            if (body != null && body.scaledBody != null)
            {
                ScaledSpaceFader[] faders = Resources.FindObjectsOfTypeAll<ScaledSpaceFader>();

                foreach (ScaledSpaceFader fader in faders)
                {
                    if (fader != null && fader.celestialBody == body)
                    {
                        Debug.Log("[CelestialRescale] Fader found from body: " + fader.celestialBody.name);
                        fader.fadeStart *= ((float)scaleFactor);
                        fader.fadeEnd *= ((float)scaleFactor);
                        break;
                    } else if (body.isStar && fader.celestialBody == body)
                    {
                        Debug.Log("[CelestialRescale] Star Fader found from body: " + fader.celestialBody.name);
                        fader.fadeStart *= ((float)starFactor);
                        fader.fadeEnd *= ((float)starFactor);
                        break;
                    }
                }

                if (!body.isStar)
                {
                    body.scaledBody.transform.localScale *= ((float)scaleFactor);
                } else
                {
                    body.scaledBody.transform.localScale *= ((float)starFactor);
                }
            }
        }

        public void ResizeAtmosphere(CelestialBody body, double atmoFactor)
        {
            if (body != null && body.atmosphere)
            {
                double newAtmoEnd = body.atmosphereDepth * atmoFactor;

                if (body.afg != null)
                {
                    body.afg.outerRadius *= ((float)atmoFactor);
                    body.afg.UpdateAtmosphere(body);
                }

                Normalize(body, body.atmosphereDepth);

                FixAtmoPresure(body, newAtmoEnd);

                FixMaxAltitude(body, newAtmoEnd);

                Normalize(body, 1 / body.atmosphereDepth);
            }
        }

        public void FixMaxAltitude(CelestialBody body, double newAtmoEnd)
        {
            body.atmosphereDepth = newAtmoEnd;
        }

        public void FixAtmoPresure(CelestialBody body, double top)
        {
            FloatCurve curve = body.atmospherePressureCurve;
            List<double[]> list = ReadCurve(curve);
            double maxAltitude = list.Last()[0];
            bool smoothEnd = list.Last()[1] == 0 && list.Count > 2;

            if (top > body.atmosphereDepth)
            {
                Extend(list, top);
            }
            else
            {
                Debug.Log("IDK this broke: " + " TOP: " + top + " atmoDepth: " + body.atmosphereDepth + " Max alt: " + maxAltitude);
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

            curve.Load(WriteCurve(list));
        }

        void Smooth(List<double[]> list)
        {
            FloatCurve curve = new FloatCurve();
            curve.Load(WriteCurve(list));

            double minPressure = list.Min(item => item[1]);
            double maxPressure = list.Max(item => item[1]);
            double range = maxPressure - minPressure;

            for (int i = 0; i < list.Count; i++)
            {
                double[] current = list[i];
                current[1] = (current[1] - minPressure) * maxPressure / range;

                if (i > 0)
                {
                    double[] previous = list[i - 1];
                    double dX = 0.01 * (current[0] - previous[0]);
                    double dY = current[1] - ((curve.Evaluate((float)(current[0] - dX)) - minPressure) * maxPressure / range);
                    current[2] = current[3] = dY / dX;
                }
            }

            list[list.Count - 1][2] = list[list.Count - 1][3] = 0;
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

        void TrimAtmo(List<double[]> list, double newAtmoEnd)
        {
            FloatCurve curve = new FloatCurve();
            curve.Load(WriteCurve(list));

            double[] lastKey = { newAtmoEnd, curve.Evaluate((float)newAtmoEnd) };

            for (int i = list.Count; i > 0; i--)
            {
                if (list[i - 2][0] < newAtmoEnd)
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
        }

        void Extend(List<double[]> list, double newAtmoEnd)
        {
            double dX = list[list.Count - 1][0] - list[list.Count - 2][0];
            double newAltitude = list[list.Count - 1][0];
            double[] K = Ketk(list);

            while (newAltitude < newAtmoEnd)
            {
                newAltitude += dX;
                double newPressure = KetY(newAltitude, list[list.Count - 1], K);
                double tangent = (newPressure - KetY(newAltitude - dX * 0.01, list[list.Count - 1], K)) / (dX * 0.01);

                if (newPressure < 0)
                {
                    if (list[list.Count - 1][1] == 0) break;
                    newPressure = 0;
                }

                list.Add(new[] { newAltitude, newPressure, tangent, tangent });
            }
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

        enum CurveTypes
        {
            Expo,
            Log,
            Poly
        }
        CurveTypes curve = CurveTypes.Expo;

        double[] Ketk(List<double[]> list)
        {
            double[] K = { };
            if (list.Count == 2)
            {
                K = new double[] { 0, (list[1][1] - list[0][1]) / (list[1][0] - list[0][0]) };
                curve = CurveTypes.Poly;
                return K;
            }
            double[] dY = { list[list.Count - 2][1] - list[list.Count - 3][1], list[list.Count - 1][1] - list[list.Count - 2][1] };
            double[] dX = { list[list.Count - 2][0] - list[list.Count - 3][0], list[list.Count - 1][0] - list[list.Count - 2][0] };
            double curvature = (dY[1] / dX[1] - dY[0] / dX[0]) / ((dX[0] + dX[1]) / 2);

            if (curvature > 0)
            {
                K = new double[] { Math.Log(list.Last()[1] / list[list.Count - 2][1]) / dX[1] };
                curve = CurveTypes.Expo;
            }
            else if (curvature < 0 && dY[1] >= 0)
            {
                K = new double[] { dY[1] / Math.Log(list.Last()[0] / list[list.Count - 2][0]) };
                curve = CurveTypes.Log;
            }
            else
            {
                K = new double[] { curvature / 2, dY[1] / dX[1] - (list.Last()[0] + list[list.Count - 2][0]) * curvature / 2 };
                curve = CurveTypes.Poly;
            }

            return K;
        }

        double KetY(double X, double[] prevKey, double[] K)
        {
            double dX = X - prevKey[0];

            if (curve == CurveTypes.Expo)
            {
                return prevKey[1] * Math.Exp(dX * K[0]);
            }
            else if (curve == CurveTypes.Log)
            {
                return K[0] * Math.Log(X / prevKey[0]) + prevKey[1];
            }
            else
            {
                return dX * (K[0] * (X + prevKey[0]) + K[1]) + prevKey[1];
            }
        }

        internal static double getScaleFactor()
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
    }
}