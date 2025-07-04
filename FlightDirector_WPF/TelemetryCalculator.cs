﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlightLib;

namespace FlightDirector_WPF
{
    public class TelemetryCalculator : TelemetryItemBase
    {
        readonly List<ITelemetryItem> items;
        readonly Func<IEnumerable<ITelemetryItem>, string> calculator;

        public TelemetryCalculator(string Id, string System, string Description, string Units, IEnumerable<ITelemetryItem> Items, Func<IEnumerable<ITelemetryItem>, string> Calculator, bool AlertOnChange=false)
        {
            this.Id = Id;
            this.System = System;
            this.Description = Description;
            this.Units = Units;
            this.AlertOnChange = AlertOnChange;
            items = Items.ToList();
            calculator = Calculator;

            foreach (var i in items)
            {
                var descr = DependencyPropertyDescriptor.FromProperty(TranslatedValueProperty, typeof(TelemetryItemBase));
                descr.AddValueChanged(i as TelemetryItemBase, new EventHandler(ValueChanged));
            }
        }

        void ValueChanged(object sender, EventArgs e)
        {
            try
            {
                var newValue = calculator(items);
                if (this.Value == newValue) return;
                var oldvalue = this.Value;
                var oldTransValue = this.TranslatedValue;
                this.Value = newValue;
                this.TranslatedValue = $"{newValue} {Units}";
                if (AlertOnChange && oldvalue != null)
                    VoiceAlert.Alert($"{this.Description} changed from {oldTransValue} to {this.TranslatedValue}");
            }
            catch (NullReferenceException) { }
        }

    }
}
