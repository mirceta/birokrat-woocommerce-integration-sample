using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common.database {
    public class CMsSqlConnectionString {
        #region // locals //
        private string _active_database = string.Empty;
        private string _default_database = string.Empty;
        #endregion
        #region // constructor //
        public CMsSqlConnectionString() {
            server = string.Empty;
            database = string.Empty;
            username = string.Empty;
            password = string.Empty;
            integratedSecurity = false;
        }
        #endregion
        #region // properties //
        public string server { get; set; }
        public string database {
            get { return _active_database; }
            set {
                if (string.IsNullOrEmpty(value)) {
                    _active_database = _default_database;
                } else {
                    if (string.IsNullOrEmpty(_default_database))
                        _default_database = value;
                    _active_database = value;
                }
            }
        }
        public string username { get; set; }
        public string password { get; set; }
        public bool integratedSecurity { get; set; }
        #endregion
        #region // overrides //
        public string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Data Source=" + server + ";");
            sb.Append("Initial Catalog=" + database + ";");
            if (integratedSecurity) {
                sb.Append("Integrated Security=true;");
            } else {
                sb.Append("User ID=" + username + ";");
                sb.Append("Password=" + password + ";");
                sb.Append("Persist Security Info=true;");
            }
            return sb.ToString();
        }
        #endregion
    }
}
