using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using ToolbarControl_NS;
using UnityEngine;

namespace Celestial_Rescale
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class RegisterToolbar : MonoBehaviour
    {
        public void Start()
        {
            ToolbarControl.RegisterMod(Utilis.MODID, Utilis.MODNAME);
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

        public void CreateButtonIcon()
        {
            ToolbarControl toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(ToggleOn, ToggleOff,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
                Utilis.MODID,
                "CelestialRescaleButton",
                "CelestialRescale/Images/PluginData/CelestialRescaleLogo.png",
                "CelestialRescale/Images/PluginData/CelestialRescaleLogo.png",
                Utilis.MODNAME
            );

            toolbarControl.buttonActive = true;
        }

        public void ToggleOn()
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
                                    Debug.LogError("[CelestialRescale]" + " In game rescaling is coming in v0.6.0 you need to wait");
                                }, 140.0f, 30.0f, false),
                            new DialogGUIButton("Log Planet's Atmo Curves",
                                delegate
                                {
                                    foreach (CelestialBody body in FlightGlobals.Bodies)
                                    {
                                        PrintCurve(body, "User Print");
                                    }
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

        public void ToggleOff()
        {
            Debug.Log("something will go here");
        }
    }
}
