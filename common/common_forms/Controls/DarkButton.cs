using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Controls
{
    public class DarkButton : Button
    {
        private Color borderColor = CustomColors.BORDER_COLOR;

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }

        public DarkButton()
        {
            try
            {
                Font = new Font("Arial", 8, FontStyle.Regular);
            }
            catch
            {
                Font = SystemFonts.DefaultFont;
            }
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            TextAlign = ContentAlignment.MiddleCenter;
            ForeColor = Color.WhiteSmoke;
            BackColor = Color.Black;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            int thickness = 2;
            int halfThickness = thickness / 2;
            using (Pen p = new Pen(borderColor, thickness))
            {
                pevent.Graphics.DrawRectangle(p, new Rectangle(halfThickness, halfThickness,
                    ClientSize.Width - thickness, ClientSize.Height - thickness));
            }
        }
    }
}
