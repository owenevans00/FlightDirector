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
using System.Windows.Shapes;

namespace FlightDirector_WPF
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public FlightLib.Command OKCommand => new((_) => { DialogResult = true; Close(); }, (_) => true);
        public FlightLib.Command CloseCommand => new((_) => { DialogResult = false; Close(); }, (_) => true);

        public string Caption { get; private set; }

        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(string), typeof(InputBox));


        public InputBox(string caption = "Enter data", string title = "Input Data", string initialValue = "Some Text")
        {
            Title = title;
            Caption = caption;
            Data = initialValue;

            InitializeComponent();
        }
    }
}
