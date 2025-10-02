using common_forms.Controls;
using common_forms.Logging;
using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Forms
{
    /// <summary>
    /// Represents a form that displays a large text area within a customizable and resizable window. 
    /// Form will auto resize to fit the contents
    /// </summary>
    public class DisplayBoxForm : Form
    {
        private readonly RichTextBoxTextFormatter _textFormatter;
        private DarkRichTextBox textBox;
        private readonly int _width;
        private readonly int MARGIN = 15;

        /// <summary>
        /// <inheritdoc cref="DisplayBoxForm"/>
        /// </summary>
        /// <param name="displayText">The text to display inside the form.</param>
        /// <param name="label">The title of the window. If not specified, defaults to "Info".</param>
        /// <param name="width">The initial width of the form.</param>
        public DisplayBoxForm(string displayText, string label = "", int width = 500)
        {
            this.Width = width;
            this.Height = width;

            textBox = new DarkRichTextBox
            {
                Left = MARGIN,
                Top = MARGIN,
                Width = this.ClientSize.Width - 2 * MARGIN,
                Height = this.ClientSize.Height - 2 * MARGIN,
                Font = new Font("Courier New", 9, FontStyle.Regular),
                Multiline = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top,
            };
            textBox.VScroll += TextBox_VScroll;

            Controls.Add(textBox);

            Text = string.IsNullOrEmpty(label) ? "Info" : label;
            BackColor = CustomColors.BACKGROUND_COLOR;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            Text = "Input Box";
            _width = width;

            _textFormatter = new RichTextBoxTextFormatter(textBox);
            UpdateText(displayText);
        }

        private void TextBox_VScroll(object sender, System.EventArgs e)
        {
            textBox.Refresh();
        }

        /// <summary>
        /// Updates the text displayed in the text box.
        /// </summary>
        /// <param name="text">The new text to display.</param>
        public void UpdateText(string text)
        {
            _textFormatter.DisplayAndFormatText(text);
            AdjustFormWidth();
        }

        private void AdjustFormWidth()
        {
            using (Graphics g = textBox.CreateGraphics())
            {
                SizeF size = g.MeasureString(textBox.Text, textBox.Font);
                int newWidth = (int)size.Width + MARGIN * 2 + SystemInformation.VerticalScrollBarWidth * 2;
                this.Width = newWidth < _width ? _width : newWidth;

                int newHeight = (int)size.Height + MARGIN * 2;
                this.Height = newHeight > Screen.PrimaryScreen.Bounds.Height ? Screen.PrimaryScreen.Bounds.Height : newHeight;
            }
            this.PerformLayout(); 
            textBox.PerformLayout();
            textBox.Refresh();
        }

        /// <summary>
        /// Will only permit one form with this type to be active
        /// </summary>
        public static DisplayBoxForm ShowForm(string displayText, string label = "", int width = 500)
        {
            return Utils.StartForm(() => new DisplayBoxForm(displayText, label, width));
        }

        /// <summary>
        /// Will not limit the amout of forms of this type that can be active
        /// </summary>
        public static DisplayBoxForm ShowForm_NonRestrictive(string displayText, string label = "", int width = 500)
        {
            DisplayBoxForm form = new DisplayBoxForm(label, displayText, width);
            form.Show();
            return form;
        }
    }
}
