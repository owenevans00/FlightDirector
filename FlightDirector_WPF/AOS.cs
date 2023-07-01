using System;
using System.Collections.Generic;
using System.Linq;
using FlightLib;

namespace FlightDirector_WPF
{
    static class AOS
    {
        public static string SignalState(IEnumerable<ITelemetryItem> a)
        {
            var rt = a.First().RawTelemetry;
            // TODO: Check time in case signal is ok but data is stale;
            var timeStamp =new DateTime( DateTime.UtcNow.Year,1,1,0,0,0,DateTimeKind.Utc) -TimeSpan.FromDays(1) + TimeSpan.FromMilliseconds(double.Parse( rt["Value"]));
            var delay= DateTime.UtcNow - timeStamp;
            return delay > TimeSpan.FromMinutes(1)
                ? "Static"
                : _aos.TryGetValue(rt["Status.Class"], out var rval) ? rval : "9";
        }

        static readonly Dictionary<string, string> _aos = new()
        {
            {"9","Stale" },
            {"24", "Signal Acquired" }
        };
    }
}
