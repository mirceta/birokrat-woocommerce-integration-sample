using System.Windows.Forms;

namespace si.birokrat.common.config_packer
{
    public class CheckboxHandler : IControlHandler
    {
        public void AssignValueToControl(Control ctrl, string value)
        {
            ((CheckBox)ctrl).Checked = value == "true";
        }

        public bool Condition(Control ctrl)
        {
            return ctrl is CheckBox;
        }

        public string Stringify(Control ctrl)
        {
            return ((CheckBox)ctrl).Checked ? "true" : "false";
        }
    }
}
