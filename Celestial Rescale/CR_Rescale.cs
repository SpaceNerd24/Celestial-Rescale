using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using CelestialRescale.Utilis;
using CelestialRescale.UI;

namespace CelestialRescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class CR_Rescale : MonoBehaviour
    {
        internal double scaleFactor = 1;
        internal double starFactor;
        internal double atmoFactor;
        internal double offsetFactor;

        internal bool isDebug;

        public void ConfigLoader()
        {
            Debug.Log("Starting the Config Loader");

            // main overall config
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("CelestialRescale"))
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
            }
        }

        public void Start()
        {
            ScreenMessages.PostScreenMessage("Starting Celestial Rescale");
            ConfigLoader();
            CR_Utilis.LoadDictionaries();
            CR_Utilis.LoadKSCOriganlPOS();
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
                    body.Mass *= starFactor;
                    body.Radius *= starFactor;
                    ResizeOrbits(body);
                    FixScaledSpace(body);
                }
                else if (body != null && scaleFactor <= 100 && body.pqsController == null)
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
                        Debug.LogError("[CelestialRescale]" + " [" + body.name + "] " + body.name + " is not working");
                    }
                    else if (body.Radius == targetRadius)
                    {
                        Debug.Log("[CelestialRescale]" + " [" + body.name + "] " + body.Radius + " new radius");
                    }
                    ResizeOrbits(body);
                    FixScaledSpace(body);
                    if (body.atmosphere)
                    {
                        AtmosphereStart(body, atmoFactor);
                    }
                }
                else if (body != null && body.pqsController != null && scaleFactor <= 100)
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

                    if (body.pqsController != null)
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
 
                    AtmosphereStart(body, atmoFactor);
                    ResizeOceans(body);
                    ResizeOrbits(body);
                    FixScaledSpace(body);
                }
            }
            FixPQSMainBody();

            if (CR_UI.UICanvas == null)
            {
                CR_UI.ShowGUI();
                Debug.Log("[CelestialRescale] Showing UI");
            }
            else
            {
                CR_UI.Destroy();
                Debug.Log("[CelestialRescale] Closing UI");
            }
        }

        private void ResizeOceans(CelestialBody body)
        {
            if (body != null && body.ocean) // Additional null check
            {
                body.oceanFogDensityPQSMult *= ((float)scaleFactor);
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
            if (body != null && body.orbit != null && body.orbitDriver != null && !body.isStar)
            {
                double originalSemiMajorAxis = body.orbit.semiMajorAxis;

                body.orbit.semiMajorAxis *= scaleFactor;
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
            if (body != null && body.orbit != null && body.orbitDriver != null && body.isStar)
            {
                double originalSemiMajorAxis = body.orbit.semiMajorAxis;

                body.orbit.semiMajorAxis *= starFactor;
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
                        Debug.Log("[CelestialRescale] Fader found");
                        fader.fadeStart *= ((float)scaleFactor);
                        fader.fadeEnd *= ((float)scaleFactor);
                        break;
                    } else if (body.isStar)
                    {
                        Debug.Log("[CelestialRescale] Star Fader found");
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

        public void AtmosphereStart(CelestialBody body, double atmoFactor)
        {
            if (body != null && body.atmosphere)
            {
                double topLayer = body.atmosphereDepth * atmoFactor;

                if (body.afg != null)
                {
                    body.afg.outerRadius *= ((float)atmoFactor);
                    body.afg.UpdateAtmosphere(body);
                }

                Normalize(body, body.atmosphereDepth);

                FixAtmoPresure(body, topLayer);

                QuickFix(body.atmosphereTemperatureCurve, topLayer);

                QuickFix(body.atmosphereTemperatureSunMultCurve, topLayer);

                FixMaxAltitude(body, topLayer);

                Normalize(body, 1 / body.atmosphereDepth);
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
        }

        void Extend(List<double[]> list, double topLayer)
        {
            double dX = list[list.Count - 1][0] - list[list.Count - 2][0];
            double newAltitude = list[list.Count - 1][0];
            double[] K = Ketk(list);

            while (newAltitude < topLayer)
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

        public void QuickFix(FloatCurve curve, double topLayer)
        {
            if (topLayer > curve.maxTime)
            {
                List<double[]> list = ReadCurve(curve); if (list.Count == 0) { ; return; }
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

        enum CurveTypes
        {
            Exponential,
            Logarithmic,
            Polynomial
        }
        CurveTypes curve = CurveTypes.Exponential;

        double[] Ketk(List<double[]> list)
        {
            double[] K = { };
            if (list.Count == 2)
            {
                K = new double[] { 0, (list[1][1] - list[0][1]) / (list[1][0] - list[0][0]) };
                curve = CurveTypes.Polynomial;
                return K;
            }
            double[] dY = { list[list.Count - 2][1] - list[list.Count - 3][1], list[list.Count - 1][1] - list[list.Count - 2][1] };
            double[] dX = { list[list.Count - 2][0] - list[list.Count - 3][0], list[list.Count - 1][0] - list[list.Count - 2][0] };
            double curvature = (dY[1] / dX[1] - dY[0] / dX[0]) / ((dX[0] + dX[1]) / 2);

            if (curvature > 0)
            {
                K = new double[] { Math.Log(list.Last()[1] / list[list.Count - 2][1]) / dX[1] };
                curve = CurveTypes.Exponential;
            }
            else if (curvature < 0 && dY[1] >= 0)
            {
                K = new double[] { dY[1] / Math.Log(list.Last()[0] / list[list.Count - 2][0]) };
                curve = CurveTypes.Logarithmic;
            }
            else
            {
                K = new double[] { curvature / 2, dY[1] / dX[1] - (list.Last()[0] + list[list.Count - 2][0]) * curvature / 2 };
                curve = CurveTypes.Polynomial;
            }

            return K;
        }

        double KetY(double X, double[] prevKey, double[] K)
        {
            double dX = X - prevKey[0];

            if (curve == CurveTypes.Exponential)
            {
                return prevKey[1] * Math.Exp(dX * K[0]);
            }
            else if (curve == CurveTypes.Logarithmic)
            {
                return K[0] * Math.Log(X / prevKey[0]) + prevKey[1];
            }
            else
            {
                return dX * (K[0] * (X + prevKey[0]) + K[1]) + prevKey[1];
            }
        }

        // not that much better than my current system
        // STFU IT WORKS
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

        internal void FixPQSMainBody()
        {
            bool doPQSCity = false;
            CelestialBody body = FlightGlobals.GetHomeBody();
            if (body != null)
            {
                double scaleFactor = getScaleFactor();

                Debug.Log("[CelestialRescale] [KSCMover] Home Body is not null");
                Debug.Log("[CelestialRescale] [KSCMover] Home Body is: " + body.name);

                PQS pqsController = body.pqsController;
                if (pqsController != null)
                {
                    // Space Center Cam
                    foreach (SpaceCenterCamera camera in Resources.FindObjectsOfTypeAll<SpaceCenterCamera>())
                    {
                        camera.zoomInitial *= ((float)scaleFactor);
                        camera.zoomMax *= ((float)scaleFactor);
                        camera.zoomMin *= ((float)scaleFactor);
                    }
                    foreach (SpaceCenterCamera2 camera in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                    {
                        camera.zoomInitial *= ((float)scaleFactor);
                        camera.zoomMax *= ((float)scaleFactor);
                        camera.zoomMin *= ((float)scaleFactor);
                    }

                    // PQSCity
                    PQSCity city = body.GetComponent<PQSCity>();
                    if (city != null && doPQSCity)
                    {
                        Vector3 PlanetPOS = body.transform.position;
                        Vector3 buildingPOS = city.transform.position;
                        Vector3 CityLOC = (buildingPOS - PlanetPOS).normalized;

                        // Max distance
                        double maxDistance = Math.Abs(2 * pqsController.mapMaxHeight);
                        maxDistance *= scaleFactor * 1 > 1 ? scaleFactor * 1 : 1;
                        maxDistance += body.Radius;

                        RaycastHit[] hits = Physics.RaycastAll(PlanetPOS + CityLOC * (float)maxDistance, -CityLOC, (float)maxDistance, LayerMask.GetMask("Local Scenery"));

                        for (int i = 0; i < hits?.Length; i++)
                        {
                            if (hits[i].collider?.GetComponent<PQ>())
                            {
                                Debug.Log("Hit " + city.name);
                                // Parameters
                                double oldGroundLevel = pqsController.GetSurfaceHeight(city.repositionRadial) - body.Radius;
                                double oldOceanOffset = body.ocean && oldGroundLevel < 0 ? oldGroundLevel : 0d;
                                oldGroundLevel = body.ocean && oldGroundLevel < 0 ? 0d : oldGroundLevel;
                                double groundLevel = (hits[i].point - PlanetPOS).magnitude - body.Radius;
                                double oceanOffset = body.ocean && groundLevel < 0 ? groundLevel : 0d;
                                groundLevel = body.ocean && groundLevel < 0 ? 0d : groundLevel;

                                if (!city.repositionToSphere && !city.repositionToSphereSurface)
                                {
                                    double builtInOffset = (city.repositionRadiusOffset - body.Radius - oldGroundLevel - oceanOffset) / scaleFactor - (groundLevel + oceanOffset - oldGroundLevel - oldOceanOffset) / (scaleFactor * 1);
                                    city.repositionRadiusOffset = body.Radius + groundLevel + oceanOffset + builtInOffset * scaleFactor;
                                }
                                else if (city.repositionToSphere && !city.repositionToSphereSurface)
                                {
                                    double builtInOffset = (city.repositionRadiusOffset - oldGroundLevel) / scaleFactor - (groundLevel - oldGroundLevel) / (scaleFactor * 1);
                                    city.repositionRadiusOffset = groundLevel + builtInOffset * scaleFactor;
                                }
                                else
                                {
                                    double builtInOffset = city.repositionRadiusOffset / scaleFactor - (groundLevel + oceanOffset - oldGroundLevel - oldOceanOffset) / (scaleFactor * 1);
                                    city.repositionRadiusOffset = builtInOffset * scaleFactor + groundLevel + oceanOffset - oldGroundLevel - oldOceanOffset;
                                }
                            }
                        }

                        city.Orientate();
                    }

                    // PQSCity2
                    PQSCity2 city2 = body.GetComponent<PQSCity2>();
                    if (city2 != null && doPQSCity)
                    {
                        Vector3 PlanetPOS = body.transform.position;
                        Vector3 buildingPOS = city2.transform.position;
                        Vector3 CityLOC = (buildingPOS - PlanetPOS).normalized;

                        // Max distance
                        double maxDistance = Math.Abs(2 * pqsController.mapMaxHeight);
                        maxDistance *= scaleFactor * 1 > 1 ? scaleFactor * 1 : 1;
                        maxDistance += body.Radius;

                        RaycastHit[] hits = Physics.RaycastAll(PlanetPOS + CityLOC * (float)maxDistance, -CityLOC, (float)maxDistance, LayerMask.GetMask("Local Scenery"));

                        for (int i = 0; i < hits?.Length; i++)
                        {
                            if (hits[i].collider?.GetComponent<PQ>())
                            {

                            }

                            // Parameters
                            double oldGroundLevel = pqsController.GetSurfaceHeight(city2.PlanetRelativePosition) - body.Radius;
                            double oldOceanOffset = body.ocean && oldGroundLevel < 0 ? oldGroundLevel : 0d;
                            oldGroundLevel = body.ocean && oldGroundLevel < 0 ? 0d : oldGroundLevel;
                            double groundLevel = (hits[i].point - PlanetPOS).magnitude - body.Radius;
                            double oceanOffset = body.ocean && groundLevel < 0 ? groundLevel : 0d;
                            groundLevel = body.ocean && groundLevel < 0 ? 0d : groundLevel;

                            city2.PositioningPoint.localPosition /= (float)(body.Radius + city2.alt);

                            if (!city2.snapToSurface)
                            {
                                double builtInOffset = (city2.alt - oldGroundLevel) / scaleFactor - (groundLevel - oldGroundLevel) / (scaleFactor * 1);
                                city2.alt = groundLevel + builtInOffset * scaleFactor;
                            }
                            else
                            {
                                double builtInOffset = city2.snapHeightOffset / scaleFactor - (groundLevel + oceanOffset - oldGroundLevel - oldOceanOffset) / (scaleFactor * 1);
                                double newOffset = builtInOffset * scaleFactor + groundLevel + oceanOffset - oldGroundLevel - oldOceanOffset;
                                city2.alt += newOffset - city2.snapHeightOffset;
                                city2.snapHeightOffset = newOffset;
                            }
                        }

                        city2.PositioningPoint.localPosition *= (float)(body.Radius + city2.alt);

                        city2.Orientate();
                    }
                }

                PQSCity kscCity = CR_Utilis.kscCity;

                if (kscCity != null)
                {
                    Debug.Log("[CelestialRescale] [KSCMover] Moving the KSC");
                    double latitudeOffset = 5.0 * scaleFactor; // Move by scaleFactor degrees
                    double longitudeOffset = scaleFactor; // Move by scaleFactor degrees

                    // Adjust the latitude and longitude
                    double newLatitude = kscCity.lat;// - latitudeOffset;
                    double newLongitude = kscCity.lon - longitudeOffset * offsetFactor;

                    // Detect the rescaled planet's radius
                    double rescaledRadius = body.Radius;

                    // Calculate the new position based on the original latitude, longitude, and the new radius
                    kscCity.repositionToSphere = true;
                    kscCity.repositionRadial = body.GetRelSurfacePosition(newLatitude, newLongitude, rescaledRadius);

                    // Apply the changes
                    kscCity.repositionToSphereSurface = true;
                    kscCity.repositionToSphereSurfaceAddHeight = true;
                    kscCity.Orientate();
                    Debug.Log("[CelestialRescale] [KSCMover] " + $"KSC origial pos: Latitude = {CR_Utilis.originalLatitude}, Longitude = {CR_Utilis.originalLongitude}");
                    Debug.Log("[CelestialRescale] [KSCMover] " + $"KSC moved to new position: Latitude = {newLatitude}, Longitude = {newLongitude}");

                    body.scaledBody.transform.localScale /= (float)scaleFactor;

                    ScaledSpaceFader[] faders = Resources.FindObjectsOfTypeAll<ScaledSpaceFader>();

                    foreach (ScaledSpaceFader fader in faders)
                    {
                        if (fader != null && fader.celestialBody == body) // Additional null check
                        {
                            fader.transform.localScale *= ((float)scaleFactor);
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("[CelestialRescale] [KSCMover] Home Body is null");
            }
        }
    }
}