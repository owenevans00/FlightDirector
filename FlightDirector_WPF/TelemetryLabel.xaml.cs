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

namespace FlightDirector_WPF
{
    /// <summary>
    /// Interaction logic for TelemetryLabel.xaml
    /// </summary>
    public partial class TelemetryLabel : UserControl
    {
        public string TelemetryId
        {
            get { return (string)GetValue(TelemetryIdProperty); }
            set { SetValue(TelemetryIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TelemetryId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TelemetryIdProperty =
            DependencyProperty.Register("TelemetryId", typeof(string), typeof(TelemetryLabel));

        public TelemetryLabel()
        {
            InitializeComponent();
        }
    }
}
