using gui_generator;
using System.Windows.Forms;

namespace gui_gen {
    public abstract class CurrentValueControl : Panel, ICurrentValueControl {
        public abstract CurrentValue CurrentValue { get; }
    }
}
