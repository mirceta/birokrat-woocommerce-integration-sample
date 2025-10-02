using System.Windows.Forms;

namespace common_forms.Configurations
{
    internal class ControlCheckHandler
    {
        internal static readonly ControlCheckHandler Shared = new ControlCheckHandler();

        internal bool RespectNamingConvention = true;

        internal bool Check<T>(Control control, string prefix)
        {
            if (RespectNamingConvention)
            {
                if (control.Name.StartsWith(prefix))
                    return true;
            }
            else
            {
                if (control is T)
                    return true;
            }
            return false;
        }
    }
}
