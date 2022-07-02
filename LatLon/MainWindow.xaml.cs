using FlightLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using static System.Math;
using System.Timers;
using System.Diagnostics;

namespace LatLon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DataProvider data;
        float x, y, z, la, lo;
        double total_lon, last_lon;
        string lo_txt = string.Empty;
        string lerp_txt = string.Empty;
        DateTime startTime = DateTime.Now;
        DateTime jd_epoch = new DateTime(1, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        readonly Timer t;

        public MainWindow()
        {
            InitializeComponent();
            data = new DataProvider(d => new TI(d), new[] { ".ANGLES" });
            data.ValueUpdated += Data_ValueUpdated;
        }

        private void Data_ValueUpdated(object? sender, UpdateEventArgs e)
        {
            switch (e.Id)
            {
                case "USLAB000032":
                    x = float.Parse(e.NewValue);
                    Dispatch(j2kx, $"{x:0.00}");
                    Calculate(update: true);
                    break;
                case "USLAB000033":
                    y = float.Parse(e.NewValue);
                    Dispatch(j2ky, $"{y:0.00}");
                    Calculate();
                    break;
                case "USLAB000034":
                    z = float.Parse(e.NewValue);
                    Dispatch(j2kz, $"{z:0.00}");
                    Calculate();
                    break;
                default:
                    break;
            }
        }

        private void Calculate(bool update = false)
        {
            var now = DateTime.UtcNow;
            Dispatch(updated, $"{now:HH:mm:ss}");

            // latitude

            var alt = new Vector3(x, y, z).Length();
            var la_rad = Asin(z / alt);
            la = (float)(la_rad * 180 / PI);
            var la_deg = la > 0 ? "°N" : "°S";

            Dispatch(lat, $"{Abs(la):0.00}{la_deg}");

            // longitude

            // From https://physics.stackexchange.com/questions/130580/how-to-determine-satellite-position-in-j2000-from-latitude-longitude-and-distan
            // x=(N+h)cosϕcosλ where h is altitude. ϕ is latitude and λ is longitude
            // For our purposes N = 0

            var cos_lon = x / alt * Cos(la_rad);

            var lon_raw = Acos(cos_lon);

            // var lon_raw = Atan(x/y);
            var date_adj = (now.DayOfYear - 79) * 360 / 365;
            var time_adj = now.TimeOfDay.TotalMinutes * 360 / (6 * 60);

            // 'approximately'
            var jd_now = (DateTime.UtcNow - jd_epoch).TotalDays + 4713*365.25;
            var j2k_now = jd_now - 2451545.0;
            var jd_time = jd_now - (int)jd_now;
            var era = (0.00273781191135448 * j2k_now + 0.7790572732640 + jd_time) % (PI * 2);
            var era_txt = $"{era * 180 / PI:0.00}";
            //lo = (float)((lon_raw - date_adj - time_adj) - 180) % 360;
            lo = (float)((lon_raw + era)*180/PI - 180) % 360;
            //lo_txt = lo <0 ? $"{lo+180:0.00}° E" : $"{lo:0.00}° W";
            //Dispatch(lon, lo_txt);
            Dispatch(lon, era_txt);

            if (update)
            {
                if (last_lon != 0)
                {
                    total_lon += Abs(lo - Abs(last_lon));
                    var spd = total_lon / (DateTime.Now - startTime).TotalSeconds * 60;
                    var spd_txt = $"{spd:0.00}°";
                    var tot_txt = $"{total_lon:0.00}° total";
                    Dispatch(degmin, spd_txt);
                    Dispatch(totdeg, tot_txt);
                }
                last_lon = lo;
            }
        }

        private void Dispatch(TextBlock b, string s)
        {
            this.Dispatcher.Invoke(() => b.Text = s);
        }
    }

    class TI : ITelemetryItem
    {
        internal TI(string[] data)
        {
            Id = data[0];
            System = data[1];
        }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Id { get; set; }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string System { get; set; }
        public string TranslatedValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Units { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AlertOnChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> StateNames { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> RawTelemetry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
