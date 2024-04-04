using com.lightstreamer.client;
using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlightDirector_WPF
{
    public class TelemetryItem : TelemetryItemBase
    {
        //Dictionary<string, string> translations;

        public static ITelemetryItem Create(string[] data) => new TelemetryItem(data);
        public static TelemetryItem Convert(ITelemetryItem item) {
            var data = new[] { item.Id, item.System, item.Description, item.Name, string.Empty, item.Units,string.Empty, "N" };
            return new TelemetryItem(data);
        }

        protected TelemetryItem(string[] data)
        {
            this.Id = data[0];
            this.System = data[1];
            this.Description = data[2];
            this.Name = data[3];
            this.Units = data[5].Replace("-", "").Replace("deg", "\u00B0");

            var kv = data[6].Split("|");
            if (kv.Length < 2 || !kv[0].Contains('='))
                this.StateNames = new Dictionary<string, string>();
            else
                this.StateNames = kv.Select(kv => kv.Trim().Split("=")).ToDictionary(k => k[0], v => v[1]);

            this.AlertOnChange = data[7].ToUpperInvariant() == "Y";
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ValueProperty)
            {
                var oldTransValue = this.TranslatedValue;
                this.TranslatedValue = Translate($"{e.NewValue}");
                if (this.AlertOnChange)
                {
                    Debug.WriteLine($"{DateTime.UtcNow}\t{Id}\t{e.OldValue}\t{TranslatedValue}");
                    if (e.OldValue != null)
                        VoiceAlert.Alert($"{this.Description} changed from {oldTransValue} to {this.TranslatedValue}");
                }
            }
            base.OnPropertyChanged(e);
        }

        private string Translate(string value)
            => StateNames.TryGetValue(value, out string rval) ? rval : (double.TryParse(value, out double dval) ? $"{dval:#,0.00} {Units}" : value);
    }
}

