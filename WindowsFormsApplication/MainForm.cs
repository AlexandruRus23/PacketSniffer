using PacketSniffer.PackageTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WindowsFormsApplication.DataAccessLayer;
using WindowsFormsApplication.Monitoring;
using WindowsFormsApplication.Utils;

namespace WindowsFormsApplication
{
    public partial class MainForm : Form
    {
        private SocketMonitor _socketMonitor;
        private IPAddress _ipAddress;
        private BackgroundWorker _bworker;

        private List<IPPacket> _packetList;

        private DataTable _dataTable;

        private int _verticleScrollBarPosition;
        private bool _stopRefreshing;
        private bool _isRunning;

        private ChartContent _pieChartContent;
        private ChartContent _timeChartContent;

        private ContentData _pieChartData;
        private ContentData _timeChartData;

        public int GRAPH_THREAD_SLEEP_TIME { get; set; }
        public int PIE_CHART_THREAD_SLEEP_TIME { get; set; }
        public int DATA_GRID_THREAD_SLEEP_TIME { get; set; }

        private Thread _pieChartThread;
        private Thread _graphThread;
        private Thread _dataGridThread;

        DAL dal;

        private readonly object syncLock = new object();

        public MainForm()
        {
            InitializeComponent();

            InitDataBase();
            InitHost();
            InitMonitor();
            InitGrid();
            InitPiechart();
            InitTimeGraph();

            Settings settings = new Settings();
        }
        
        private void InitDataBase()
        {
            try
            {
                dal = new DAL();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }            
        }

        //private void RefreshGrid()
        //{
        //    dataGridView1.SuspendLayout();

        //    if (!_stopRefreshing)
        //    {
        //        //dataGridView1.DataSource = null;

        //        //int newCount = _packetList.Count;

        //        //for (int i = _packetCount; i < newCount; i++)
        //        //{
        //        //    _dataTable.Rows.Add(_packetList[i].TimeStamp, _packetList[i].Direction, _packetList[i].Protocol, _packetList[i].ApplicationName,
        //        //    _packetList[i].SourceAddress, _packetList[i].TransportLayerPacket.SourcePort, _packetList[i].DestinationAddress,
        //        //    _packetList[i].TransportLayerPacket.DestinationPort, _packetList[i].TTL, _packetList[i].HeaderLength, _packetList[i].MessageLength);
        //        //}

        //        //_packetCount = newCount;

        //        //_dataTable = dal.GetIPPackets();

        //        //_dataTable.Rows.Add(ipPacket.TimeStamp, ipPacket.Direction, ipPacket.Protocol, ipPacket.ApplicationName,
        //        //    ipPacket.SourceAddress, ipPacket.TransportLayerPacket.SourcePort, ipPacket.DestinationAddress,
        //        //    ipPacket.TransportLayerPacket.DestinationPort, ipPacket.TTL, ipPacket.HeaderLength, ipPacket.MessageLength);

        //        //dataGridView1.Rows.Add(ipPacket.TimeStamp, ipPacket.Direction, ipPacket.Protocol, ipPacket.ApplicationName,
        //        //    ipPacket.SourceAddress, ipPacket.TransportLayerPacket.SourcePort, ipPacket.DestinationAddress,
        //        //    ipPacket.TransportLayerPacket.DestinationPort, ipPacket.TTL, ipPacket.HeaderLength, ipPacket.MessageLength);

        //        dataGridView1.FirstDisplayedScrollingRowIndex = _dataTable.Rows.Count;

        //        //if (_dataTable.Rows.Count - _verticleScrollBarPosition <= dataGridView1.Height / 20)
        //        //{
        //        //    _verticleScrollBarPosition = _dataTable.Rows.Count - 1;
        //        //}

        //        //dataGridView1.Scroll -= dataGridView1_Scroll;
        //        //dataGridView1.FirstDisplayedScrollingRowIndex = _verticleScrollBarPosition;
        //        //dataGridView1.Scroll += dataGridView1_Scroll;
        //    }

        //    dataGridView1.ResumeLayout();
        //}

