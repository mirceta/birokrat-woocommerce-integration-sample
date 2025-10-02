using System.Drawing;
using System.Windows.Forms;

using System;
using System.Collections.Generic;

namespace tests_gui.events
{
    public class ResultsRichTextBoxFormatter
{
    private RichTextBox richTextBox;

    public ResultsRichTextBoxFormatter(RichTextBox richTextBox)
    {
        this.richTextBox = richTextBox;
    }

    Color currentColor;
    bool isBold;
    public void AppendFormattedText(List<string> lines)
    {
        currentColor = richTextBox.ForeColor; // Default text color
        isBold = false;

        foreach (var line in lines)
        {
            if (IsBadge(line))
                {
                    SetStyle(line);
                }
                else
                {
                    // Apply formatting and append text
                    ApplyFormatting(line);
                }
            }
    }

        private void ApplyFormatting(string line)
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionColor = currentColor;
            richTextBox.SelectionFont = new Font(richTextBox.Font, isBold ? FontStyle.Bold : FontStyle.Regular);
            richTextBox.AppendText(line + Environment.NewLine);

            // Reset to default after appending
            richTextBox.SelectionColor = richTextBox.ForeColor;
            richTextBox.SelectionFont = richTextBox.Font;
        }

        private void SetStyle(string line)
        {
            string badgeContent = line.Trim('<', '>');
            if (badgeContent.Equals("bold", StringComparison.OrdinalIgnoreCase))
            {
                isBold = true;
            }
            else
            {
                // Reset bold to false when color changes
                isBold = false;
                currentColor = ColorFromName(badgeContent);
            }
        }

        private bool IsBadge(string text)
    {
        return text.StartsWith("<") && text.EndsWith(">");
    }

    private Color ColorFromName(string name)
    {
        // Attempt to parse the color name; default to black if not found
        try
        {
            return ColorTranslator.FromHtml(name);
        }
        catch
        {
            return Color.Black;
        }
    }
}



}
