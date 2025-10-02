using System;

namespace biro_to_woo_common.error_handling.reports
{
    public interface IOperationReport
    {
        object ObjectUnderInspection { get; }
        string Id { get; }
        string Signature { get; }
        
        OperationOutcome OperationOutcome { get; }
        Exception Ex { get; }
    }

    public enum OperationOutcome {
        Success,
        Warning,
        Error,
        Ignore
    }
}
