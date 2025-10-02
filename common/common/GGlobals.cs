using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace si.birokrat.next.common {
    public class GGlobals
    {

        public const string NetworkPathForDocuments = @"\\sqlbirokrat\Andersen\";

        public static void BirokratSafetyChecks()
        {
            testAccess(@"\\sqlbirokrat\Andersen");
            testAccess(@"C:\Birokrat");
            if (!IsUserAdministrator())
                throw new Exception("PROSIMO UPORABLJAJTE PROGRAM SAMO KOT UPORABNIK ADMINISTRATOR!");
        }

        private static void testAccess(string path)
        {
            // does user have access to sqlbirokrat and other network paths?
            path = $@"{path}\test123dowehavenetaccess.txt";
            if (File.Exists(path))
            {
            }
            else
            {
                File.WriteAllText(path, "some");
            }
            File.Delete(path);
        }

        private static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            WindowsIdentity user = null;
            try
            {
                //get the currently logged in user
                user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            finally
            {
                if (user != null)
                    user.Dispose();
            }
            return isAdmin;
        }
    }
}
