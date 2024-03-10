using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
using System.Xaml;

namespace Map
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MapVM map;

        public MainWindow()
        {
            InitializeComponent();

            BindConverters();

            HookMapProperty();
        }

        [MemberNotNull(nameof(map))]
        private void HookMapProperty()
        {
            map = (MapVM)Resources["map"];
            var pd = TypeDescriptor.GetProperties(map).Find("X", false);
            var dpd = DependencyPropertyDescriptor.FromProperty(pd);
            dpd.AddValueChanged(map, MapHook);
        }

        private void MapHook(object? sender, EventArgs e)
        {
            var canvas = FindName("mapCanvas") as Canvas;
            var glnc = new GeoToLonConverter() { ScaleTo = canvas!.ActualWidth };
            var gltc = new GeoToLatConverter() { ScaleTo = canvas!.ActualHeight };
            var el = new Ellipse() { Width = 5, Height = 5, Fill = Brushes.Yellow };
            canvas!.Children.Add(el);
            Canvas.SetLeft(el, (double)glnc.Convert(map.Longitude, typeof(double), new(), CultureInfo.CurrentCulture));
            Canvas.SetTop(el, (double)gltc.Convert(map.Latitude, typeof(double), new(), CultureInfo.CurrentCulture));
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
    }
}
