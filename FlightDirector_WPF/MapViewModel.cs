using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Windows;
using System.Windows.Media.Media3D;
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

        const double aspectRatio = 2.0;
        const double offset = 5;

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

        //public string FormattedLatitude => $"{Abs(Latitude):N2}°{(Latitude < 0 ? "S" : "N")}";

        //public string FormattedLongitude => $"{Abs(Longitude):N2}°{(Longitude > 0 ? "E" : "W")}";

        public double Heading
        {
            get { return (double)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        public double MapLatitudePosition
        {
            get { return (double)GetValue(MapLatitudePositionProperty); }
            set { SetValue(MapLatitudePositionProperty, value); }
        }

        public double MapLongitudePosition
        {
            get { return (double)GetValue(MapLongitudePositionProperty); }
            set { SetValue(MapLongitudePositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MapLongitudePosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapLongitudePositionProperty =
            DependencyProperty.Register("MapLongitudePosition", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for MapLatitudePosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapLatitudePositionProperty =
            DependencyProperty.Register("MapLatitudePosition", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

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
        readonly string mapUriFormat = "pack://application:,,,/Maps/world.topo.bathy.2004{0:00}.3x720x360.png";

        public Uri MapUri
        {
            get { return (Uri)GetValue(MapUriProperty); }
            set { SetValue(MapUriProperty, value); }
        }

        public static readonly DependencyProperty MapUriProperty =
            DependencyProperty.Register("MapUri", typeof(Uri), typeof(MapViewModel), new PropertyMetadata(null));
        #endregion

        #region Map Image Size and Offset
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
                
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public double LabelOffset
        {
            get { return (double)GetValue(LabelOffsetProperty); }
            set { SetValue(LabelOffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelOffsetProperty =
            DependencyProperty.Register("LabelOffset", typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(nameof(Height), typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register(nameof(Width), typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Top.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register(nameof(Top), typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));
        
        // Using a DependencyProperty as the backing store for Left.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register(nameof(Left), typeof(double), typeof(MapViewModel), new PropertyMetadata(0.0));
        #endregion


        public MapViewModel()
        {
            MapUri = new Uri(string.Format(mapUriFormat, DateTime.Today.Month));
            telemetry = new DataProvider(factory, converter: TelemetryItem.Convert);
            telemetry.ValueUpdated += Telemetry_ValueUpdated;
            Heading = 90;
            //Debug.WriteLine($"DateAdj = {R2D(date_adj)}°");
        }

        internal void ContainerSizeChanged(Size newSize)
        {
            // given a rectangle of size (x,y) figure out what
            // rectangle of aspect ratio 'a' fits inside it

            double containerAspectRatio = newSize.Width / newSize.Height;
            if (containerAspectRatio > aspectRatio) // container is wider than content
            {
                Height = newSize.Height;
                Width = Height * aspectRatio;
                Top = 0.0;
                Left = (newSize.Width - Width) / 2;

            }
            else
            {
                Width = newSize.Width;
                Height = Width / aspectRatio;
                Left = 0;
                Top = (newSize.Height - Height) / 2;
            }

            LabelOffset = -Top;
            MapLatitudePosition = offset + Height / 180 * Latitude * -1 + Height / 2;
            MapLongitudePosition = offset + Width / 360 * Longitude + Width / 2;
        }

        private void Telemetry_ValueUpdated(object sender, UpdateEventArgs e)
        {
            // Custom telemetry values always seem to come up as 0, so unless/until we figure
            // out why, we'll use a copy of the code from FlightLib.ECIToGeoConverter here
            //switch (e.Id)
            //{
            //    case "USLAB000HDG":
            //        Dispatcher.BeginInvoke(() => Heading = float.Parse(e.NewValue));
            //        break;
            //    case "USLAB00ULAT":
            //        var foo = (sender as FlightLib.DataProvider)["USLAB00ULAT"];
            //        Dispatcher.BeginInvoke(() => Latitude = float.Parse(e.NewValue));
            //        break;
            //    case "USLAB00ULON":
            //        Dispatcher.BeginInvoke(() => Longitude = float.Parse(e.NewValue));
            //        break;
            //    case "USLAB000032":
            //        break;
            //}
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
            var lon_raw = (lon_deg - (date_adj * 1) + (time_adj * -1));
            Longitude = lon_raw > 180 ? lon_raw - 360 : lon_raw;

            if (prevLat != 0 && prevLon != 0)
            {
                var deltaLat = (float)(Latitude - prevLat);
                var deltaLon = (float)(Longitude - prevLon);
                //var hdg_raw = R2D(Acos(deltaLon / new Vector2(deltaLat, deltaLon).Length()));
                var hdg_raw = R2D(Atan(deltaLon / deltaLat));
                Heading = hdg_raw < 0 ? hdg_raw + 180 : hdg_raw;// + (deltaLat > 0 ? 0 : 90);
            }
            prevLat = Latitude;
            prevLon = Longitude;

            MapLatitudePosition = offset + Height / 180 * Latitude * -1 + Height / 2;
            MapLongitudePosition = offset + Width / 360 * Longitude + Width / 2;
        }

        static double R2D(double radians) => radians * 180 / PI;
    }
}
