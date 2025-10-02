using gui_generator;
using System.Windows.Forms;

namespace gui_gen {
    public class PrimitiveRenderer : CurrentValueControl {

        GenericTextField gtf;
        CurrentValue val;
        GenericCheckbox chk;

        public PrimitiveRenderer(int width, CurrentValue val, int depth) {
            this.val = val;

            if (val.type == "Boolean")
            {
                chk = GenericControls.CreateCheckbox(width, val.variable, val.value == "True", depth);
                this.Controls.Add(chk);
                this.Size = chk.Size;
            }
            else
            {
                var ctr = GenericControls.CreateTextfield(width, val.variable, val.value, depth);
                gtf = ctr;
                this.Controls.Add(ctr);
                this.Size = ctr.Size;
            }
        }

        public override CurrentValue CurrentValue {
            get {
                if (val.type == "Boolean")
                {
                    val.value = chk.Checked ? "True" : "False";
                }
                else
                {
                    val.value = gtf.Text;
                }
                return val;
            }
        }
    }
}
