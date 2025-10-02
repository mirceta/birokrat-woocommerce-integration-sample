namespace common_ops.diagnostics.Constants
{
    public static class TextConstants
    {
        public static readonly string DELIMITER = "||";
        public static readonly string NULL_FIELD = "null";

        public static readonly string POSTFIX_OK = "OK";
        public static readonly string POSTFIX_WARNING = "WARNING";
        public static readonly string POSTFIX_ERROR = "ERROR";
        public static readonly string POSTFIX_REPAIR = "REPAIR";


        public static string GetPostfixEndingIfAny(string line)
        {
            if (line.EndsWith(POSTFIX_REPAIR, System.StringComparison.CurrentCultureIgnoreCase))
                return POSTFIX_REPAIR;
            if (line.EndsWith(POSTFIX_ERROR, System.StringComparison.CurrentCultureIgnoreCase))
                return POSTFIX_ERROR;
            if (line.EndsWith(POSTFIX_WARNING, System.StringComparison.CurrentCultureIgnoreCase))
                return POSTFIX_WARNING;
            if (line.EndsWith(POSTFIX_OK, System.StringComparison.CurrentCultureIgnoreCase))
                return POSTFIX_OK;
            return string.Empty;
        }
    }
}
