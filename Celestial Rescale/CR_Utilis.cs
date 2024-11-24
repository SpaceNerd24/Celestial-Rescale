using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using CelestialRescale.API;

namespace CelestialRescale.Utilis
{
    public static class CR_Utilis
    {
        // not currently needed
        public static string MODID = "Celestial_Rescale";
        public static string MODNAME = "Celestial Rescale";

        internal static Dictionary<string, FloatCurve> bodyPresureCurvesDictionary =
            new Dictionary<string, FloatCurve>();
        internal static Dictionary<string, FloatCurve> bodyTempCurvesDictionary =
            new Dictionary<string, FloatCurve>();

        internal static PQSCity kscCity = null;
        internal static double originalLatitude;
        internal static double originalLongitude;
        internal static Vector3 KSCOrignalPOS;

        internal static void LoadDictionaries()
        {
            Debug.Log("Testing the new Dictionary system");
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                bodyPresureCurvesDictionary.Add(body.name, body.atmospherePressureCurve);
                bodyTempCurvesDictionary.Add(body.name, body.atmosphereTemperatureCurve);
            }
        }

        internal static void LoadKSCOriganlPOS()
        {
            foreach (PQSCity city in Resources.FindObjectsOfTypeAll<PQSCity>())
            {
                if (city.name == "KSC")
                {
                    kscCity = city;
                    Debug.Log("KSC found");
                    KSCOrignalPOS = kscCity.repositionRadial;
                    originalLatitude = kscCity.lat;
                    originalLongitude = kscCity.lon;
                    break;
                }
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