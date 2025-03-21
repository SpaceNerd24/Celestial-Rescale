using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace CelestialRescale.Utilis
{
    public static class CR_Utilis
    {
        public static string version = "1.0.0";

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
}