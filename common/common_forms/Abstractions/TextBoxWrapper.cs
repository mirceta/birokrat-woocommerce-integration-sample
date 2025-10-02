using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Abstractions
{
    public class TextBoxWrapper : ITextBoxWrapper
    {
        private readonly TextBox _textBox;

        public TextBoxWrapper(TextBox textBox)
        {
            _textBox = textBox;
        }

        public string Text => _textBox.Text;
        public TextBox GetTextBox() => _textBox;

        public void SetText(string text)
        {
            if (_textBox.InvokeRequired)
            {
                _textBox.BeginInvoke((MethodInvoker)delegate ()
                {
                    _textBox.Text = text;
                });
            }
            else
            {
                _textBox.Text = text;
            }
        }

        public void ChangeBackColor(Color color)
        {
            if (_textBox.InvokeRequired)
            {
                _textBox.BeginInvoke((MethodInvoker)delegate ()
                {
                    _textBox.BackColor = color;
                });
            }
            else
            {
                _textBox.BackColor = color;
            }
        }

        public void ChangeForeColor(Color color)
        {
            if (_textBox.InvokeRequired)
            {
                _textBox.BeginInvoke((MethodInvoker)delegate ()
                {
                    _textBox.ForeColor = color;
                });
            }
            else
            {
                _textBox.ForeColor = color;
            }
        }
    }
}
