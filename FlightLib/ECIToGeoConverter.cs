using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static System.Math;

namespace FlightLib
{
    internal class ECIToGeoConverter
    {
        readonly double date_adj = (double)(DateTime.UtcNow.DayOfYear - 79) / 365 * 360;
        const double day = 24 * 60 * 60; // seconds elapsed per day
        const double noon = 12 * 60 * 60; // seconds elapsed at noon

        const double eq_radius = 6378.1370; // equatorial and polar radii in km (same units as state vectors)
        const double pl_radius = 6356.7523; // for calculating exact altitude above WGS84 geoid
        const double eq_radius2 = eq_radius * eq_radius; // and their squares
        const double pl_radius2 = pl_radius * pl_radius;
        double prevLat, prevLon;

        public readonly List<string> telemetryIds = new() { "USLAB000032", "USLAB000033", "USLAB000034" };
        private readonly Dictionary<string, float> stateVec;
        private readonly Dictionary<string, bool> stateVecTracker;

        internal double Latitude, Longitude, Altitude, Heading;

        public string FormattedLatitude => $"{Abs(Latitude):N2}°{(Latitude < 0 ? "S" : "N")}";

        public string FormattedLongitude => $"{Abs(Longitude):N2}°{(Longitude > 0 ? "E" : "W")}";

        internal ECIToGeoConverter()
        {
            stateVec = telemetryIds.ToDictionary(i => i, _ => 0F);
            stateVecTracker = telemetryIds.ToDictionary(i => i, _ => false);
        }

        private void Calculate()
        {
            var X = stateVec[telemetryIds[0]];
            var Y = stateVec[telemetryIds[1]];
            var Z = stateVec[telemetryIds[2]];

            // distance to the center of the earth
            var alt = new Vector3(X, Y, Z).Length();
            var sin_lat = Z / alt;
            var cos_lat = Z / alt;
            // latitude
            var la_rad = Asin(sin_lat);
            if (double.IsNaN(la_rad)) la_rad = 0.0;
            Latitude = R2D(la_rad);
            // altitude is length of state vector minus radius of earth
            // at specified latitude
            var geo = Sqrt(
                (Pow((eq_radius2 * cos_lat), 2) + Pow((pl_radius2 * sin_lat), 2)) /
                (Pow((eq_radius * cos_lat), 2) + Pow((pl_radius * sin_lat), 2))
                );
            Altitude = alt - geo;

            var lon_j2k = Acos(X / new Vector2(X, Y).Length());
            if (double.IsNaN(lon_j2k)) lon_j2k = 0.0;

            // shenanigans to get the correct quadrant 
            if (Y < 0) lon_j2k *= -1;
            if (lon_j2k < 0) lon_j2k += 2 * PI;
            lon_j2k %= 2 * PI;

            // "longitude" relative to vernal equinox
            var lon_deg = R2D(lon_j2k);

            // adjust for actual position & rotation of earth
            double time_adj = (DateTime.UtcNow.TimeOfDay.TotalSeconds - noon) / day * 360;
            var lon_raw = lon_deg - date_adj - time_adj;
            Longitude = lon_raw > 180 ? (lon_raw - 180) - 180 : lon_raw;

            if (prevLat != 0 && prevLon != 0)
            {
                var deltaLat = (float)(Latitude - prevLat);
                var deltaLon = (float)(Longitude - prevLon);
                var hdg_raw = R2D(Acos(deltaLon / new Vector2(deltaLat, deltaLon).Length()));
                Heading = hdg_raw + (deltaLat > 0 ? 0 : 90);
            }
            prevLat = Latitude;
            prevLon = Longitude;
        }
        static double R2D(double radians) => radians * 180 / PI;

        internal bool TryUpdate(IEnumerable<float> values, string telemetryId)
        {
            // guard for dodgy data
            if (!stateVec.ContainsKey(telemetryId) || values.Count() != 3) return false;

            // update relevant component of state vector by choosing the value corresponding
            // to the updated telemetry id. This only needs to happen once per update cycle,
            // even though it'll get called several times so use the tracker to shortcut it.
            if (!stateVecTracker[telemetryId])
                stateVec[telemetryId] = values.ToArray()[telemetryIds.IndexOf(telemetryId)];

            // update and check if we have new data for all the state vector components
            stateVecTracker[telemetryId] = true;
            if (!stateVecTracker.Values.All(v => v)) return false;

            // clear tracking and update geographic data values
            foreach (var k in stateVecTracker.Keys)
                stateVecTracker[k] = false;
            Calculate();

            return true;

        }
    }
}
