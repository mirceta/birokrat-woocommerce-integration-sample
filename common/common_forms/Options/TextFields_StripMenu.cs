using System.Windows.Forms;

namespace common_forms.Options
{
    public class TextFields_StripMenu
    {
        public void CreateCopyPasteMenu(TextBoxBase textBox)
        {
            var menuContext = new ContextMenuStrip();

            ToolStripMenuItem copyMenuItem = new ToolStripMenuItem("Copy");
            copyMenuItem.Click += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(textBox.SelectedText))
                    Clipboard.SetText(textBox.SelectedText);
            };

            ToolStripMenuItem pasteMenuItem = new ToolStripMenuItem("Paste");
            pasteMenuItem.Click += (sender, e) =>
            {
                if (string.IsNullOrEmpty(Clipboard.GetText()))
                    return;

                if (!string.IsNullOrEmpty(textBox.SelectedText))
                {
                    textBox.SelectedText = Clipboard.GetText();
                }
                else
                {
                    int selectionIndex = textBox.SelectionStart;
                    textBox.Text = textBox.Text.Insert(selectionIndex, Clipboard.GetText());
                    textBox.SelectionStart = selectionIndex + Clipboard.GetText().Length;
                }
            };

            menuContext.Items.Add(copyMenuItem);
            menuContext.Items.Add(pasteMenuItem);

            textBox.ContextMenuStrip = menuContext;
        }
    }
}