        private void InitTimeGraph()
        {
            // Adaugarea optiunilor in combo box
            comboBox1.Items.AddRange(new string[] { ChartContent.Protocol.ToString(), ChartContent.ApplicationName.ToString(), ChartContent.Port.ToString() });
            comboBox1.SelectedIndex = 0;

            comboBox3.Items.AddRange(new string[] { "Number of packets", "Size of packets" });
            comboBox3.SelectedIndex = 0;

            // Initializarea timpului de repaus al thread-ului pentru graficul in timp
            GRAPH_THREAD_SLEEP_TIME = 5000;
        }

        private void InitPiechart()
        {
            // Adaugarea optiunilor in combo box
            comboBox2.Items.AddRange(new string[] { ChartContent.Protocol.ToString(), ChartContent.ApplicationName.ToString(), ChartContent.Port.ToString() });
            comboBox2.SelectedIndex = 0;

            comboBox4.Items.AddRange(new string[] { "Number of packets", "Size of packets" });
            comboBox4.SelectedIndex = 0;

            // Initializarea timpului de repaus al thread-ului pentru pie chart
            PIE_CHART_THREAD_SLEEP_TIME = 2000;
        }

        private void RefreshPieChart()
        {
            while (_isRunning)
            {
                // Invocarea thread-ului interfetei
                if (InvokeRequired)
                {
                    Invoke((Action)(() =>
                    {
                        chart1.Series[0].Points.Clear();

                        var data = dal.GetPiechartData(_pieChartContent, _pieChartData);

                        foreach(var item in data)
                        {
                            chart1.Series[0].Points.AddXY(item.Key, item.Value);
                        }

                        if(_pieChartContent == ChartContent.Port)
                        {
                            chart1.Series[0].IsValueShownAsLabel = false;
                            chart1.Series[0].IsVisibleInLegend = false;
                        }
                        else
                        {
                            chart1.Series[0].IsValueShownAsLabel = true;
                            chart1.Series[0].IsVisibleInLegend = true;
                        }
                                                
                    }));
                }
                Thread.Sleep(PIE_CHART_THREAD_SLEEP_TIME);
            }
        }

        private void RefreshTimeChart()
        {
            while (_isRunning)
            {
                // Invocarea thread-ului interfetei
                if (InvokeRequired)
                {
                    Invoke((Action)(() =>
                    {
                        chart2.DataSource = null;
                        chart2.Series.Clear();

                        Dictionary<string, string> dates = new Dictionary<string, string>();                        
                        var result = dal.GetTimeChartData(_timeChartContent, _timeChartData, _dataTable, out dates);

                        if(result == null)
                        {
                            return;
                        }

                        for(int i = 0; i < result.Count; i++)
                        {
                            chart2.Series.Add(result[i][0].ToString());
                            chart2.Series[i].ChartType = SeriesChartType.Spline;

                            for(int j = 1; j < result[i].Length; j++)
                            {
                                var key = string.Empty;

                                if(dates.Count > j - 1)
                                {
                                    key = dates.ElementAt(j - 1).Key;
                                }

                                if (result[i][j] == null || result[i][j].ToString().Equals(string.Empty))
                                {
                                    result[i][j] = "0";
                                }

                                if (_timeChartData == ContentData.Sum)
                                {
                                    chart2.Series[i].Points.AddXY(key, int.Parse(result[i][j].ToString()));
                                }
                                else
                                {
                                    chart2.Series[i].Points.AddXY(key, result[i][j]);
                                }
                            }
                        }

                    }));

                    Thread.Sleep(GRAPH_THREAD_SLEEP_TIME);
                }
            }
        }

        private void RefreshDataGrid()
        {
            while (_isRunning)
            {
                // Invocarea thread-ului interfetei
                if (InvokeRequired)
                {
                    Invoke((Action)(() =>
                    {
                        _dataTable = dal.GetIPPackets(_dataTable);

                        //dataGridView1.FirstDisplayedScrollingRowIndex = _dataTable.Rows.Count;                       

                        //if (_dataTable.Rows.Count > 0 && _dataTable.Rows.Count - _verticleScrollBarPosition <= dataGridView1.Height / 20)
                        //{
                        //    _verticleScrollBarPosition = _dataTable.Rows.Count - 1;
                        //}

                        //dataGridView1.Scroll -= dataGridView1_Scroll;
                        //dataGridView1.FirstDisplayedScrollingRowIndex = _verticleScrollBarPosition;
                        //dataGridView1.Scroll += dataGridView1_Scroll;

                    }));
                }

                Thread.Sleep(DATA_GRID_THREAD_SLEEP_TIME);
            }            
        }

