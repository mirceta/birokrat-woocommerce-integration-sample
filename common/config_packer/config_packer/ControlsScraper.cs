using System.Collections.Generic;
using System.Windows.Forms;

namespace si.birokrat.common.config_packer
{
    internal class ControlsScraper
    {

        List<IControlHandler> controlHandlers;

        public ControlsScraper(List<IControlHandler> controlHandlers)
        {
            this.controlHandlers = controlHandlers;
        }

        public List<Control> RelevantControls(Control ctrl)
        {
             List<Control> res = new List<Control>();
            if (ctrl.Controls.Count == 0)
            {
                if (ctrl.Name.StartsWith("tb") || 
                    ctrl.Name.StartsWith("cb"))
                {
                    res.Add(ctrl);
                    return res;
                }
            }

            foreach (var x in controlHandlers) {
                if (x.Condition(ctrl)) {
                    res.Add(ctrl);
                    return res;
                }
            }

            for (int i = 0; i < ctrl.Controls.Count; i++)
            {
                var ctr = ctrl.Controls[i];
                res.AddRange(RelevantControls(ctr));
            }
            return res;
        }
    }
}
