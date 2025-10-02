using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Abstractions
{
    public class ButtonWrapper : IButtonWrapper
    {
        private readonly Button _button;

        public ButtonWrapper(Button button)
        {
            _button = button;
        }

        public Button GetButton() => _button;
        public string Text => _button.Text;

        public void ChangeBackColor(Color color)
        {
            if (_button.InvokeRequired)
            {
                _button.BeginInvoke((MethodInvoker)delegate ()
                {
                    _button.BackColor = color;
                });
            }
            else
            {
                _button.BackColor = color;
            }
        }

        public void ChangeForeColor(Color color)
        {
            if (_button.InvokeRequired)
            {
                _button.BeginInvoke((MethodInvoker)delegate ()
                {
                    _button.ForeColor = color;
                });
            }
            else
            {
                _button.ForeColor = color;
            }
        }

        public void SetText(string text)
        {
            if (_button.InvokeRequired)
            {
                _button.BeginInvoke((MethodInvoker)delegate ()
                {
                    _button.Text = text;
                });
            }
            else
            {
                _button.Text = text;
            }
        }
    }
}
