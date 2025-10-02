using common_forms.Configurations;
using System.Linq;
using System.Windows.Forms;

namespace common_forms.Configurations
{
    public class Handler_CheckBox : IControlHandler
    {
        public void AssignValueToControl(Control ctrl, string value)
        {
            value = value.Trim().ToLower();
            ((CheckBox)ctrl).Checked = value == "true";
        }
        public string GetControlValue(Control ctrl) => ((CheckBox)ctrl).Checked ? "true" : "false";
        public string GetControlNameWithoutPrefix(Control ctrl) => new string(ctrl.Name.SkipWhile(x => char.IsLower(x)).ToArray());

        public bool DoesMatchTo(Control ctrl) 
        {
            if (ctrl is CheckBox)
            {
                if (ControlCheckHandler.Shared.Check<CheckBox>(ctrl, "chb"))
                    return true;
            }
            return false;
        }
    }
}
