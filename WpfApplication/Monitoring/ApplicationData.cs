using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication.Monitoring
{
    public class ApplicationData
    {
        public string ApplicationName { get; }
        public int PacketsCount { get; set; }
        public int PacketsSize { get; set; }

        public ApplicationData(string applicationName)
        {
            ApplicationName = applicationName;
        }
    }
}
