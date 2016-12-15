using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication.Utils;

namespace WindowsFormsApplication
{
    public partial class Options : Form
    {
        private MainForm _mainForm;

        public Options(MainForm mainform)
        {
            InitializeComponent();

            _mainForm = mainform;

            textBox1.Text = _mainForm.DATA_GRID_THREAD_SLEEP_TIME.ToString();
            textBox2.Text = _mainForm.PIE_CHART_THREAD_SLEEP_TIME.ToString();
            textBox3.Text = _mainForm.GRAPH_THREAD_SLEEP_TIME.ToString();

            StringBuilder sb = new StringBuilder();
            foreach(var item in Settings.BlockedPorts)
            {
                sb.AppendFormat("{0};", item);
            }
            richTextBox1.Text = sb.ToString();
            checkBox1.Checked = Settings.BlockPacketsWithNoProcess;
            checkBox2.Checked = Settings.LogToFile;

            label1.Focus();
        }

        // Evenimentul butonului cancel
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Evenimentul butonului de salvare
        private void button2_Click(object sender, EventArgs e)
        {
            _mainForm.DATA_GRID_THREAD_SLEEP_TIME = int.Parse(textBox1.Text);
            _mainForm.PIE_CHART_THREAD_SLEEP_TIME = int.Parse(textBox2.Text);
            _mainForm.GRAPH_THREAD_SLEEP_TIME = int.Parse(textBox3.Text);

            var ports = richTextBox1.Text.Split(';');
            for(int i = 0; i < ports.Length; i++)
            {
                if(!Settings.BlockedPorts.Contains(ports[i].Trim(' ').TrimEnd(';')) && ports[i] != string.Empty)
                {
                    Settings.BlockedPorts.Add(ports[i].Trim(' '));
                }                
            }

            Settings.BlockPacketsWithNoProcess = checkBox1.Checked;
            Settings.LogToFile = checkBox2.Checked;

            Close();
        }
    }
}
