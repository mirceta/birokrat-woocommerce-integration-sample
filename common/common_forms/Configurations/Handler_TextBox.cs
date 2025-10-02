using common_forms.Configurations;
using System.Linq;
using System.Windows.Forms;

namespace common_forms.Configurations
{
    public class Handler_TextBox : IControlHandler 
    {
        public void AssignValueToControl(Control ctrl, string value)
        {
            ((TextBox)ctrl).Text = value;
        }
        public string GetControlValue(Control ctrl) => ((TextBox)ctrl).Text ?? "";
        public string GetControlNameWithoutPrefix(Control ctrl) => new string(ctrl.Name.SkipWhile(x => char.IsLower(x)).ToArray());
        public bool DoesMatchTo(Control ctrl)
        {
            if (ctrl is TextBox)
            {
                if (ControlCheckHandler.Shared.Check<TextBox>(ctrl, "tb"))
                    return true;
            }
            return false;
        }
    }
}
