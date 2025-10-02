using System.Windows.Forms;

namespace common_forms.Options
{
    public class TextFields_LimitSigns
    {

        /// <summary>
        /// Will only allow numbers to be filled in text field
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="limit">Maximum amount of digits allowed</param>
        public void AllowOnlyWholeNumbers(TextBoxBase textBox, int limit = 8)
        {
            textBox.TextChanged += (sender, e) =>
            {
                var text = textBox.Text.Trim();
                if (text.Length > limit)
                    text = text.Substring(0, limit);
                if (string.IsNullOrEmpty(text))
                    return;
                if (!int.TryParse(text, out var taxNumber))
                    text = text.Substring(0, text.Length - 1);
                if (!int.TryParse(text, out taxNumber))
                    text = string.Empty;
                textBox.Text = text;

                textBox.SelectionStart = textBox.Text.Length;
                textBox.SelectionLength = 0;
            };
        }
    }
}
