using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tests_gui.events;

namespace playground_forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            var rtbFormatter = new ResultsRichTextBoxFormatter(richTextBox1);

            var lines = new List<string>() {
                "<bold>",
                "krneki",
                "</bold>",
                "",
                "<green>",
                "somechomesome",
                "</green>",
                "",
                "nekinekineki"
            };
            rtbFormatter.AppendFormattedText(lines);
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            var rtbFormatter = new ResultsRichTextBoxFormatter(richTextBox1);

            var tmp = tbInput.Text.Split("\n").ToList();

            rtbFormatter.AppendFormattedText(tmp);
        }
    }
}
