using gui_generator;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace gui_gen {
    public class InterfaceRenderer : CurrentValueControl {

        CurrentValue val;

        public InterfaceRenderer(int width, CurrentValue val, Func<int> rerender, int height_displacement, int depth) {

            this.val = val;
            
            Control ctr = new Control();
            ctr.Controls.Add(GenericControls.CreateTextfield(width, val.variable, val.type, depth));

            Control ctr2 = new RecursiveRenderer(width, height_displacement, rerender, depth, null).recurse(val.currentImplementation);
            ctr2.Location = new System.Drawing.Point(25, 30);

            ctr.Size = new System.Drawing.Size(1000, 30 + ctr2.Size.Height);
            ctr.BackColor = GenericControls.GetBackcolor(depth);
            ctr.Controls.Add(ctr2);
        }

        public override CurrentValue CurrentValue => val;
    }
}
