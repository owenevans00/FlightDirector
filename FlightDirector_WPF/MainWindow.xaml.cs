using FlightLib;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FlightDirector_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Process unity;
        TelemetryServer ts;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            Title += " - Debug";
#endif
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

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Border;
            var hwndctrl = new HwndControl(control.ActualHeight, control.ActualWidth);
            control.Child = hwndctrl;
//#if !DEBUG
            unity = Process.Start(@"C:\Users\owene\source\ISS_3D\Build\ISS_3D.exe", $"-parentHWND {hwndctrl.Handle}");
//#endif
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try { unity?.Kill(); }
            finally {
                ts.Stop();
            }
        }
    }
}