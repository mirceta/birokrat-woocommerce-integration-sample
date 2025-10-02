using common_forms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Forms
{
    /// <summary>
    /// Represents a form that prompts the user for input with an optional autofill suggestion.
    /// </summary>
    public class InputForm : Form
    {
        private DarkTextBox textBox;
        private DarkButton okButton;
        private DarkButton cancelButton;
        private Label label;

        /// <summary>
        /// <inheritdoc cref="InputForm"/>
        /// </summary>
        /// <param name="prompt">The text prompt to display above the input box.</param>
        /// <param name="autofillContent">Default text to appear in the input box.</param>
        /// <param name="width">Width of the form.</param>
        public InputForm(string prompt, string autofillContent = "", int width = 550)
        {
            Width = width;
            double aspectRatio = .714;
            Height = (int)(Width * aspectRatio);

            // Calculate positions and sizes based on the dynamic dimensions
            int margin = 15;
            int labelHeight = 80;
            int textBoxHeight = ClientSize.Height / 2;
            int textBoxWidth = ClientSize.Width - 2 * margin;
            int buttonWidth = 100;
            int buttonHeight = 40;

            label = new Label
            {
                Left = margin,
                Top = margin,
                Text = prompt,
                Width = ClientSize.Width - margin * 2,
                Height = labelHeight,
                AutoSize = false,
                BackColor = CustomColors.BACKGROUND_COLOR,
                ForeColor = Color.White
            };

            textBox = new DarkTextBox
            {
                Left = margin,
                Top = textBoxHeight / 2,
                Width = textBoxWidth,
                Height = textBoxHeight,
                Text = autofillContent,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            int buttonTop = textBox.Top + textBox.Height + margin;

            cancelButton = new DarkButton
            {
                Text = "Cancel",
                Left = ClientSize.Width * 2 / 3 - buttonWidth / 2,
                Width = buttonWidth,
                Height = buttonHeight,
                Top = buttonTop,
                DialogResult = DialogResult.Cancel
            };

            okButton = new DarkButton
            {
                Text = "OK",
                Left = ClientSize.Width / 3 - buttonWidth / 2,
                Width = buttonWidth,
                Height = buttonHeight,
                Top = buttonTop,
                DialogResult = DialogResult.OK
            };


            // Add controls to the form
            Controls.Add(textBox);
            Controls.Add(label);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            // Form settings
            AcceptButton = okButton;
            CancelButton = cancelButton;

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = CustomColors.BACKGROUND_COLOR;

            Text = "Input Box";
        }

        /// <summary>
        /// Retrieves the text input by the user.
        /// </summary>
        /// <returns>The text entered by the user.</returns>
        public string GetInputValue()
        {
            return textBox.Text;
        }

        public static (string content, DialogResult dialogResult) ShowForm(string prompt, string autofill = "")
        {
            using (var form = new InputForm(prompt, autofill))
            {
                DialogResult result = form.ShowDialog();
                return (form.GetInputValue(), result);
            }
        }
    }
}
