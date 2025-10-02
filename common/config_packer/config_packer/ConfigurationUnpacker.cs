using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace si.birokrat.common.config_packer
{
    internal class ConfigurationUnpacker
    {
        List<IControlHandler> controlHandlers;

        public ConfigurationUnpacker(List<IControlHandler> controlHandlers)
        {
            this.controlHandlers = controlHandlers;
        }

        ErrorBehavior errorBehavior = ErrorBehavior.THROW_EXCEPTION;
        public void SetOnErrorBehaviour(ErrorBehavior errorBehavior) { 
            this.errorBehavior = errorBehavior;
        }

        public void UnpackConfigurationToGui(Dictionary<string, object> config, Form form)
        {

            List<Control> ctrls = new ControlsScraper(controlHandlers).RelevantControls(form);
            foreach (Control ctrl in ctrls)
            {
                string value = ExtractValueOfControl_From_ConfigurationFile(config, ctrl);
                assignValueToControl(ctrl, value);

            }
        }

        private void assignValueToControl(Control ctrl, string value)
        {
            if (ctrl is TextBox)
            {
                ctrl.Text = value;
                return;
            }
            foreach (var x in controlHandlers)
            {
                if (x.Condition(ctrl))
                {
                    x.AssignValueToControl(ctrl, value);
                    return;
                }
            }
            ((ComboBox)ctrl).SelectedItem = value;
        }

        private string ExtractValueOfControl_From_ConfigurationFile(Dictionary<string, object> config, 
            Control ctrl)
        {
            string value = "";
            try
            {
                var tmp = config;
                string[] parts = ctrl.Name.Substring(2).Split('_');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!tmp.ContainsKey(parts[i])) {
                        if (errorBehavior == ErrorBehavior.ASSUME_DEFAULT)
                            break;
                    }

                    if (i == parts.Length - 1)
                    {
                        
                        value = (string)tmp[parts[i]];
                    }
                    else
                    {
                        tmp = (Dictionary<string, object>)tmp[parts[i]];
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("The value of the control was not in the dictionary!" + ex.Message);
            }

            return value;
        }
    }
}
