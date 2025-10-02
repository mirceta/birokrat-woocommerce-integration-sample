using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Abstractions
{
    public interface ITextBoxWrapper
    {
        string Text { get; }
        void SetText(string text);
        void ChangeBackColor(Color color);
        void ChangeForeColor(Color color);
        TextBox GetTextBox();
    }
}
