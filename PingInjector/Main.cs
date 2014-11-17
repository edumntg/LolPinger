//LolPing created by Eduardo Montilva edumntg@gmail.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.NetworkInformation;

namespace PingInjector
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow(); 

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        int SelectedServerId = 0;
        string[] servers = { "LAN", "LAS", "NA", "OCE", "EUW", "EUNE", "BR", "KR", "TR" };
        string url = "leagueoflegends.com";
        string SelectedServerUrl = "";

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
            timer1.Start();
            comboBox1.Items.AddRange(servers);
            comboBox1.SelectedIndex = 0;
            PingTimer.Start();
            Process[] process = Process.GetProcessesByName("LolClient");
            if(process.Length > 0)
            {
                Process launcher = process[0];
                Rect LauncherRect = new Rect();
                GetWindowRect(launcher.MainWindowHandle, ref LauncherRect);


                Point p = new Point((LauncherRect.Right) - (this.Width), (LauncherRect.Top - this.Height));
                this.Location = p;
                this.Focus();

            }

        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {
            Process[] process = Process.GetProcessesByName("LolClient"); //name of the launcher LoLClient.exe
            if (process.Length > 0)
            {
                Process launcher = process[0];
                Rect LauncherRect = new Rect();
                GetWindowRect(launcher.MainWindowHandle, ref LauncherRect);

                if (!selecting)
                {
                    Point p = new Point((LauncherRect.Right) - (this.Width), (LauncherRect.Top - this.Height));
                    this.Location = p;
                    this.Focus();
                }

            }
            else
            {
                if(this.Visible)
                    this.Hide();
            }
        }

        Thread t;
        private void PingTimer_Tick(object sender, EventArgs e)
        {
            if (t != null && t.IsAlive)
            {
                t.Abort();
                t = null;
            }

            t = new Thread(new ThreadStart(Pinger));
            t.Start();
        }

        public void Pinger()
        {
            
            try
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    return;
                }
                Ping ping = new Ping();
                PingReply reply = ping.Send(SelectedServerUrl, Convert.ToInt32(timer1.Interval));
                int ms = Convert.ToInt32(reply.RoundtripTime.ToString());
                Color c = Color.Green;
                if (ms >= 200)
                    c = Color.Orange;
                if (ms >= 280)
                    c = Color.Red;

                if(ms > 0) //send only if ping > 0, if not then maybe net is slow (timeout)
                    SendPing(ms.ToString() + " ms", c);
            }
            catch (PingException ex)
            {
                //ignore
                return;
            }
        }

        internal delegate void Delegate(string t, Color c);
        public void SendPing(string t, Color c)
        {
            if (this.label1.InvokeRequired)
            {
                Delegate md = new Delegate(SendPing);
                this.Invoke(md, new object[] { t, c });
            }
            else
            {
                this.label1.Text = t;
                this.label1.ForeColor = c;
            }
            notifyIcon1.Text = "Ping: " + t;
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedServerId = comboBox1.SelectedIndex;
            SelectedServerUrl = servers[SelectedServerId].ToLower() + "." + url;
            selecting = false;
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        bool selecting = false;
        private void comboBox1_Click(object sender, EventArgs e)
        {
            selecting = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            selecting = true;
        }
    }
}
