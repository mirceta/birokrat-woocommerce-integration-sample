using common_ops.diagnostics.Checks.Networking.Checks;
using common_ops.diagnostics.Checks.Networking.Utils;
using common_ops.Executors.Shell;

namespace common_ops.diagnostics.Checks.Networking
{
    public class NetworkingFactory
    {
        private readonly IShellExecutor _shellExecutor;
        private readonly FirewallRulesFetcher _firewallRulesFetcher;

        public NetworkingFactory()
        {
            _shellExecutor = new ShellExecutor();
            _firewallRulesFetcher = new FirewallRulesFetcher(_shellExecutor);
        }

        /// <summary>
        /// This will create new Firewall rules via PowerShell. Repair option is only used for creating new rule if there is none for specific port.
        /// Otherwise it will return a WARNING and you will have to check firewall rules manually.
        /// Firewall can have multiple rules for same port but note that most restrictive will be enforced.
        /// With repair option new rule will be created you can set rule name, Direction (Inbound/Outbound), Protocol (UDP, TCP) and port number. Rule
        /// will be created for all profiles and any IP.
        /// <para>IMPORTANT: Info about firewall rules are fetched only once (Lazy) per factory since it is long running operation. If you need to recheck a rule build new factory.</para>
        /// </summary>
        /// <returns>Returned <see cref="ResultRecord.AdditionalInfo"/> with full information about the rules</returns>
        public ICheck Build_PortRuleByNameCheck(
            string ruleName,
            FirewallRule_Direction direction,
            FirewallRule_Protocol protocol,
            int port,
            bool doRepair = false)
        {
            return new Firewall_PortsByName_CheckAndRepair(_shellExecutor, _firewallRulesFetcher, ruleName, direction, protocol, port, doRepair);
        }

        /// <summary>
        /// Represents a check and optional repair operation to ensure that a specified domain
        /// is correctly trusted under Windows Internet Settings (ZoneMap\Domains) for execution permissions.
        /// <para>
        /// This check verifies if the domain is registered under the Local Intranet zone (ZoneId = 1).
        /// If the domain is missing and repair is enabled, the class attempts to add the required registry entry.
        /// </para>
        /// <para>
        /// The operation executes PowerShell commands asynchronously through an injected <see cref="IShellExecutor"/>.
        /// Results are returned as <see cref="ResultRecord"/> instances, containing success status and detailed messages.
        /// </para>
        /// <para>
        /// Typical usage involves first checking whether the execution permission exists, and if necessary,
        /// automatically repairing it to prevent Windows "Open File - Security Warning" prompts when running files from network shares.
        /// </para>
        /// <para>
        /// <see cref="ResultRecord.AdditionalInfo"/> will contain lines for each operation (check and repair). For final result only
        /// look at <see cref="ResultRecord.Result"/>.
        /// </para>
        /// </summary>
        public ICheck Build_ZoneIdentifierRulesCheck(string domainName, bool doRepair = false)
        {
            return new ZoneIdentifier_ExecutionPermission_CheckAndRepair(_shellExecutor, domainName, doRepair);
        }

        //TODO ??
        //public ICheck Build_PortRuleByProgramName()

        /// <summary>
        /// Temporarily connects to a network share, identifies the user account associated with the connection,
        /// and ensures cleanup of any active session before and after the check. Will return user for the connection
        /// </para>
        /// <para>
        /// The operation executes PowerShell commands asynchronously through an injected <see cref="IShellExecutor"/>.
        /// Results are returned as <see cref="ResultRecord"/> instances, containing success status and detailed messages.
        /// </para>
        /// <para>
        /// <see cref="ResultRecord.AdditionalInfo"/> Result will depend if connection can be established. Additional info
        /// will contain username
        /// look at <see cref="ResultRecord.Result"/>.
        /// </para>
        /// </summary>
        public ICheck Build_UsedUser_InternalConnection_Check(string domainName, bool doRepair = false)
        {
            return new UsedUser_InternalConnection_Check(_shellExecutor, domainName, doRepair);
        }
    }
}
