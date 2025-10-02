using System.Collections.Generic;
using System.Windows.Forms;

namespace common_forms.Configurations.Utilities
{
    internal class AdapterHelper
    {
        public Dictionary<string, string> PackControls(Control parent, List<IControlHandler> acceptedHandlers)
        {
            var recognized = GetAllRecognizedControls(parent, acceptedHandlers);
            var dict = new Dictionary<string, string>();

            foreach (Control rec in recognized)
            {
                var selected = GetControlHandler(rec, acceptedHandlers);
                dict.Add(selected.Handler.GetControlNameWithoutPrefix(rec), selected.Handler.GetControlValue(rec));
            }
            return dict;
        }

        public void UnpackControls(Control parent, List<IControlHandler> acceptedHandlers, Dictionary<string, string> config)
        {

            if (parent.InvokeRequired)
            {
                parent.BeginInvoke((MethodInvoker)delegate ()
                {
                    UnpackControls(parent, acceptedHandlers, config);
                });
            }

            var recognized = GetAllRecognizedControls(parent, acceptedHandlers);

            foreach (var cfg in config)
            {
                foreach (Control control in recognized)
                {
                    var handler = GetControlHandler(control, acceptedHandlers).Handler;
                    var name = handler.GetControlNameWithoutPrefix(control);
                    if (cfg.Key == name)
                        handler.AssignValueToControl(control, cfg.Value);
                }
            }
        }

        private IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;

                if (control.HasChildren)
                {
                    foreach (Control childControl in GetAllControls(control))
                        yield return childControl;
                }
            }
        }

        private List<Control> GetAllRecognizedControls(Control parent, List<IControlHandler> acceptedHandlers)
        {
            var controls = GetAllControls(parent);
            var recognized = new List<Control>();

            foreach (Control ctrl in controls)
            {
                var result = GetControlHandler(ctrl, acceptedHandlers);
                if (result.Result)
                    recognized.Add(ctrl);
            }
            return recognized;
        }

        private (bool Result, IControlHandler Handler) GetControlHandler(Control control, List<IControlHandler> acceptedHandlers)
        {
            foreach (var handler in acceptedHandlers)
            {
                if (handler.DoesMatchTo(control))
                    return (Result: true, Handler: handler);
            }
            return (Result: false, Handler: null);
        }
    }
}
