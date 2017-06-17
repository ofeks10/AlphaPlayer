using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
