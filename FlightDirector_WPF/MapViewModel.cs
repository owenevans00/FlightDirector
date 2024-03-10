using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
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

        #region Coordinates
        private readonly Dictionary<string, float> stateVec = new() {
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

        public string FormattedLatitude => $"{Abs(Latitude):N2}°{(Latitude < 0 ? "N": "S")}";

        public string FormattedLongitude => $"{Abs(Longitude):N2}°{(Longitude > 0 ?"E" : "W")}";


        // Using a DependencyProperty as the backing store for Altitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AltitudeProperty =
            DependencyProperty.Register("Altitude", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Longitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register("Longitude", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Latitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register("Latitude", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Z.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register("Z", typeof(float), typeof(MapViewModel), new PropertyMetadata(0F));

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(float), typeof(MapViewModel), new PropertyMetadata(0F));

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
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

        // Using a DependencyProperty as the backing store for MapUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapUriProperty =
            DependencyProperty.Register("MapUri", typeof(Uri), typeof(MapViewModel), new PropertyMetadata(null));
        #endregion

        public MapViewModel()
        {
            MapUri = new Uri(string.Format(mapUriFormat, DateTime.Today.Month));
            telemetry = new DataProvider(factory);
            telemetry.ValueUpdated += Telemetry_ValueUpdated;
            Debug.WriteLine($"DateAdj = {R2D(date_adj)}°");
        }

        private void Telemetry_ValueUpdated(object? sender, UpdateEventArgs e)
        {
            if (!stateVec.ContainsKey(e.Id)) return;
            stateVec[e.Id] = float.Parse(e.NewValue);

            Application.Current?.Dispatcher.BeginInvoke(Calculate);
        }

        private void Calculate()
        {
            X = stateVec["USLAB000032"];
            Y = stateVec["USLAB000033"];
            Z = stateVec["USLAB000034"];

            var fudge = 0.0 ;

            var alt = new Vector3(X, Y, Z).Length();
            Altitude = alt - 6385;

            var la_rad = Asin(Z / alt);
            Latitude = R2D(la_rad);

            double time_adj = fudge - (DateTime.UtcNow.TimeOfDay.TotalSeconds  -noon ) / day * 360;

            var lon_j2k = Acos(X / new Vector2(X, Y).Length());
            if (Y < 0) lon_j2k *= -1;
            if (lon_j2k < 0) lon_j2k += 2 * PI;
            lon_j2k %= 2 * PI;
            var lon_deg = R2D(lon_j2k);
            var lon_raw = (lon_deg - (date_adj * 1) + (time_adj * 1) + fudge);
            Longitude = lon_raw > 180 ? (lon_raw -180) - 180 : lon_raw;
        }

        static double R2D(double radians) => radians * 180 / PI;
    }
}
