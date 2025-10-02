using gui_generator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gui_gen
{

    public partial class IntegrationObjectConfigRenderForm : Form {

        public const int HEIGHT_DISPLACEMENT = 30;

        CurrentValue root;
        string original;
        CurrentValueControl ctr;

        string input_json;
        string output_path;
        bool is_test;


        Point p = new Point(0, 0);

        public IntegrationObjectConfigRenderForm(string input_json, string output_json_path, bool is_test) {

            this.input_json = input_json;
            this.output_path = output_json_path;
            this.is_test = is_test;
            
            InitializeComponent();
            RenderGui();
            if (is_test) {
                button1.PerformClick();
                Environment.Exit(0);
            }
        }

        private void CompareResults() {
            var a = input_json;
            var b = File.ReadAllText(output_path);
            new ResultCompare(a, b).Show();
        }

        private void RenderGui() {
            var data = input_json;
            original = data;
            var json = JsonConvert.DeserializeObject<CurrentValue>(data);
            root = json;
            Rerender();
        }

        void Rerender() {
            p = panel1.AutoScrollPosition;
            panel1.Controls.Clear();

            if (ctr != null)
                root = ctr.CurrentValue;


            var memos = root.implementationOptions.ToList();
            var vars = root.dependencies.Where(x => x.type != "EntryObject`1").ToArray();
            var dememo = new VariableDememoizerDecorator(new Dememoizer(memos), vars);


            int width = 1000;
            ctr = new RecursiveRenderer(width, HEIGHT_DISPLACEMENT, () => { Rerender(); return 0; }, 0, dememo).recurse(root);
            

            panel1.AutoScroll = true;
            panel1.Controls.Add(ctr);
            p.Y = -p.Y;
            panel1.AutoScrollPosition = p;
        }

        private void button1_Click(object sender, EventArgs e) {

            var orig = JsonConvert.DeserializeObject<CurrentValue>(original);
            IntegrationConfigTools.NullImplementationOptions(orig);
            string json = JsonConvert.SerializeObject(orig);

            var currVal = ctr.CurrentValue;
            IntegrationConfigTools.NullImplementationOptions(currVal);
            string json2 = JsonConvert.SerializeObject(currVal);

            new ResultCompare(json, json2).Show();
        }
    }
}