        // Initializare tabela despre informatii ale pachetelor
        private void InitGrid()
        {
            _dataTable = new DataTable();

            DataColumn col = new DataColumn();
            col.Caption = "ID";
            col.ColumnName = "ID";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.Caption = "No.";
            col.ColumnName = "No.";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.Caption = "Date";
            col.ColumnName = "Date";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.Caption = "Time stamp";
            col.ColumnName = "Time stamp";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.Caption = "Direction";
            col.ColumnName = "Direction";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "Protocol";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "Application";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "Source Address";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "Source Port";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "Destination Address";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "Destination Port";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "TTL";
            _dataTable.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "Packet Length";
            _dataTable.Columns.Add(col);
            
            dataGridView1.DataSource = _dataTable;
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.SetDoubleBuffered(true);

            DATA_GRID_THREAD_SLEEP_TIME = 2000;
        }

        private void InitMonitor()
        {
            _packetList = new List<IPPacket>();

            _socketMonitor = new SocketMonitor(_ipAddress, dal);

            // Initializarea unui nou thread pentru monitorizare
            _bworker = new BackgroundWorker();
            _bworker.DoWork += (s, ev) =>
            {
                _socketMonitor.StartMonitoring();
            };
        }

        // Gasire adresa locala
        private void InitHost()
        {
            string hostName = Dns.GetHostName();
            var ipAddressArray = Dns.GetHostAddresses(hostName);
            var ipAddressList = ipAddressArray.ToList();
            _ipAddress = ipAddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();

            label1.Text = "Host IPv4 Address:";
            label3.Text = _ipAddress.ToString();
        }        

        private void startButton_Click(object sender, EventArgs e)
        {
            dal.resetDatabase();
            _dataTable.Clear();

            _isRunning = true;
            _pieChartThread = new Thread(RefreshPieChart);
            _pieChartThread.Start();

            _graphThread = new Thread(RefreshTimeChart);
            _graphThread.Start();

            _dataGridThread = new Thread(RefreshDataGrid);
            _dataGridThread.Start();

            startButton.Enabled = false;
            stopButton.Enabled = true;
            _bworker.RunWorkerAsync();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _isRunning = false;
            startButton.Enabled = true;
            stopButton.Enabled = false;
            _socketMonitor.IsRunning = false;
        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            _stopRefreshing = true;
            _verticleScrollBarPosition = dataGridView1.FirstDisplayedScrollingRowIndex;
            _stopRefreshing = false;
        }

        // Metoda pentru desenarea celulelor dinamica
        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= _dataTable.Rows.Count)
            {
                return;
            }

            if (e.ColumnIndex >= _dataTable.Columns.Count)
            {
                return;
            }

            e.Value = _dataTable.Rows[e.RowIndex][e.ColumnIndex];
        }

        // Obiect continut pie chart
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                _pieChartContent = ChartContent.Protocol;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                _pieChartContent = ChartContent.ApplicationName;
            }
            else
            {
                _pieChartContent = ChartContent.Port;
            }
        }

        // Continut pie chart
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                _timeChartContent = ChartContent.Protocol;
            }
            else if (comboBox2.SelectedIndex == 1)
            {
                _timeChartContent = ChartContent.ApplicationName;
            }
            else
            {
                _timeChartContent = ChartContent.Port;
            }
        }

        // Deschiderea meniului de optiuni
        private void button1_Click(object sender, EventArgs e)
        {
            stopButton_Click(sender, e);

            var options = new Options(this);
            options.Show();
        }

        // Continut time chart
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox3.SelectedIndex == 0)
            {
                _pieChartData = ContentData.Count;
            }
            else
            {
                _pieChartData = ContentData.Sum;
            }
        }

        // Obiect time chart
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == 0)
            {
                _timeChartData = ContentData.Count;
            }
            else
            {
                _timeChartData = ContentData.Sum;
            }
        }
    }
}
