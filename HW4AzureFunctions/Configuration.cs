using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace HW4AzureFunctions {
    class Configuration {
        private static string GetEnvironmentVariable (string variable, string defaultValue = "") {
            string val = Environment.GetEnvironmentVariable (variable);
            return (val.Trim () == "") ?
                defaultValue :
                val;
        }

        public static string AzureStorageConnectionString {
            get { return GetEnvironmentVariable (Constants.AzureStorageConnectionStringEntry); }
        }
    }
}
