using System;
using System.Drawing;
using System.Windows.Forms;

namespace common_forms.Logging
{
    /// <summary>
    /// Takes text with special formatting codes (like those used in markdown) and makes it look pretty in a RichTextBox, 
    /// applying styles such as bold, italic, colored... See <see cref="TextTag"/>. This method makes sure to safely update the RichTextBox even if it's being 
    /// used from different parts of the program at once.
    /// <para>Limitations: For tag will be applied for whole line. There cant be multiple tags in the same line. Tags are case sensitive. First letter is upper
    /// while others are lower as defined in <see cref="TextTag"/></para>
    /// </summary>
    /// <param name="text">The text that needs to be shown in the RichTextBox. This text can include special "tags" that tell 
    /// the program how to format the text. For example, tags can make the text bold, italic, underlined, have a strikethrough, 
    /// or change its color. The method reads these tags and formats the text accordingly as it shows it in the RichTextBox.</param>
    /// <remarks>
    /// 
    /// <para>EXAMPLE:</para>
    /// <para>var formatter = new RichTextBoxTextFormatter(richTextBox);</para>
    /// <para><![CDATA[var text = $"<Red>Neki</Red>{Environment.NewLine}<Green>Neki</Green>";]]></para>
    /// <para><![CDATA[var text = $"{formatter.WrapTextInTag("Neki", TextTag.Red)}{Environment.NewLine}{formatter.WrapTextInTag("Neki", TextTag.Green)}";]]></para>
    /// <para>formatter.DisplayAndFormatText(text);</para>
    /// </remarks>
    public class RichTextBoxTextFormatter
    {
        private readonly RichTextBox _textBox;
        private readonly Array _tagTypes;
        private readonly Tags _tags;

        /// <summary>
        /// <inheritdoc cref="RichTextBoxTextFormatter"/>
        /// </summary>
        /// <param name="textBox">The RichTextBox control where formatted text will be displayed.</param>
        public RichTextBoxTextFormatter(RichTextBox textBox)
        {
            _tags = new Tags();
            _textBox = textBox;
            _tagTypes = Enum.GetValues(typeof(TextTag));
        }

        /// <summary>
        /// Formats the provided text and displays it in the associated RichTextBox.
        /// This method checks if the operation needs to be invoked on the UI thread and formats the text using defined markdown tags.
        /// </summary>
        /// <param name="text">The text to format and display.</param>
        public void DisplayAndFormatText(string text)
        {
            if (_textBox.InvokeRequired)
            {
                _textBox.BeginInvoke((MethodInvoker)delegate ()
                {
                    UpdateTextWithMarkdowns(text);
                });
            }
            else
            {
                UpdateTextWithMarkdowns(text);
            }
        }

        public string WrapTextInTag(string text, TextTag tag)
        {
            return _tags.WrapInTag(text, tag);
        }

        private void UpdateTextWithMarkdowns(string text)
        {
            ApplyMarkdowns(text);
            _textBox.Refresh();
        }

        private void ApplyMarkdowns(string text)
        {
            string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            _textBox.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                bool effects = false;
                TextTag? tagType = TextTag.None;

                foreach (TextTag value in _tagTypes)
                {
                    var startTag = _tags.StartingTag(value);
                    if (lines[i].Contains(startTag))
                    {
                        tagType = value;
                        effects = true;
                        lines[i] = lines[i].Replace(startTag, string.Empty);
                        lines[i] = lines[i].Replace(_tags.EndingTag(value), string.Empty);
                        break;
                    }
                }

                if (!effects)
                    _textBox?.AppendText($"{lines[i]}{Environment.NewLine}");
                else
                {
                    switch (tagType)
                    {
                        case TextTag.Bold:
                            ChangeFont(lines[i], FontStyle.Bold);
                            break;
                        case TextTag.Italic:
                            ChangeFont(lines[i], FontStyle.Italic);
                            break;
                        case TextTag.Underline:
                            ChangeFont(lines[i], FontStyle.Underline);
                            break;
                        case TextTag.Strikeout:
                            ChangeFont(lines[i], FontStyle.Strikeout);
                            break;
                        case TextTag.Red:
                            ChangeColor(lines[i], Color.Red);
                            break;
                        case TextTag.Orange:
                            ChangeColor(lines[i], Color.Orange);
                            break;
                        case TextTag.Violet:
                            ChangeColor(lines[i], Color.Violet);
                            break;
                        case TextTag.Yellow:
                            ChangeColor(lines[i], Color.Yellow);
                            break;
                        case TextTag.Blue:
                            ChangeColor(lines[i], Color.Blue);
                            break;
                        case TextTag.Cyan:
                            ChangeColor(lines[i], Color.Cyan);
                            break;
                        case TextTag.Green:
                            ChangeColor(lines[i], Color.Green);
                            break;
                        case TextTag.Lime:
                            ChangeColor(lines[i], Color.LimeGreen);
                            break;
                        default:
                            _textBox.AppendText($"{lines[i]}{Environment.NewLine}");
                            break;
                    }
                }
            }
        }

        private void ChangeFont(string line, FontStyle style)
        {
            Font currentFont = _textBox.SelectionFont;
            if (currentFont == null)
                return;

            FontStyle newFontStyle = currentFont.Style ^ style;
            _textBox.SelectionFont = new Font(currentFont, newFontStyle);
            _textBox.AppendText($"{line}{Environment.NewLine}");
        }

        private void ChangeColor(string line, Color color)
        {
            _textBox.SelectionStart = _textBox.TextLength;
            _textBox.SelectionLength = 0;
            _textBox.SelectionColor = color;
            _textBox.AppendText($"{line}{Environment.NewLine}");
            _textBox.SelectionColor = _textBox.ForeColor;
        }
    }
}
