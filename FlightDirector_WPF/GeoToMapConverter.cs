using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FlightDirector_WPF
{
    internal class GeoToMapConverter : IValueConverter
    {
        const float imageWidth = 720;
        const float imageHeight = 360;
        const float offset = 5; // radius of dot

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => parameter.ToString() switch
        {
            "LON" => ScaleToImage(float.Parse(value as string), 360, imageWidth),
            "LAT" => ScaleToImage(-float.Parse(value as string), 180, imageHeight),
            _ => throw new ArgumentException("Bad parameter")
        } - offset;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        static float ScaleToImage(float value, float rangeMax, float scaleTo)
        {
            var i = (value + rangeMax /2) / rangeMax;
            Debug.Assert(i >= 0F && i <= 1F);
            return i * scaleTo;
        }
    }
}
