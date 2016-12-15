using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication.Utils
{
    public class Settings
    {
        public static List<string> BlockedPorts { get; set; }
        public static bool BlockPacketsWithNoProcess { get; set; }
        public static bool LogToFile { get; set; }

        public Settings()
        {
            BlockedPorts = new List<string>();
            BlockPacketsWithNoProcess = false;
            LogToFile = false;
        }

        public static string GetSettingsString()
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("Block Packets With no Process: {0}\n", BlockPacketsWithNoProcess);

            if(BlockedPorts.Count > 0)
            {
                result.AppendFormat("Blocked ports: {0}", BlockedPorts[0]);

                for(int i = 1; i < BlockedPorts.Count; i++)
                {
                    result.AppendFormat(", {0}", BlockedPorts[i]);
                }
            }

            return result.ToString();
        }
    }
}
