namespace common_ops.diagnostics
{

    /// <summary>
    /// For general use only important info is Result. Result will be true if operation succeeded. AdditionalInfo and ICheckImplementationName are used for extensive logging.
    /// </summary>
    public struct ResultRecord
    {
        public bool Result;
        public string ICheckImplementationName;
        public string[] AdditionalInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result">Did check succeed or not</param>
        /// <param name="executionMethod">Name of the class executing the method</param>
        /// <param name="additionalInfo">Aditional information for extensive logging. For general check you only need result</param>
        public ResultRecord(bool result, string executionMethod, params string[] additionalInfo)
        {
            Result = result;
            ICheckImplementationName = executionMethod;
            AdditionalInfo = additionalInfo;
        }
    }
}
