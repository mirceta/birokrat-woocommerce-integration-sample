using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Next.Checks
{
    /// <summary>
    /// By default Checks if the specified TCP ports (5000, 19000, 19001, 19002, 19005) are open. Optionaly you can provide ports as an argument in constructor.
    /// Executes a shell command to retrieve the status of the ports and determines if they are properly opened.
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: [0] - requested ports, [1] requested ports that are properly opened.
    /// Will return false if the shell command fails or if all requeste ports are not opened.</para>
    /// </summary>
    public class BiroNext_OpenedPorts_Check : ICheck
    {
        private readonly IShellExecutor _shellExecutor;
        private readonly int[] _ports;

        /// <summary>
        /// <inheritdoc cref="BiroNext_OpenedPorts_Check"/>
        /// </summary>
        public BiroNext_OpenedPorts_Check(IShellExecutor shellExecutor, params int[] ports)
        {
            _shellExecutor = shellExecutor;
            _ports = ports;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            string result = string.Empty;
            if (_ports.Length > 0)
                result = await _shellExecutor.Get_TCPPorts_ListOpenedAsync(_ports);
            else
                result = await _shellExecutor.Get_TCPPorts_ListOpenedAsync(BiroNextConstants.NextPorts);

            if (result.EndsWith(System.Environment.NewLine))
                result = result.Substring(0, result.Length - System.Environment.NewLine.Length);

            var lines = result.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim())
                .ToArray();

            ResultRecord record;
            if (lines.Length == 2)
                record = new ResultRecord(CheckResult(lines), GetType().Name, lines);
            else
                record = new ResultRecord(false, GetType().Name, lines);

            return record;
        }

        private bool CheckResult(string[] lines)
        {
            if (lines.Length > 1)
                return lines[0].Length == lines[1].Length;
            return false;
        }
    }
}
