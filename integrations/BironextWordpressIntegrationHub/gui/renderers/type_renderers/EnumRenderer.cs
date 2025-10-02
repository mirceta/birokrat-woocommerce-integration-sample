using gui_generator;
using System.Linq;
using System.Windows.Forms;

namespace gui_gen {
    public class EnumRenderer : CurrentValueControl {

        CurrentValue val;

        ComboBox combo;
        IDememoizer dememo;
        public EnumRenderer(CurrentValue val, int depth, IDememoizer dememo) {
            this.val = val;
            this.dememo = dememo;
            
            if (val.elements == null) {
                var nk = dememo.Find(val.type.Replace("@#@#", ""));
                val.elements = nk.elements;
            }

            int sizex = 300;
            int sizey = 30;

            string label = val.variable;
            var history = val.elements.Select(x => x.value).ToList();

            string name = label;
            this.Size = new System.Drawing.Size(sizex, sizey);

            var lbl = new Label();
            lbl.Text = label;
            lbl.Size = new System.Drawing.Size(sizex / 2, sizey);
            lbl.Location = new System.Drawing.Point(0, 0);
            this.Controls.Add(lbl);

            var hist = history.Distinct().ToList();

            combo = new NoMouseWheelComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Size = new System.Drawing.Size(sizex / 2, sizey);
            combo.Location = new System.Drawing.Point(sizex / 2, 0);
            combo.Items.AddRange(hist.ToArray());

            string tmp = hist.First();
            if (val.value != null)
                tmp = hist.Where(x => x == val.value).Single();


            int tm = hist.IndexOf(tmp);

            combo.SelectedValue = tm;
            combo.SelectedItem = tm;
            combo.SelectedIndex = tm;

            this.Controls.Add(combo);

        }

        public override CurrentValue CurrentValue { get {
                val.value = (string)combo.SelectedItem;
                return val;
            }
        }
    }
}
