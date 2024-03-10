using FlightLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Map
{
    internal class TelemetryItem : DependencyObject, ITelemetryItem
    {
        public static ITelemetryItem Create(string[] data) => new TelemetryItem(data);

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #region Other ITelemetryItem Attributes
        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(TelemetryItem), new PropertyMetadata(""));

        // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(TelemetryItem), new PropertyMetadata(""));

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(TelemetryItem), new PropertyMetadata(""));

        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string System { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string TranslatedValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Units { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AlertOnChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Dictionary<string, string> StateNames { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> RawTelemetry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        TelemetryItem(string[] data) {
            Id = data[0];
            Name = data[3];
            Value = "0.0";
        }
    }
}
