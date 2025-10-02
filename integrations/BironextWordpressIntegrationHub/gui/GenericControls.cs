using System;
using System.Drawing;
using System.Windows.Forms;

namespace gui_gen {
    public class GenericControls {

        public static Random random = new Random();

        public static GenericTextField CreateTextfield(int width, string labelContent, string textboxContent, int depth) {
            return new GenericTextField(width, labelContent, textboxContent, depth);
        }

        public static GenericCheckbox CreateCheckbox(int width, string labelContent, bool isChecked, int depth)
        {
            return new GenericCheckbox(width, labelContent, isChecked, depth);
        }


        public static Color GetBackcolor(int depth) {
            if (depth % 2 == 0) {
                return Color.FromArgb(255, 255, 255);
            } else {
                return Color.FromArgb(155, 155, 155);
            }
        }

        public static Color GetFrontcolor(int depth) {
            if (depth % 2 == 0) {
                return Color.FromArgb(0, 0, 0);
            } else {
                return Color.FromArgb(0, 0, 0);
            }
        }
    }

    public class GenericTextField : Control {

        TextBox tbControl;

        public GenericTextField(int width, string labelContent, string textboxContent, int depth) {

            int useFraction = 6;

            var label = new Label();
            label.Text = labelContent;
            label.Size = new System.Drawing.Size(width / useFraction, 30);
            label.Location = new System.Drawing.Point(50, 0);
            label.Dock = DockStyle.Left;
            label.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(label);

            int contentLength = textboxContent == null ? 0 : textboxContent.Length;
            contentLength = Math.Max((useFraction - 1) * width / useFraction, contentLength * 10);

            var textbox = new TextBox();
            textbox.Text = textboxContent;
            textbox.Size = new System.Drawing.Size(contentLength, 30);
            textbox.Location = new System.Drawing.Point(width / useFraction, 0);
            tbControl = textbox;
            Controls.Add(textbox);

            Size = new System.Drawing.Size(width, 30);


            ForeColor = GenericControls.GetFrontcolor(depth);
            BackColor = GenericControls.GetBackcolor(depth);
        }

        public string Text => tbControl.Text;
    }

    public class GenericCheckbox : Control
    {
        CheckBox cbControl;

        public GenericCheckbox(int width, string labelContent, bool isChecked, int depth)
        {

            int useFraction = 6;

            var label = new Label();
            label.Text = labelContent;
            label.Size = new Size(width / useFraction, 30);
            label.Location = new Point(0, 0); // Adjusted for better alignment with the checkbox
            label.Dock = DockStyle.Left;
            label.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(label);

            var checkbox = new CheckBox();
            checkbox.Checked = isChecked;
            checkbox.Size = new Size(50, 30); // Adjusted size for consistency
            checkbox.Location = new Point(width / useFraction, 0); // Adjusted location for proper alignment
            cbControl = checkbox;
            Controls.Add(checkbox);

            Size = new Size(width, 30); // Adjusted overall size for better visibility

            ForeColor = GenericControls.GetFrontcolor(depth);
            BackColor = GenericControls.GetBackcolor(depth);
        }

        public bool Checked => cbControl.Checked;
    }

}
