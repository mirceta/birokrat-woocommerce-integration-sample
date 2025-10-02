using common_ops.diagnostics.Checks.Networking.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Networking.Checks
{
    /// <summary>
    /// This will create new Firewall rules via PowerShell. Repair option is only used for creating new rule if there is none for specific port.
    /// Otherwise it will return a WARNING and you will have to check firewall rules manually.
    /// Firewall can have multiple rules for same port but note that most restrictive will be enforced.
    /// With repair option new rule will be created you can set rule name, Direction (Inbound/Outbound), Protocol (UDP, TCP) and port number. Rule
    /// will be created for all profiles and any IP.
    /// <para>IMPORTANT: Info about firewall rules are fetched only once (Lazy) per factory since it is long running operation. If you need to recheck a rule build new factory.</para>
    /// </summary>
    /// <returns>Returned <see cref="ResultRecord.AdditionalInfo"/> with full information about the rules</returns>
    public class Firewall_PortsByName_CheckAndRepair : ICheck
    {
        private readonly IShellExecutor _shellExecutor;
        private readonly IFirewallRulesFetcher _firewallRulesFetcher;
        private readonly string _ruleName;
        private readonly FirewallRule_Direction _direction;
        private readonly FirewallRule_Protocol _protocol;
        private readonly int _port;
        private readonly bool _doRepair;

        /// <summary>
        /// <inheritdoc cref="Firewall_PortsByName_CheckAndRepair"/>
        /// </summary>
        public Firewall_PortsByName_CheckAndRepair(
            IShellExecutor shellExecutor,
            IFirewallRulesFetcher rulesFetcher,
            string ruleName,
            FirewallRule_Direction direction,
            FirewallRule_Protocol protocol,
            int port,
            bool doRepair)
        {
            _shellExecutor = shellExecutor;
            _firewallRulesFetcher = rulesFetcher;
            _ruleName = ruleName;
            _direction = direction;
            _protocol = protocol;
            _port = port;
            _doRepair = doRepair;
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
            List<string> info = new List<string>();

            var portInfoResult = await _firewallRulesFetcher.Fetch();

            var existing = SplitResultIntoLines(portInfoResult)
                .Where(x => x.Contains(_port.ToString()))
                .Where(x => x.Contains(_direction.ToString()))
                .Where(x => x.Contains(_protocol.ToString()))
                .Select(x =>
                {
                    var line = x.Split(ShellCommands.SHELL_SEPARATOR).ToArray();
                    return new FirewallInfo(line[0], line[1], line[2], line[3]);
                }).ToArray();

            if (!existing.Any())
                info.Add($"No rule for port {_port} with Name: {_ruleName}, Direction: {_direction} and Protocol: {_protocol}! {TextConstants.POSTFIX_WARNING}");
            else
            {
                if (existing.Length > 1)
                {
                    info.Add("Possible duplicate rules? FIX MANUALLY! " + TextConstants.POSTFIX_ERROR);
                    foreach (var item in existing)
                        info.Add(BuildInfoLine(item, TextConstants.POSTFIX_WARNING));

                    return new ResultRecord(true, GetType().Name, info.ToArray());
                }
                else
                {
                    var item = existing.First();

                    if (IsRuleValid(item))
                        info.Add(BuildInfoLine(item, TextConstants.POSTFIX_OK));
                    else
                        info.Add(BuildInfoLine(item, TextConstants.POSTFIX_WARNING));
                }
            }

            if (!existing.Any() && _doRepair)
            {
                var command = ShellCommands.GetSetFirewallRuleCommand(_ruleName, _protocol.ToString(), _port, _direction.ToString());
                var newRuleResult = await _shellExecutor.ExecuteInBackgroundAsync(command, true);

                if (newRuleResult.Contains("SUCCESS"))
                    info.Add($"Rule created successfully!  Name: {_ruleName}, Protocol: {_protocol.ToString()}, Port: {_port}, Direction: {_direction.ToString()}. {TextConstants.POSTFIX_REPAIR}");
                else
                {
                    info.Add("Rule creation failed! " + TextConstants.POSTFIX_ERROR);
                    return new ResultRecord(false, GetType().Name, info.ToArray());
                }
            }

            return new ResultRecord(true, GetType().Name, info.ToArray());
        }

        private bool IsRuleValid(FirewallInfo item)
        {
            if (!item.Name.Equals(_ruleName, StringComparison.OrdinalIgnoreCase))
                return false;
            if (item.Protocol != _protocol.ToString())
                return false;
            return true;
        }

        private string[] SplitResultIntoLines(string result)
        {
            string[] lines = result.Split(new[] { '\n' });

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd('\r');
            }

            return lines;
        }

        private string BuildInfoLine(FirewallInfo info, string postfix)
        {
            return $"Firewall Rule Found - Name: {info.Name}, Protocol: {info.Protocol}, Port: {info.Port}, Direction: {info.Direction}. {postfix}";
        }
    }

    internal struct FirewallInfo
    {
        internal string Name;
        internal string Direction;
        internal string Protocol;
        internal string Port;

        public FirewallInfo(string name, string direction, string protocol, string port)
        {
            Name = name;
            Direction = direction;
            Protocol = protocol;
            Port = port;
        }

        public override string ToString()
        {
            return $"{Name}{TextConstants.DELIMITER}{Direction}{TextConstants.DELIMITER}{Protocol}{TextConstants.DELIMITER}{Port}";
        }
    }
}
