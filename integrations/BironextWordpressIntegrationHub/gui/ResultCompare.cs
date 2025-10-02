using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gui_gen {
    public partial class ResultCompare : Form
    {
        private Label lblDifferences;
        private Button btnNextDifference;
        private int currentDifferenceIndex = -1;
        private readonly System.Collections.Generic.List<int> differenceLines = new System.Collections.Generic.List<int>();

        public ResultCompare(string leftText, string rightText)
        {
            InitializeComponent();


            // Initialize the label and button
            lblDifferences = new Label
            {
                AutoSize = true,
                Location = new Point(10, 10),
                Text = "Differences: 0"
            };
            Controls.Add(lblDifferences);

            btnNextDifference = new Button
            {
                Text = "Move to Next Difference",
                Location = new Point(170, 10),
                Size = new Size(120, 35)
            };
            btnNextDifference.Click += BtnNextDifference_Click;
            Controls.Add(btnNextDifference);

            leftText = CanonicalizeAndFormatJson(leftText);
            rightText = CanonicalizeAndFormatJson(rightText);

            // Generate the diff using DiffPlex
            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diff = diffBuilder.BuildDiffModel(leftText, rightText);

            int lineNumber = 0;

            // Display the diffs in the rich text boxes
            foreach (var line in diff.Lines)
            {
                lineNumber++;
                Color lineColor;
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        lineColor = Color.LightGreen;
                        rtbRight.AppendText(line.Text + Environment.NewLine, lineColor);
                        differenceLines.Add(lineNumber);
                        break;
                    case ChangeType.Deleted:
                        lineColor = Color.Salmon;
                        rtbLeft.AppendText(line.Text + Environment.NewLine, lineColor);
                        differenceLines.Add(lineNumber);
                        break;
                    default:
                        lineColor = Color.Black;
                        rtbLeft.AppendText(line.Text + Environment.NewLine, lineColor);
                        rtbRight.AppendText(line.Text + Environment.NewLine, lineColor);
                        break;
                }
            }

            lblDifferences.Text = $"Differences: {differenceLines.Count / 2}";

            btnCancel.Click += (x, e) => Close();
        }

        public void setOnOkClick(Func<Task> onOkClick) {
            btnOK.Click += async (x, e) => await onOkClick.Invoke();
        }

        private void BtnNextDifference_Click(object sender, EventArgs e)
        {
            // Increment the current difference index, looping back to 0 if we reach the end
            currentDifferenceIndex++;
            if (currentDifferenceIndex >= differenceLines.Count) currentDifferenceIndex = 0; // Loop back to the first difference

            UpdateLabelAndHighlightDifference();
        }

        private void UpdateLabelAndHighlightDifference()
        {
            var line = differenceLines[currentDifferenceIndex];
            // Update the label to show the current difference number (note that we add 1 to make it human-readable)
            // Since each difference involves two list items (for left and right), we divide by 2 and adjust for human-readable indexing
            lblDifferences.Text = $"Difference: {currentDifferenceIndex / 2 + 1} of {differenceLines.Count / 2}";

            // Assuming rtbLeft and rtbRight are the RichTextBoxes used
            rtbLeft.Focus();
            rtbLeft.SelectAll();
            rtbLeft.SelectionBackColor = rtbLeft.BackColor; // Reset previously highlighted line
            rtbRight.SelectAll();
            rtbRight.SelectionBackColor = rtbRight.BackColor; // Reset previously highlighted line

            HighlightLine(rtbLeft, line);
            HighlightLine(rtbRight, line);
        }


        private void HighlightLine(RichTextBox rtb, int lineNumber)
        {
            var lines = rtb.Lines;
            if (lineNumber - 1 < lines.Length)
            {
                var start = rtb.GetFirstCharIndexFromLine(lineNumber - 1);
                var length = lines[lineNumber - 1].Length;
                rtb.Select(start, length);
                rtb.SelectionBackColor = Color.Yellow; // Highlight color
                rtb.ScrollToCaret(); // Scroll to make the highlighted line visible
            }
        }

        private string GetSubstring(RichTextBox rtb, int lineNumber, int length)
        {
            var lines = rtb.Lines;
            if (lineNumber - 1 < lines.Length)
            {
                var start = rtb.GetFirstCharIndexFromLine(lineNumber - 1);
                if (start + length <= rtb.Text.Length)
                {
                    return rtb.Text.Substring(start, length);
                }
            }
            return string.Empty; // Return empty string if the line number or length is out of bounds
        }


        private string CanonicalizeAndFormatJson(string json)
        {
            var parsedJson = JObject.Parse(json);
            var sorted = SortJObject(parsedJson);

            ReplaceNullWithEmptyString(sorted);

            return JsonConvert.SerializeObject(sorted, Formatting.Indented);
        }

        private void ReplaceNullWithEmptyString(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                foreach (var property in obj.Properties().ToList())
                {
                    if (property.Value.Type == JTokenType.Null)
                    {
                        property.Value = string.Empty;
                    }
                    else
                    {
                        ReplaceNullWithEmptyString(property.Value);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                var array = (JArray)token;
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].Type == JTokenType.Null)
                    {
                        array[i] = string.Empty;
                    }
                    else
                    {
                        ReplaceNullWithEmptyString(array[i]);
                    }
                }
            }
        }


        private JToken SortJObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return new JObject(
                        token.Children<JProperty>()
                            .OrderBy(prop => prop.Name)
                            .Select(prop => new JProperty(prop.Name, SortJObject(prop.Value)))
                    );

                case JTokenType.Array:
                    return new JArray(token.Children().Select(SortJObject));

                default:
                    return token;
            }
        }

    }

    public static class RichTextBoxExtensions {
        public static void AppendText(this RichTextBox box, string text, Color color) {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
