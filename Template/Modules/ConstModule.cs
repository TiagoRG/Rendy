using System;
using System.Collections.Generic;
using System.Text;

namespace Rendy.Modules
{
    public class ConstModule
    {
        public const string footer = "© Rendy | Made by TiagoRG#8003";

        public static string GetIconUrl(string value)
        {
            if (value == "success")
                return "https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/success-24.png";
            if (value == "error")
                return "https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/error-43.png";
            if (value == "warning")
                return "https://icons.veryicon.com/png/o/miscellaneous/cloud-call-center/warning-60.png";
            if (value == "info")
                return "https://icons.veryicon.com/png/o/miscellaneous/blue-market-monitoring/tip-4.png";
            return "";
        }
    }
}
