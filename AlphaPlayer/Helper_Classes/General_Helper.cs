using System;

namespace AlphaPlayer.Helper_Classes
{
    class General_Helper
    {
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.ToString("hh\\:mm\\:ss");
        }
    }
}
