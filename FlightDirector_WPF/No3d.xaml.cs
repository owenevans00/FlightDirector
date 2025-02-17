using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace FlightDirector_WPF
{
    /// <summary>
    /// Interaction logic for No3d.xaml
    /// </summary>
    public partial class No3d : Window
    {
        TelemetryServer ts;
       
        public No3d()
        {
            InitializeComponent();
#if DEBUG
            Title += " - Debug";
#endif
            BindConverters();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ts = new TelemetryServer();
            ts.Start();
        }

        private void Iss_cam_reload(object sender, RoutedEventArgs e)
            => iss_cam.CoreWebView2.Navigate($"{iss_cam.Source}");

        private void Iss_cam_popout(object sender, RoutedEventArgs e)
            => Process.Start(new ProcessStartInfo() { FileName = iss_cam.Source.AbsoluteUri, UseShellExecute = true });

        private void EHDC_reload(object sender, RoutedEventArgs e)
            => ehd_cam.CoreWebView2.Navigate($"{ehd_cam.Source}");

        private void EHDC_popout(object sender, RoutedEventArgs e)
            => Process.Start(new ProcessStartInfo() { FileName = ehd_cam.Source.AbsoluteUri, UseShellExecute = true });

        private void Map_popout(object sender, RoutedEventArgs e)
        {
            var fvm = this.Resources["dd"] as FlightViewModel;
            var maps_uri = $"https://www.google.com/maps/place/{fvm["USLAB000LAT"].TranslatedValue}+{fvm["USLAB000LON"].TranslatedValue}";
            Process.Start(new ProcessStartInfo() { FileName = maps_uri, UseShellExecute = true });
        }

        private void BindConverters()
        {
            var glnc = this.Resources["glnc"] as GeoToLonConverter;
            var gltc = this.Resources["gltc"] as GeoToLatConverter;
            var canvas = FindName("mapCanvas") as Canvas;

            Binding b1 = new("ActualHeight") { Source = canvas };
            BindingOperations.SetBinding(gltc, GeoToMapConverter.ScaleToProperty, b1);

            Binding b2 = new("ActualWidth") { Source = canvas };
            BindingOperations.SetBinding(glnc, GeoToMapConverter.ScaleToProperty, b2);
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Border;
            var hwndctrl = new HwndControl(control.ActualHeight, control.ActualWidth);
            control.Child = hwndctrl;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                var fvm = this.Resources["dd"] as FlightViewModel;
                fvm?.unity?.Kill();
            }
            finally
            {
                ts.Stop();
            }
        }
    }
}