using System.Collections.Generic;
using System.Windows.Forms;

namespace si.birokrat.common.config_packer
{

    public interface IControlHandler {
        bool Condition(Control ctrl);
        string Stringify(Control ctrl);
        void AssignValueToControl(Control ctrl, string value);
    }

    internal class ConfigurationPacker
    {
        List<IControlHandler> controlHandlers;
        public ConfigurationPacker(List<IControlHandler> controlHandlers) { 
            this.controlHandlers = controlHandlers;
        }

        public Dictionary<string, object> GetConfigurationFromGui(Form form)
        {

            Dictionary<string, object> json = new Dictionary<string, object>();

            List<Control> ctrls = new ControlsScraper(controlHandlers).RelevantControls(form);

            foreach (Control ctrl in ctrls)
            {
                string name = ctrl.Name.Substring(2);

                string value = "";
                value = extractValueOutOfControl(ctrl);

                json = FillDict(json, name, value);
            }
            return json;
        }

        private string extractValueOutOfControl(Control ctrl)
        {
            string value;
            if (ctrl is TextBox)
            {
                value = ctrl.Text;
                return value;
            }

            foreach (var x in controlHandlers) {
                if (x.Condition(ctrl)) {
                    return x.Stringify(ctrl);
                }
            }
            return (string)((ComboBox)ctrl).SelectedItem;
        }

        private Dictionary<string, object> FillDict(Dictionary<string, object> dict, string name, string value)
        {
            string[] parts = name.Split('_');
            if (parts.Length > 1)
            {
                var some = new Dictionary<string, object>();
                if (!dict.ContainsKey(parts[0]))
                {
                    dict[parts[0]] = some;
                }
                else
                {
                    some = (Dictionary<string, object>)dict[parts[0]];
                }
                name = string.Join("_", subarray(parts, 1));
                dict[parts[0]] = FillDict(some, name, value);
            }
            else
            {
                dict[name] = value;
            }
            return dict;
        }

        private string[] subarray(string[] array, int start_index)
        {

            if (array.Length == 1) return null;

            string[] result = new string[array.Length - start_index];
            for (int i = start_index; i < array.Length; i++)
            {
                result[i - start_index] = array[i];
            }
            return result;
        }
    }
}
