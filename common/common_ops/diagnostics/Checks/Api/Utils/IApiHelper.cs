using BirokratNext;
using System;

namespace common_ops.diagnostics.Checks.Api.Utils
{
    public interface IApiHelper
    {
        string BuildArgumentFromResult(string input, bool didSucceed);
        ApiClientV2 BuildClient(string apiKey = "");
        string BuildExceptionErrorArgument(Exception ex);
        string GetWebAddress();
    }
}
