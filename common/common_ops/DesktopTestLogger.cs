using System;
using System.IO;

namespace common_ops
{
    public static class DesktopTestLogger
    {
        private static readonly string FILE_NAME = "bull.txt";

        private static string BuildPath => Path.Combine("C:\\Users\\", Environment.UserName, "Desktop", FILE_NAME);

        public static void Write(string message)
        {
            try
            {
                File.WriteAllText(BuildPath, message);

            }
            catch (Exception ex)
            {

            }
        }

        public static void Append(string message)
        {
            try
            {
                var path = BuildPath;
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    File.WriteAllText(path, text + Environment.NewLine + message);
                }
                else
                {
                    File.WriteAllText(BuildPath, message);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
