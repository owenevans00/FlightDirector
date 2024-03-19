using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Map
{
    abstract class GeoToMapConverter : DependencyObject, IValueConverter
    {
        const float offset = 5; // radius of dot

        public double ScaleTo
        {
            get { return (double)GetValue(ScaleToProperty); }
            set { SetValue(ScaleToProperty, value); }
        }

        public static readonly DependencyProperty ScaleToProperty =
            DependencyProperty.Register("ScaleTo", typeof(double), typeof(GeoToMapConverter), new PropertyMetadata(1.0));

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        protected double ScaleToImage(double value, double rangeMax)
        {
            var i = ((value + rangeMax / 2) % rangeMax) / rangeMax;
            Debug.Assert(i >= 0F && i <= 1F);
            return i * ScaleTo - offset;
        }
    }

    class GeoToLatConverter : GeoToMapConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ScaleToImage(-System.Convert.ToDouble(value), 180);

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    class GeoToLonConverter : GeoToMapConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ScaleToImage(System.Convert.ToDouble(value), 360);

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
