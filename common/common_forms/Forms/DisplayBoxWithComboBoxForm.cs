using common_forms.Controls;
using common_forms.Logging;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Forms
{
    /// <summary>
    /// Represents a form that displays a large text area within a customizable and resizable window. 
    /// Form will auto resize to fit the contents. Content is added to combo box. Class is configured to work with
    /// <see cref="RichTextBoxTextFormatter"/> but can work as standalone for not formatted text.
    /// </summary>
    public class DisplayBoxWithComboBoxForm : Form
    {
        private DarkRichTextBox _textBox;
        private DarkComboBox _comboBox;
        private readonly RichTextBoxTextFormatter _textFormatter;

        private readonly int _width;
        private readonly int MARGIN = 15;
        private readonly int CONTROL_HEIGHT = 30;
        private readonly Dictionary<string, string> _content;

        /// <summary>
        /// <inheritdoc cref="DisplayBoxWithComboBoxForm"/>
        /// </summary>
        /// <param name="displayText">The text to display inside the form.</param>
        /// <param name="content">AddContent to be displayed and selected via combo box.</param>
        /// <param name="width">The initial width of the form.</param>
        public DisplayBoxWithComboBoxForm(Dictionary<string, string> content, int width = 500)
            : this(width)
        {
            if (content == null)
                _content = new Dictionary<string, string>();
            else
                _content = content;
        }

        public DisplayBoxWithComboBoxForm(int width = 500)
        {
            this.Width = width;
            this.Height = width;

            _comboBox = new DarkComboBox
            {
                Top = MARGIN,
                Left = this.ClientSize.Width / 4,
                Width = this.ClientSize.Width / 2,
                Height = CONTROL_HEIGHT,
                Font = new Font("Courier New", 9, FontStyle.Regular),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
            };

            _textBox = new DarkRichTextBox
            {
                Left = MARGIN,
                Top = MARGIN * 2 + CONTROL_HEIGHT,
                Width = this.ClientSize.Width - 2 * MARGIN,
                Height = this.ClientSize.Height - 3 * MARGIN - CONTROL_HEIGHT,
                Font = new Font("Courier New", 9, FontStyle.Regular),
                Text = string.Empty,
                Multiline = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top,
            };

            Controls.Add(_textBox);
            Controls.Add(_comboBox);

            Text = "Info";
            BackColor = CustomColors.BACKGROUND_COLOR;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            _width = width;

            _textFormatter = new RichTextBoxTextFormatter(_textBox);

            _comboBox.SelectedIndexChanged += OnComboBox_SelectedIndexChanged;
            _textBox.VScroll += OnTextBox_VScroll;
        }

        private void OnTextBox_VScroll(object sender, System.EventArgs e)
        {
            _textBox.Refresh();
        }

        private void OnComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (_content.TryGetValue(_comboBox.Text, out var value))
                UpdateText(value);
        }

        /// <summary>
        /// Updates the text displayed in the text box.
        /// </summary>
        /// <param name="text">The new text to display.</param>
        public void AddContent(string key, string value)
        {
            _content.Add(key, value);
            _comboBox.Items.Add(key);

            if (_comboBox.Text == string.Empty)
            {
                _comboBox.SelectedIndex = 0;
            }
        }

        private void UpdateText(string text)
        {
            _textFormatter.DisplayAndFormatText(text);
            AdjustFormWidth();
        }

        private void AdjustFormWidth()
        {
            using (Graphics g = _textBox.CreateGraphics())
            {
                SizeF size = g.MeasureString(_textBox.Text, _textBox.Font);
                int newWidth = (int)size.Width + MARGIN * 2 + SystemInformation.VerticalScrollBarWidth * 2;
                this.Width = newWidth < _width ? _width : newWidth;
            }
            this.PerformLayout();
            _textBox.PerformLayout();
            _textBox.Refresh();
        }

        /// <summary>
        /// Will only permit one form with this type to be active
        /// </summary>
        public static DisplayBoxWithComboBoxForm ShowForm(Dictionary<string, string> content = default, int width = 500)
        {
            return Utils.StartForm(() => new DisplayBoxWithComboBoxForm(content, width));
        }

        /// <summary>
        /// Will not limit the amout of forms of this type that can be active
        /// </summary>
        public static DisplayBoxWithComboBoxForm ShowForm_NonRestrictive(Dictionary<string, string> content = default, int width = 500)
        {
            DisplayBoxWithComboBoxForm form = new DisplayBoxWithComboBoxForm(content, width);
            form.Show();
            return form;
        }
    }
}
