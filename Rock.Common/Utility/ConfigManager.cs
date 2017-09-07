using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Rock.Common.Utility
{
    public static class ConfigManager
    {
        public static string GetConfigValue(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return "true";
            }
        }
    }
}
