using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class AtmosphereFixer : MonoBehaviour
    {
        public double scaleFactor = 2;
        public float scaleFactor2 = 2;

        public void Start()
        {

            ConfigLoader();

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body != null && body.atmosphere)
                {
                    PrintCurve(body, "First Curves");

                    CurveModifierSetup(body, "Debuging");

                    PrintCurve(body, "Final Curves");
                }
            }
        }

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

        void Multiply(FloatCurve curve, double multiplier)
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

        List<double[]> ReadCurve(FloatCurve curve)
        {
            ConfigNode config = new ConfigNode();
            List<double[]> list = new List<double[]>();

            curve.Save(config);

            foreach (string k in config.GetValues("key"))
            {
                string[] valueStrings = k.Split(',');
                double[] values = new double[valueStrings.Length];

                for (int i = 0; i < valueStrings.Length; i++)
                {
                    if (double.TryParse(valueStrings[i], out double parsedValue))
                    {
                        values[i] = parsedValue;
                    }
                }

                list.Add(values);
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

        void CurveModifierSetup(CelestialBody body, string name)
        {
            Debug.Log("Doing Curve Stuff " + name + " for body " + body.name);
            FloatCurve newPressureCurve = MultiplyKeysAndCreateNewCurve(body.atmospherePressureCurve, "pressureCurve");
            FloatCurve newTempCurve = MultiplyKeysAndCreateNewCurve(body.atmosphereTemperatureCurve, "temperatureCurve");
            FloatCurve newSunCurve = MultiplyKeysAndCreateNewCurve(body.atmosphereTemperatureSunMultCurve, "temperatureSunMultCurve");

            body.atmospherePressureCurve = newPressureCurve;
            body.atmosphereTemperatureCurve = newTempCurve;
            body.atmosphereTemperatureSunMultCurve = newSunCurve;
            Debug.Log("curve1");
        }

        FloatCurve MultiplyKeysAndCreateNewCurve(FloatCurve curve, string name)
        {
            // Save the curve to a blank ConfigNode
            ConfigNode config = new ConfigNode();
            curve.Save(config);

            // Create a new FloatCurve to store the modified values
            FloatCurve newCurve = new FloatCurve();

            // Extract all the keys from the ConfigNode
            foreach (string key in config.GetValues("key"))
            {
                string[] keyParts = key.Split(',');
                if (float.TryParse(keyParts[0], out float keyValue))
                {
                    // Scale the keys
                    float multipliedValue = keyValue * scaleFactor2;

                    // Generate a new curve with the scaled keys
                    newCurve.Add(multipliedValue, float.Parse(keyParts[1]), float.Parse(keyParts[2]), float.Parse(keyParts[3]));
                }
            }

            // Log the modified keys
            Debug.Log("CelestialRescale.AtmosphereFixer " + name + " - Modified Keys:");
            foreach (string key in config.GetValues("key"))
            {
                Debug.Log("Key: " + key);
            }

            // Return the new curve
            return newCurve;
        }
    }
}
