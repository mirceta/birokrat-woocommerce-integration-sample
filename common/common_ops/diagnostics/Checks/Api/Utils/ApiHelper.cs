using BirokratNext;
using common_ops.diagnostics.Constants;
using System;

namespace common_ops.diagnostics.Checks.Api.Utils
{
    internal class ApiHelper : IApiHelper
    {
        private string _address = "http://localhost:19000/api/";
        private string _apiKey = "SO3onPC7AhmrSgI54J6uNDKEfYmFpJlk+Ze7nskPQQw=";

        public ApiClientV2 BuildClient(string apiKey = "")
        {
            var key = string.IsNullOrEmpty(apiKey) ? _apiKey : apiKey;
            return new ApiClientV2(apiAddress: _address, apiKey: key);
        }

        public string BuildArgumentFromResult(string input, bool didSucceed)
        {
            if (didSucceed)
                return BuildOKArgument(input);
            return BuildERRORArgument(input);
        }

        private string BuildOKArgument(string input)
        {
            return input + TextConstants.DELIMITER + TextConstants.POSTFIX_OK;
        }

        private string BuildERRORArgument(string input)
        {
            return input + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR;
        }

        public string BuildExceptionErrorArgument(Exception ex)
        {
            return TextConstants.POSTFIX_ERROR + ": " + ex + ". " + TextConstants.POSTFIX_ERROR;
        }

        public string GetWebAddress()
        {
            return _address;
        }
    }
}
