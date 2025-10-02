using System;
using System.Globalization;
using System.Windows.Forms;

namespace si.birokrat.common.config_packer
{
    public class DateTimePickerHandler : IControlHandler
    {
        public void AssignValueToControl(Control ctrl, string value)
        {
            ((DateTimePicker)ctrl).Value = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public bool Condition(Control ctrl)
        {
            return ctrl is DateTimePicker;
        }

        public string Stringify(Control ctrl)
        {
            return ((DateTimePicker)ctrl).Value.ToString("yyyy-MM-dd");
        }
    }
}
