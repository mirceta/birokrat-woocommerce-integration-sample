using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Controls
{
    public class DarkLabel : Label
    {
        public DarkLabel()
        {
            try
            {
                Font = new Font("Arial", 8, FontStyle.Regular);
            }
            catch
            {
                Font = SystemFonts.DefaultFont;
            }

            TextAlign = ContentAlignment.MiddleLeft;
            ForeColor = Color.WhiteSmoke;
            BackColor = CustomColors.BACKGROUND_COLOR;
        }
    }
}
