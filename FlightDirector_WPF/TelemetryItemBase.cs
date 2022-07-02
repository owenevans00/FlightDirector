using FlightLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlightDirector_WPF
{
    public abstract class TelemetryItemBase : DependencyObject, ITelemetryItem
    {
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Id
        {
            get => (string)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public string System
        {
            get => (string)GetValue(SystemProperty);
            set => SetValue(SystemProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string TranslatedValue
        {
            get => (string)GetValue(TranslatedValueProperty);
            set => SetValue(TranslatedValueProperty, value);
        }

        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public string Units
        {
            get => (string)GetValue(UnitsProperty);
            set => SetValue(UnitsProperty, value);
        }

        public bool AlertOnChange
        {
            get { return (bool)GetValue(AlertOnChangeProperty); }
            set { SetValue(AlertOnChangeProperty, value); }
        }

        public Dictionary<string,string> RawTelemetry
        {
            get { return (Dictionary<string,string>)GetValue(RawTelemetryProperty); }
            set { SetValue(RawTelemetryProperty, value); }
        }

        public Dictionary<string,string> StateNames
        {
            get { return (Dictionary<string,string>)GetValue(StateNamesProperty); }
            set { SetValue(StateNamesProperty, value); }
        }

        public static readonly DependencyProperty StateNamesProperty =
            DependencyProperty.Register("StateNames", typeof(Dictionary<string,string>), typeof(TelemetryItemBase));

        public static readonly DependencyProperty AlertOnChangeProperty =
            DependencyProperty.Register("AlertOnChange", typeof(bool), typeof(TelemetryItemBase), new PropertyMetadata(false));

        public static readonly DependencyProperty RawTelemetryProperty =
            DependencyProperty.Register("RawTelemetry", typeof(Dictionary<string,string>), typeof(TelemetryItemBase));

        public static readonly DependencyProperty UnitsProperty =
            DependencyProperty.Register("Units", typeof(string), typeof(TelemetryItemBase));

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(TelemetryItemBase));

        public static readonly DependencyProperty TranslatedValueProperty =
            DependencyProperty.Register("TranslatedValue", typeof(string), typeof(TelemetryItemBase), new PropertyMetadata("0"));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(TelemetryItemBase));

        public static readonly DependencyProperty SystemProperty =
            DependencyProperty.Register("System", typeof(string), typeof(TelemetryItemBase));

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(TelemetryItemBase));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(TelemetryItemBase));
    }
}
