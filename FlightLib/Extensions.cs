﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Threading.Tasks;

namespace FlightLib
{
    public static class Extensions
    {
        public static IEnumerable<string> ReadLines(this StreamReader stream)
        {
            while (!stream.EndOfStream)
                yield return stream.ReadLine();
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
                yield return (T)enumerator.Current;

            enumerator.Reset();
        }

        public static double AsDouble(this ITelemetryItem i)
            => double.TryParse(i.Value, out double d) ? d : 0;

        public static float AsFloat(this ITelemetryItem i)
            => float.TryParse(i.Value, out float d) ? d : 0;

        public static int AsInt(this ITelemetryItem i)
            => int.TryParse(i.Value, out int d) ? d : 0;

        public static T SecondOrDefault<T>(this IEnumerable<T> e)
            => e.Skip(1).FirstOrDefault();

        public static string SafeValue(this ITelemetryItem i)
        {
            if (i.Dispatcher is not null)
                try { return i.Dispatcher.Invoke(() => i.Value); }
                catch (TaskCanceledException) { return i.Value; }
                catch (NotImplementedException) { return i.Value; }
            else
                return i.Value;
        }

        public static void WriteLineIf(this TextWriter writer, bool condition, string text)
        {
            if (condition) writer.WriteLine(text);
        }
    }
}
