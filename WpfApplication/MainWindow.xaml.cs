using PacketSniffer;
using PacketSniffer.PackageTypes;
using PacketSniffer.PackageTypes.TransportLayerPacketTypes;
using PacketSniffer.PacketsMonitors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApplication.Monitoring;
using static PacketSniffer.PacketsMonitors.BaseMonitor;

namespace WpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SocketMonitor _socketMonitor;
        private IPAddress _ipAddress;
        private BackgroundWorker _bworker;
        private ObservableCollection<IPPacket> _packetList;
        private static object _syncLock;

        public MainWindow()
        {
            InitializeComponent();

            InitHost();
            InitMonitor();           
        }

        private void InitMonitor()
        {
            _syncLock = new object();
            _packetList = new ObservableCollection<IPPacket>();
            BindingOperations.EnableCollectionSynchronization(_packetList, _syncLock);

            _socketMonitor = new SocketMonitor(_ipAddress, this);

            _bworker = new BackgroundWorker();
            _bworker.DoWork += (s, ev) =>
            {
                _socketMonitor.StartMonitoring();
            };
        }

        private void InitHost()
        {
            string hostName = Dns.GetHostName();
            var ipAddressArray = Dns.GetHostAddresses(hostName);
            var ipAddressList = ipAddressArray.ToList();
            _ipAddress = ipAddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            _bworker.RunWorkerAsync();            
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            _socketMonitor.IsRunning = false;
        }

        public void AddPacketToList(IPPacket ipPacket)
        {
            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke((Action)(() => AddPacketToList(ipPacket)));
            }
            else
            {
                lock (_syncLock)
                {
                    _packetList.Add(ipPacket);
                    dataGrid.ItemsSource = _packetList;
                    //dataGrid.UpdateLayout();
                    dataGrid.ScrollIntoView(ipPacket);
                }            
            }
        }
    }
}
