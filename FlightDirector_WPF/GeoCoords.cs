using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlightDirector_WPF
{
    internal static class GeoCoords
    {
        internal static float Latitude(IEnumerable<ITelemetryItem> coords) {
            Debug.Assert(coords.Count() == 3);
            var a = coords.ToArray();
            var x = a[0].AsFloat();
            var y = a[1].AsFloat();
            var z = a[2].AsFloat();

            var alt = new Vector3(x,y,z).Length();
            var la_rad = Math.Asin(z / alt);
            return (float)(la_rad * 180 / Math.PI);
        }

        internal static string FormattedLatitude(IEnumerable<ITelemetryItem> coords)
        {
            var lat = Latitude(coords);
            return $"{Math.Abs(lat):N2}°{((lat > 0) ? "N" : "S")}";
        }

        internal static float Longitude(IEnumerable<ITelemetryItem> coords)
        {
            Debug.Assert(coords.Count() == 3);
            var a = coords.ToArray();
            var x = a[0].AsFloat();
            var y = a[1].AsFloat();
            var z = a[2].AsFloat();

            double date_adj = (double)DateTime.UtcNow.DayOfYear / 365 / (2 * Math.PI);
            var noon = 12 * 60 * 60;
            double time_adj = (DateTime.UtcNow.TimeOfDay.TotalSeconds - noon) * (2 * Math.PI / (24 * 60 * 60));

            var lon_j2k = Math.Acos(y / new Vector2(x, y).Length());

            var lon_rad = (x >= 0) ? 0 + date_adj - time_adj - lon_j2k :
                      (y >= 0) ? 0 + date_adj - time_adj + lon_j2k :
                      (z >= 0) ? Math.PI - date_adj + time_adj - lon_j2k :
                                 0 + date_adj - time_adj + lon_j2k;

            if (lon_rad < -Math.PI) lon_rad += 2 * Math.PI;
            if (lon_rad > Math.PI) lon_rad -= 2 * Math.PI;

            return (float)(lon_rad * 180 / Math.PI);
        }

        internal static string FormattedLongitude(IEnumerable<ITelemetryItem> coords)
        {
            var lon = Longitude(coords);
            return $"{Math.Abs(lon):N2}°{((lon > 0) ? "E" : "W")}";
        }
    }
}
