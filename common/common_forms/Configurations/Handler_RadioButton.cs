using common_forms.Configurations;
using System.Linq;
using System.Windows.Forms;

namespace common_forms.Configurations
{
    public class Handler_RadioButton : IControlHandler 
    {
        public void AssignValueToControl(Control ctrl, string value)
        {
            value = value.Trim().ToLower();
            ((RadioButton)ctrl).Checked = value == "true";
        }
        public string GetControlValue(Control ctrl) => ((RadioButton)ctrl).Checked ? "true" : "false";
        public string GetControlNameWithoutPrefix(Control ctrl) => new string(ctrl.Name.SkipWhile(x => char.IsLower(x)).ToArray());

        public bool DoesMatchTo(Control ctrl)
        {
            if (ctrl is RadioButton)
            {
                if (ControlCheckHandler.Shared.Check<RadioButton>(ctrl, "rbtn"))
                    return true;
            }
            return false;
        }
    }
}
