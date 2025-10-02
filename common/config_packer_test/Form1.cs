using Newtonsoft.Json;
using si.birokrat.common.config_packer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace config_packer_test
{
    public partial class Form1 : Form
    {
        public const string INTEG_SETTINGS_FILE_NAME = "integsettings.json";

        public Form1()
        {
            InitializeComponent();
            loadConfig();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosed += (x, e) =>
            {
                var some = GetConfigAdapter().GetConfigurationFromGui(this);
                var tmp = JsonConvert.SerializeObject(some);
                File.WriteAllText(INTEG_SETTINGS_FILE_NAME, tmp);
            };
        }

        void loadConfig()
        {


            //string json = @"{""Test1"": ""aa"", ""Test2"": ""aa""}";
            string json = @"{""Test1"": ""aa""}";
            var tmp = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            
            /*
             ErrorBehavior parameter controls what happens when in the form there is a control for which
             no corresponding value in the JSON exists.
             If this happens we can either throw an exception or assume a default parameter!
             */
            GetConfigAdapter().UnpackConfigurationToGui(ErrorBehavior.ASSUME_DEFAULT, tmp, this);   
        }


        GuiToConfigurationAdapter GetConfigAdapter()
        {
            return new GuiToConfigurationAdapter(new List<IControlHandler> { new DateTimePickerHandler(), new CheckboxHandler() });
        }

    }
}
