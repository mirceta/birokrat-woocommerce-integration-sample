using common_ops.diagnostics.Checks.Api.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Api.Checks
{
    /// <summary>
    /// Will check if simple connection can be established with next api/test endpoint. Will throw and error and return false if operation can't succeed. 
    /// 
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: operation result in format: Api simple test||OK if operation succeeds.
    /// Otherwise will return exception error</para>
    /// 
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR </para>
    /// </summary>
    public class Api_Test_Check : ICheck
    {
        private readonly bool _repair;
        private readonly IApiHelper _apiHelper;
        private readonly string _apiKey;

        /// <summary>
        /// <inheritdoc cref="Api_Test_Check"/>
        /// </summary>
        public Api_Test_Check(IApiHelper apiHelper, string apiKey = "")
        {
            _apiHelper = apiHelper;
            _apiKey = apiKey;
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
            try
            {
                var apiClient = _apiHelper.BuildClient(_apiKey);
                var testCheck = await apiClient.HttpClient.GetAsync(_apiHelper.GetWebAddress() + "test");

                var result = testCheck.StatusCode == System.Net.HttpStatusCode.OK;
                var content = _apiHelper.BuildArgumentFromResult("Api simple test", result);
                return new ResultRecord(result, GetType().Name, content);
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, _apiHelper.BuildExceptionErrorArgument(ex));
            }
        }
    }
}
