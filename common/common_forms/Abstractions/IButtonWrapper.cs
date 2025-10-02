using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Abstractions
{
    public interface IButtonWrapper
    {
        string Text { get; }
        void SetText(string text);
        void ChangeBackColor(Color color);
        void ChangeForeColor(Color color);
        Button GetButton();
    }
}
