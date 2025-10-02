using System.Collections.Generic;
using System.Windows.Forms;

namespace si.birokrat.common.config_packer
{


    /*
     The GuiToConfigurationAdapter class bridges a GUI (Form1) and a configuration dictionary. 
     
     You need to comply with the following naming convention:

     // json: tbSomeChome => { "SomeChome": "3" } 
     //    tbSome_Chome => { "Some": { "Chome": "3" }}




     */
    public class GuiToConfigurationAdapter
    {
        List<IControlHandler> controlHandlers;
        public GuiToConfigurationAdapter(List<IControlHandler> controlHandlers)
        {
            this.controlHandlers = controlHandlers;
        }
        public Dictionary<string, object> GetConfigurationFromGui(Form form)
        {
            return new ConfigurationPacker(controlHandlers).GetConfigurationFromGui(form);
        }
        public void UnpackConfigurationToGui(ErrorBehavior errbeh, Dictionary<string, object> config, Form form)
        {
            var x = new ConfigurationUnpacker(controlHandlers);
            x.SetOnErrorBehaviour(errbeh);
            x.UnpackConfigurationToGui(config, form);
        }
    }
}
