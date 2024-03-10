using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Celestial_Rescale
{
    internal class Utilis
    {
        public void Start()
        {
            if (CelestialRescale.isDoingAtmospheres && CelestialRescale.usingBrokenWay == false)
            {
                foreach (CelestialBody body in FlightGlobals.Bodies)
                {
                    if (body.atmosphere && body != null)
                    {
                        body.atmospherePressureCurveIsNormalized = true;
                    }
                }
            }
        }
    }
    public interface ParserBS1 : ParserBS2
    {
        void SetFromString(string s);
    }


    public interface ParserBS2
    {
        string ValueToString();
    }

    public interface ParserBS3<T>
    {
        T Value { get; set; }
    }

    //I fing hate parsers
    public class ParserBS<T> : ParserBS1, ParserBS2, ParserBS3<List<T>>
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

        public ParserBS()
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

        public ParserBS(List<T> i)
            : this()
        {
            Value = i;
        }

        public static implicit operator List<T>(ParserBS<T> parser)
        {
            return parser.Value;
        }

        public static implicit operator ParserBS<T>(List<T> value)
        {
            return new ParserBS<T>(value);
        }
    }
}