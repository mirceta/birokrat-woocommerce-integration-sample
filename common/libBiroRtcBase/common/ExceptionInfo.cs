using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

namespace si.birokrat.rtc.common {
    public class ExceptionInfo : Exception {
        #region // properties //
        public string assemblyName { get; set; } = string.Empty;
        public string assemblyFullName { get; set; } = string.Empty;
        public Exception exception { get; set; } = null;
        public string customInfo { get; set; } = string.Empty;
        public List<ExceptionStackInfo> stackInfo { get; set; } = new List<ExceptionStackInfo>();
        #endregion
        #region // public static //
        public static ExceptionInfo Create(
            Exception ex = null,
            string customInfo = "") {

            #region // base //
            ExceptionInfo ei = new ExceptionInfo();
            ei.exception = ex;
            ei.customInfo = customInfo;
            #endregion

            #region // assembly //
            Assembly assembly = Assembly.GetExecutingAssembly();
            ei.assemblyFullName = assembly.FullName;
            ei.assemblyName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));
            #endregion

            #region // stack trace //
            StackTrace st = new StackTrace(true);
            for (int frameIndex = 1; frameIndex < st.FrameCount; frameIndex++) {
                StackFrame sf = st.GetFrame(frameIndex);
                ExceptionStackInfo esi = new ExceptionStackInfo(frameIndex - 1, sf);
                ei.stackInfo.Add(esi);
            }
            #endregion

            return ei;
        }
        public static void Save(ExceptionInfo ei) {
            #region // json settings //
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            #endregion

            #region // serialize //
            string json = JsonConvert.SerializeObject(ei, jsonSerializerSettings);
            #endregion

            #region // write //
            string fileName = ei.assemblyName + "." + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".exi";
            File.WriteAllText(fileName, json);
            #endregion
        }
        #endregion
    }
    public class ExceptionStackInfo {
        #region // constructor //
        public ExceptionStackInfo() { }
        public ExceptionStackInfo(int position, StackFrame stackFrame) {
            this.position = position;
            fileName = stackFrame.GetFileName();
            fileRow = stackFrame.GetFileLineNumber();
            fileColumn = stackFrame.GetFileColumnNumber();
            methodClass = stackFrame.GetMethod().ReflectedType.FullName;
            methodName = stackFrame.GetMethod().Name;
        }
        #endregion
        #region // properties //
        public int position { get; set; } = 0;
        public string fileName { get; set; } = string.Empty;
        public int fileRow { get; set; } = 0;
        public int fileColumn { get; set; } = 0;
        public string methodClass { get; set; } = string.Empty;
        public string methodName { get; set; } = string.Empty;
        #endregion
    }
}
