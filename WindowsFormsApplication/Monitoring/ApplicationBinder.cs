using PacketSniffer.PackageTypes;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication.Monitoring
{
    public class ApplicationBinder
    {
        private ProcessStartInfo _processStartInfo;
        private Process _process;

        private StreamReader _output;
        private StreamReader _error;

        private IPAddress _localIP;
        private string _protocol;        

        private object syncLock;

        public ApplicationBinder(IPAddress localIP, string protocol)
        {
            _protocol = protocol;
            _localIP = localIP;

            // Initializare fereastra pentru netstat
            _processStartInfo = new ProcessStartInfo("cmd.exe");
            _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _processStartInfo.UseShellExecute = false;
            _processStartInfo.RedirectStandardInput = true;
            _processStartInfo.RedirectStandardError = true;
            _processStartInfo.RedirectStandardOutput = true;
            _processStartInfo.CreateNoWindow = true;

            syncLock = new object();
        }

        public void BindApplicationToPacket(IPPacket ipPacket)
        {
            if (ipPacket.Protocol == "TCP" || ipPacket.Protocol == "UDP")
            {
                string portNumber;
                if (ipPacket.SourceAddress.Equals(_localIP))
                {
                    ipPacket.Direction = "Outgoing";
                    portNumber = ipPacket.TransportLayerPacket.SourcePort;
                }
                else
                {
                    ipPacket.Direction = "Incoming";
                    portNumber = ipPacket.TransportLayerPacket.DestinationPort;
                }

                string pid = GetPIDForPort(portNumber);
                var process = Process.GetProcesses().ToList().Where(x => x.Id.ToString() == pid).FirstOrDefault();
                ipPacket.ApplicationName = process != null ? process.ProcessName : "Could not find process";
            }
            else
            {
                ipPacket.ApplicationName = string.Empty;
            }
        }

        private string GetPIDForPort(string portNumber)
        {
            // Initializare rulare utilitate netstat
            _processStartInfo.Arguments = string.Format("/c \"netstat.exe -aon -p {0} | find \":{1}\"\"", _protocol, portNumber);

            _process = new Process();
            _process.StartInfo = _processStartInfo;

            _process.Start();

            // redirectarea iesirilor
            _output = _process.StandardOutput;
            _error = _process.StandardError;

            string content = _output.ReadToEnd();
            string exitCode = _process.ExitCode.ToString();

            string substring = content.TrimStart(' ');
            string protocol;
            string localAddress;
            string foreignAddress;
            string state = string.Empty;
            string port;
            string pid = "-1";

            // Parsarea string-ului rezultat
            while (!state.Equals("ESTABLISHED") && substring.Contains(":" + portNumber))
            {
                try
                {
                    substring = content.TrimStart(' ');
                    int indexOfWhiteSpace = substring.IndexOf(' ');
                    protocol = substring.Substring(0, indexOfWhiteSpace);

                    substring = substring.Substring(indexOfWhiteSpace).TrimStart(' ');
                    indexOfWhiteSpace = substring.IndexOf(':');
                    localAddress = substring.Substring(0, indexOfWhiteSpace);

                    substring = substring.Substring(indexOfWhiteSpace).TrimStart(' ');
                    indexOfWhiteSpace = substring.IndexOf(' ');
                    port = substring.Substring(1, indexOfWhiteSpace - 1);

                    substring = substring.Substring(indexOfWhiteSpace).TrimStart(' ');
                    indexOfWhiteSpace = substring.IndexOf(' ');
                    foreignAddress = substring.Substring(0, indexOfWhiteSpace);

                    if (protocol == "TCP")
                    {
                        substring = substring.Substring(indexOfWhiteSpace).TrimStart(' ');
                        indexOfWhiteSpace = substring.IndexOf(' ');
                        state = substring.Substring(0, indexOfWhiteSpace);

                        pid = new string(substring.Substring(indexOfWhiteSpace).TrimStart(' ').Substring(0, 5).Where(c => Char.IsDigit(c)).ToArray());
                    }
                    else
                    {
                        if (substring.Split('\n').Length > 2)
                        {
                            pid = new string(substring.Substring(indexOfWhiteSpace).TrimStart(' ').Substring(0, 5).Where(c => Char.IsDigit(c)).ToArray());
                        }
                        else
                        {
                            pid = new string(substring.Substring(indexOfWhiteSpace).TrimStart(' ').Where(c => Char.IsDigit(c)).ToArray());
                        }                        
                    }
                    content = content.Split('\n')[1];

                    if(protocol == "UDP")
                    {
                        break;
                    }
                }
                catch
                {

                }
            }            

            return pid;
        }
    }
}
