using gui_generator;

namespace gui_gen {
    public class NopRenderer : CurrentValueControl {

        public NopRenderer(int depth) {
            var ctr = GenericControls.CreateTextfield(5, "NOP", "NOP", depth);
            this.Controls.Add(ctr);
            this.Size = ctr.Size;
        }

        public override CurrentValue CurrentValue => null;
    }

    public class VariableRenderer : CurrentValueControl {


        CurrentValue value;
        public VariableRenderer(CurrentValue value) {
            this.value = value;
            this.Size = new System.Drawing.Size(0, 0);
        }

        public override CurrentValue CurrentValue => value;
    }
}
