using UnityEngine;
using ToolbarControl_NS;
using System;
using System.Linq;
using KSP.UI.Screens;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using Celestial_Rescale.API;

namespace Celestial_Rescale.Utilis
{
    public static class CR_Utilis
    {
        public static string MODID = "Celestial_Rescale";
        public static string MODNAME = "Celestial Rescale";

        internal static void ResetBody(CelestialBody body)
        {
            FloatCurve presureCurve = CR_API.GetAtmoCurve(body, false);
            FloatCurve tempCurve = CR_API.GetAtmoCurve(body, true);
            double scaleFactor = CR_API.GetScaleFactor();
            float scaleFactor2 = CR_API.GetScaleFactor2();

            if (body.pqsController != null)
            {
                body.Radius /= scaleFactor;
                body.pqsController.radius /= scaleFactor;
                body.pqsController.radiusDelta /= scaleFactor;
                body.pqsController.radiusSquared /= scaleFactor;
                body.pqsController.radiusMax /= scaleFactor;
                body.pqsController.radiusMin /= scaleFactor;
                body.Mass /= scaleFactor;
                body.sphereOfInfluence /= scaleFactor;
            }

            if (body.pqsController != null)
            {
                body.pqsController.mapMaxHeight /= scaleFactor;
            }

            body.pqsController.RebuildSphere();

            if (body.atmosphere)
            {
                ResetAtmo(body, scaleFactor);
            }
            ResetScaledSpace(body, scaleFactor2);
            ResetOcean(body);
            ResetOrbit(body, scaleFactor);
        }

        internal static void ResetAtmo(CelestialBody body, double scaleFactor)
        {
            body.afg.outerRadius = Convert.ToSingle(body.Radius);
            body.afg.UpdateAtmosphere(body);
            body.atmosphereDepth /= scaleFactor;
        }

        internal static void ResetScaledSpace(CelestialBody body, float scaleFactor2)
        {
            if (body != null && body.scaledBody != null) // Additional null check
            {
                ScaledSpaceFader[] faders = Resources.FindObjectsOfTypeAll<ScaledSpaceFader>();

                foreach (ScaledSpaceFader fader in faders)
                {
                    if (fader != null && fader.celestialBody == body) // Additional null check
                    {
                        // Modify the properties of the fader as needed
                        fader.fadeStart /= scaleFactor2;
                        fader.fadeEnd /= scaleFactor2;
                        break;
                    }
                }

                body.scaledBody.transform.localScale /= scaleFactor2;
            }
        }

        internal static void ResetOcean(CelestialBody body)
        {
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

        internal static void ResetOrbit(CelestialBody body, double scaleFactor)
        {
            if (body != null && body.orbit != null && body.orbitDriver != null)
            {
                body.orbit.semiMajorAxis /= scaleFactor;
                body.orbit.UpdateFromUT(Planetarium.GetUniversalTime());
            }
        }
    }

    public interface Parser1 : Parser2
    {
        void SetFromString(string s);
    }

    public interface Parser2
    {
        string ValueToString();
    }

    public interface Parser3<T>
    {
        T Value { get; set; }
    }

    public class MainParser<T> : Parser1, Parser2, Parser3<List<T>>
    {
        private static readonly char[] splitChars = new char[4] { ' ', ',', ';', '\t' };

        private static Dictionary<Type, Func<string, T>> parsers = new Dictionary<Type, Func<string, T>>();

        private readonly Func<string, T> _parserDelegate;

        public List<T> Value { get; set; }

        public void SetFromString(string s)
        {
            Value = new List<T>();
            string[] array = s.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            foreach (string arg in array)
            {
                Value.Add(_parserDelegate(arg));
            }
        }

        public string ValueToString()
        {
            if (Value != null)
            {
                return string.Join(" ", Value.Select((T v) => v.ToString()).ToArray());
            }

            return null;
        }

        public MainParser()
        {
            Value = new List<T>();
            Type typeFromHandle = typeof(T);
            if (!parsers.TryGetValue(typeFromHandle, out _parserDelegate))
            {
                MethodInfo method = typeFromHandle.GetMethod("Parse", new Type[1] { typeof(string) });
                _parserDelegate = (Func<string, T>)Delegate.CreateDelegate(typeof(Func<string, T>), method);
                parsers.Add(typeFromHandle, _parserDelegate);
            }
        }

        public MainParser(List<T> i)
            : this()
        {
            Value = i;
        }

        public static implicit operator List<T>(MainParser<T> parser)
        {
            return parser.Value;
        }

        public static implicit operator MainParser<T>(List<T> value)
        {
            return new MainParser<T>(value);
        }
    }

    /*
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Test : MonoBehaviour
    {
        public void Start()
        {
            double newScaleFactor = 10;
            CR_API.ChangeScaleFactor(newScaleFactor);
            Debug.Log("New " + newScaleFactor + " Accual: " + CR_API.GetScaleFactor());

            float newScaleFactor2 = 10;
            CR_API.ChangeScaleFactor2(newScaleFactor2);
            Debug.Log("New "+ newScaleFactor2 + " Accual: " + CR_API.GetScaleFactor2());
        }
    }
    */


}