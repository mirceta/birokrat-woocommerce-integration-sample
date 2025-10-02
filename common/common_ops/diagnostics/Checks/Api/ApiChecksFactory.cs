using common_ops.diagnostics.Checks.Api.Checks;
using common_ops.diagnostics.Checks.Api.Utils;

namespace common_ops.diagnostics.Checks.Api
{
    public class ApiChecksFactory
    {
        /// <summary>
        /// Will check Artikle calls CRUD operations. Can artikel be created, updated and deleted. Will retrurn true if all operations succeed.
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: result for each operation in format: NameOfOperation||Postfix.
        /// Separated with <c>||</c>. Example: Create Call||OK\r\Update Call||OK\r\Delete Call||OK</para>
        /// 
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR </para>
        /// </summary>
        public Api_Artikel_Check Build_Api_Artikel_Check(string apiKey = "")
        {
            return new Api_Artikel_Check(new ApiHelper(), apiKey);
        }

        /// <summary>
        /// Will check if simple connection can be established with next api/test endpoint. Will throw and error and return false if operation can't succeed. 
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: operation result in format: Api simple test||OK if operation succeeds.
        /// Otherwise will return exception error</para>
        /// 
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR </para>
        /// </summary>
        public Api_Test_Check Build_Api_Test_Check(string apiKey = "")
        {
            return new Api_Test_Check(new ApiHelper(), apiKey);
        }
    }
}
