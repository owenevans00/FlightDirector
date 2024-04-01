using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Windows;
using static System.Math;

namespace FlightDirector_WPF
{
    internal class MapViewModel : DependencyObject
    {
        private readonly DataProvider telemetry;
        private readonly Func<string[], int, ITelemetryItem> factory = (a, i) => TelemetryItem.Create(a);

        readonly double date_adj = (double)(DateTime.UtcNow.DayOfYear - 79) / 365 * 360;
        readonly double day = 24 * 60 * 60; // seconds elapsed per day
        readonly double noon = 12 * 60 * 60; // seconds elapsed at noon
        double prevLat, prevLon;

        #region Coordinates
        private readonly Dictionary<string, float> stateVec = new() {
            {"USLAB000032",0F },
            {"USLAB000033",0F },
            {"USLAB000034",0F },
        };

        private readonly Dictionary<string, float> stateVecTracker = new() {
            {"USLAB000032",0F },
            {"USLAB000033",0F },
            {"USLAB000034",0F },
        };

        public float X
        {
            get { return (float)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public float Y
        {
            get { return (float)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public float Z
        {
            get { return (float)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }

        public double Altitude
        {
            get { return (double)GetValue(AltitudeProperty); }
            set { SetValue(AltitudeProperty, value); }
        }

        public double Latitude
        {
            get { return (double)GetValue(LatitudeProperty); }
            set { SetValue(LatitudeProperty, value); }
        }

        public double Longitude
        {
            get { return (double)GetValue(LongitudeProperty); }
            set { SetValue(LongitudeProperty, value); }
        }

        public string FormattedLatitude => $"{Abs(Latitude):N2}°{(Latitude < 0 ? "S" : "N")}";

        public string FormattedLongitude => $"{Abs(Longitude):N2}°{(Longitude > 0 ? "E" : "W")}";

        public double Heading
        {
            get { return (double)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register("Heading", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty AltitudeProperty =
            DependencyProperty.Register("Altitude", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register("Longitude", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register("Latitude", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register("Z", typeof(float), typeof(MapViewModel), new PropertyMetadata(0F));

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(float), typeof(MapViewModel), new PropertyMetadata(0F));

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(float), typeof(MapViewModel), new PropertyMetadata(0F));
        #endregion

        #region Satellite Photomosaic Background
        readonly string mapUriFormat = "pack://application:,,,/Maps/world.topo.bathy.2004{0:00}.3x5400x2700.png";

        public Uri MapUri
        {
            get { return (Uri)GetValue(MapUriProperty); }
            set { SetValue(MapUriProperty, value); }
        }

        public static readonly DependencyProperty MapUriProperty =
            DependencyProperty.Register("MapUri", typeof(Uri), typeof(MapViewModel), new PropertyMetadata(null));
        #endregion

        public MapViewModel()
        {
            MapUri = new Uri(string.Format(mapUriFormat, DateTime.Today.Month));
            telemetry = new DataProvider(factory);
            telemetry.ValueUpdated += Telemetry_ValueUpdated;
            Heading = 90;
            Debug.WriteLine($"DateAdj = {R2D(date_adj)}°");
        }

        private void Telemetry_ValueUpdated(object sender, UpdateEventArgs e)
        {
            if (!stateVec.ContainsKey(e.Id)) return;
            stateVec[e.Id] = float.Parse(e.NewValue);
            stateVecTracker[e.Id] = 1;
            if (stateVecTracker.Values.Sum() == 3)
            {
                foreach (var k in stateVecTracker.Keys)
                    stateVecTracker[k] = 0;
                Application.Current?.Dispatcher.BeginInvoke(Calculate);
            }
        }

        private void Calculate()
        {
            X = stateVec["USLAB000032"];
            Y = stateVec["USLAB000033"];
            Z = stateVec["USLAB000034"];

            var fudge = 0.0;

            var alt = new Vector3(X, Y, Z).Length();
            Altitude = alt - 6385;

            var la_rad = Asin(Z / alt);
            Latitude = R2D(la_rad);

            double time_adj = (DateTime.UtcNow.TimeOfDay.TotalSeconds - noon) / day * 360;

            var lon_j2k = Acos(X / new Vector2(X, Y).Length());
            if (Y < 0) lon_j2k *= -1;
            if (lon_j2k < 0) lon_j2k += 2 * PI;
            lon_j2k %= 2 * PI;
            var lon_deg = R2D(lon_j2k);
            var lon_raw = (lon_deg - (date_adj * 1) + (time_adj * -1) + fudge);
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
    }
}
