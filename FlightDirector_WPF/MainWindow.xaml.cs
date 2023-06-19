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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightDirector_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TelemetryServer ts;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            Title += " - Debug";
#endif
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ts = new TelemetryServer();
            //ts.Start();
        }

        private void ListBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void Iss_cam_reload(object sender, RoutedEventArgs e)
            => iss_cam.CoreWebView2.Navigate($"{iss_cam.Source}");

        private void Iss_cam_popout(object sender, RoutedEventArgs e)
            => Process.Start(new ProcessStartInfo() { FileName = iss_cam.Source.AbsoluteUri, UseShellExecute = true });

        private void EHDC_reload(object sender, RoutedEventArgs e)
            => ehd_cam.CoreWebView2.Navigate($"{ehd_cam.Source}");

        private void EHDC_popout(object sender, RoutedEventArgs e)
            => Process.Start(new ProcessStartInfo() { FileName = ehd_cam.Source.AbsoluteUri, UseShellExecute = true });

    }
}