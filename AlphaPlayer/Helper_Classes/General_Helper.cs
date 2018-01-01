using System;
using System.Security.Principal;

namespace AlphaPlayer.Helper_Classes
{
    class General_Helper
    {
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.ToString("hh\\:mm\\:ss");
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
