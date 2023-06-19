using FlightLib;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Windows;
using static System.Math;

namespace LatLon
{
    internal class Calculator : DependencyObject
    {


        public float X
        {
            get { return (float)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(float), typeof(Calculator), new PropertyMetadata(0F));



        public float Y
        {
            get { return (float)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(float), typeof(Calculator), new PropertyMetadata(0F));



        public float Z
        {
            get { return (float)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Z.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register("Z", typeof(float), typeof(Calculator), new PropertyMetadata(0F));



        public DateTime UtcNow
        {
            get { return (DateTime)GetValue(UtcNowProperty); }
            set { SetValue(UtcNowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UtcNow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UtcNowProperty =
            DependencyProperty.Register("UtcNow", typeof(DateTime), typeof(Calculator));


        public DateTime LocalNow
        {
            get { return (DateTime)GetValue(LocalNowProperty); }
            set { SetValue(LocalNowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LocalNow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocalNowProperty =
            DependencyProperty.Register("LocalNow", typeof(DateTime), typeof(Calculator));



        public string Latitude
        {
            get { return (string)GetValue(LatitudeProperty); }
            set { SetValue(LatitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Latitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register("Latitude", typeof(string), typeof(Calculator));

        public string Longitude
        {
            get { return (string)GetValue(LongitudeProperty); }
            set { SetValue(LongitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Longitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register("Longitude", typeof(string), typeof(Calculator));




        public int Locator_x
        {
            get { return (int)GetValue(Locator_xProperty); }
            set { SetValue(Locator_xProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Locator_x.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Locator_xProperty =
            DependencyProperty.Register("Locator_x", typeof(int), typeof(Calculator), new PropertyMetadata(0));



        public int Locator_y
        {
            get { return (int)GetValue(Locator_yProperty); }
            set { SetValue(Locator_yProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Locator_y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Locator_yProperty =
            DependencyProperty.Register("Locator_y", typeof(int), typeof(Calculator), new PropertyMetadata(0));




        public double RawLongitude
        {
            get { return (double)GetValue(RawLongitudeProperty); }
            set { SetValue(RawLongitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RawLongitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RawLongitudeProperty =
            DependencyProperty.Register("RawLongitude", typeof(double), typeof(Calculator), new PropertyMetadata(0.00));



        private readonly DataProvider data;

        public Calculator()
        {
            data = new DataProvider(d => new TI(d), new[] { ".ANGLES" });
            data.ValueUpdated += Data_ValueUpdated;
        }

        private void Data_ValueUpdated(object? sender, UpdateEventArgs e)
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
            {
                switch (e.Id)
                {
                    case "USLAB000032":
                        X = float.Parse(e.NewValue);
                        Calculate();
                        break;
                    case "USLAB000033":
                        Y = float.Parse(e.NewValue);
                        Calculate();
                        break;
                    case "USLAB000034":
                        Z = float.Parse(e.NewValue);
                        Calculate();
                        break;
                    default:
                        break;
                }
            }
            else
                try
                {
                    Dispatcher.Invoke(() => Data_ValueUpdated(sender, e));
                }
                catch { }
        }

        private void Calculate()
        {
            UtcNow = DateTime.UtcNow;
            LocalNow = DateTime.Now;

            // latitude

            var alt = new Vector3(X, Y, Z).Length();
            var la_rad = Asin(Z / alt);
            var la = (float)(la_rad * 180 / PI);
            var la_deg = la > 0 ? "°N" : "°S";

            Latitude = $"{Abs(la):0.00}{la_deg}";
            Locator_y = (int)Floor(86F - la) * 2;

            // longitude
            double lon_rad = 0.00;

            var lon_j2k = Acos(Y / new Vector2(X, Y).Length());
            //var offset = 0;

            double date_adj = (double)UtcNow.DayOfYear / 365 / (2 * PI);
            var noon = 12 * 60 * 60;
            double time_adj = (UtcNow.TimeOfDay.TotalSeconds - noon) * (2 * PI / (24 * 60 * 60));

            lon_rad = (X >= 0) ? 0 + date_adj - time_adj - lon_j2k :
                      (Y >= 0) ? 0 + date_adj - time_adj + lon_j2k :
                      (Z >= 0) ? PI - date_adj + time_adj - lon_j2k :
                                 0 + date_adj - time_adj + lon_j2k;

            RawLongitude = R2D(lon_rad % (2 * PI));

            Locator_x = (int)Floor((RawLongitude + 180) % 360) * 2;

            string d;
            double lo;
            if (RawLongitude > 180)
            {
                lo = 180 - RawLongitude % 180;
                d = "W";
            }
            else if (RawLongitude < 0)
            {
                lo = Abs(RawLongitude);
                d = "W";
            }
            else
            {
                lo = RawLongitude;
                d = "E";
            }

            Longitude = $"{lo:0.00}°{d}";

            Debug.WriteLine($"{R2D(date_adj)}\t{R2D(time_adj)}\t{R2D(lon_j2k)}");

        }

        private static double R2D(double r)
            => r * 180 / PI;
    }
}
