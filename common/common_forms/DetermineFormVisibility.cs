using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace common_forms
{
    public class DetermineFormVisibility
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        const int GWL_STYLE = -16;
        const int WS_VISIBLE = 0x10000000;

        public bool IsFormVisible(Form parent)
        {
            int style = GetWindowLong(parent.Handle, GWL_STYLE);
            if ((style & WS_VISIBLE) == 0)
            {
                return false;
            }
            return true;
        }
    }
}
