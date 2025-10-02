using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace common
{
    public class ModeOfProgramStarting
    {
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
