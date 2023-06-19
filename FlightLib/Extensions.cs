using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
    }
}
    