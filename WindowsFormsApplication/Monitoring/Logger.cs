using PacketSniffer.PackageTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication.Utils;

namespace WindowsFormsApplication.Monitoring
{
    public class Logger
    {
        private StreamWriter file;
        private int _packetCount;

        public Logger()
        {
            _packetCount = 0;

            file = new StreamWriter("Log.txt", true);
            file.Write("\n\n");
            file.WriteLine(String.Format("\tStarted monitoring at {0}\n", DateTime.Now));
            file.WriteLine("Settings:\n{0}\n", Settings.GetSettingsString());
            file.WriteLine(String.Format("{0, -10}{1, -12}{2, -10}{3, -10}{4, -30}{5, -18}{6, -15}{7, -18}{8, -18}{9, -10}", "Number", "Date", "Time", "Protocol", "Application", "Source IP", "Source Port", "Destination IP", "Destination Port", "Packet Length"));
        }

        public void WriteToFile(IPPacket ipPacket)
        {
            try
            {
                file.WriteLine(String.Format("{0, -10}{1}", ++_packetCount, ipPacket.ToString()));
            }
            catch
            {
                MessageBox.Show("Could not write to file.");
            }            
        }

        public void CloseFile()
        {
            file.Close();
        }
    }
}
