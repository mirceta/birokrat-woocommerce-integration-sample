using core.structs;
using si.birokrat.next.common.build;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpplugingen {
    public class WppluginGen {

        static Dictionary<string, string> orderStatusHooks = new Dictionary<string, string>() {
                { "processing", "add_action( 'woocommerce_order_status_processing', array($this, 'onOrderStatusChanged'));\n"},
                { "on-hold", "add_action( 'woocommerce_order_status_on-hold', array($this, 'onOrderStatusChanged'));\n"},
                { "completed", "add_action( 'woocommerce_order_status_completed', array($this, 'onOrderStatusChanged'));\n"}
            };


        static string attachmentHook = "add_filter('woocommerce_email_attachments', array($this, 'onEmailIntercepted'), 10, 3);";

        static string productHooks = @"
        add_action('transition_post_status', array($this, 'wpse_110037_new_posts'), 10, 3);
		add_action('pre_post_update', array($this, 'beforeOperationOnProduct'), 10, 2);
            ";


        string deploymentpath;
        string birowooaddress;
        public WppluginGen(string deploymentPath) {
            this.deploymentpath = deploymentPath;
            birowooaddress = "https://doesnt.matter.anymore/";
        }

        public async Task createPlugin(PhpPluginConfig PhpPluginConfigVal, string apikey)
        {

            string basepluginpath = Path.Combine(Build.ProjectPath, "wpplugin", "birokrat");
            string path = Path.Combine(basepluginpath, "birokrat.php");
            string program = File.ReadAllText(path);


            // remove the comments around the preprocessor lines
            program = program.Replace("/*********", "");
            program = program.Replace("*********/", "");

            program = program.Replace("[![![APIKEY]!]!]", "'" + apikey + "';\n");
            program = program.Replace("[![![SERVERADDRESS]!]!]", "'" + birowooaddress + "';\n");

            if (PhpPluginConfigVal.AttachmentHook)
            {
                program = program.Replace("[![![ATTACHMENT_ACCEPTABLE_STATUSES]!]!]", makeAcceptableStatusesString(PhpPluginConfigVal.AcceptableAttachmentOrderStatuses));
                program = program.Replace("[![![ATTACHMENTHOOK]!]!]", attachmentHook);
            }
            else
            {
                program = program.Replace("[![![ATTACHMENT_ACCEPTABLE_STATUSES]!]!]", "array();\n");
                program = program.Replace("[![![ATTACHMENTHOOK]!]!]", "");
            }
            if (PhpPluginConfigVal.ProductHooks)
            {
                program = program.Replace("[![![PRODUCTHOOKS]!]!]", productHooks);
            }
            else
            {
                program = program.Replace("[![![PRODUCTHOOKS]!]!]", "");
            }

            if (PhpPluginConfigVal.OrderStatusHooks != null && PhpPluginConfigVal.OrderStatusHooks.Count > 0)
            {
                string some = string.Join("", PhpPluginConfigVal.OrderStatusHooks.Select(x => orderStatusHooks[x]));
                program = program.Replace("[![![ORDERSTATUSHOOKS]!]!]", some);
            }
            else
            {
                program = program.Replace("[![![ORDERSTATUSHOOKS]!]!]", "");
            }

            CopyFilesRecursively(basepluginpath, deploymentpath);

            string phpContent = program;
            phpContent = phpContent.Replace("\r\n", "\n").Replace("\r", "\n");
            phpContent = phpContent.Trim();
            File.WriteAllText(Path.Combine(deploymentpath, "birokrat.php"), phpContent, new UTF8Encoding(false));
        }

        private string makeAcceptableStatusesString(List<string> acceptableStatuses)
        {

            string stats = string.Join(",", acceptableStatuses.Select(x => "\"" + x + "\""));

            string acceptablestatuses = $"array({stats});\n";
            return acceptablestatuses;
        }

        private void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string originalFilePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                string destFilePath = originalFilePath.Replace(sourcePath, targetPath);

                // Read the contents of the file
                string fileContents = File.ReadAllText(originalFilePath);
                fileContents = fileContents.Replace("\r\n", "\n").Replace("\r", "\n");
                fileContents = fileContents.Trim();

                // Normalize the file content to UTF-8 without BOM and write to the destination
                File.WriteAllText(destFilePath, fileContents, new UTF8Encoding(false));
            }
        }
    }
}
