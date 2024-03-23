using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui;

namespace Test_MauiApp1.Converters
{
    public class PermissionToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          

            if (value == null) return Colors.Grey;

            if (System.Convert.ToInt32(value) == 1) return Colors.Green; 
            if (System.Convert.ToInt32(value) == 2) return Colors.Blue;
            if (System.Convert.ToInt32(value) == 3) return Colors.Grey;

            return "Gray";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //For this case you can ignore this
            return null;
        }
    }
}
