#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConnTracer.Services.Network;
using ConnTracer.Services.Core;
using ConnTracer.Services.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;

namespace ConnTracer
{
    public partial class MainForm : Form
    {
        private readonly DeviceScanner deviceScanner = new();
        private readonly NetworkScanner networkScanner = new();
        private readonly BandwidthAnalyzer bandwidthAnalyzer = new();
        private readonly BandwidthMonitor bandwidthMonitor = new();
        private readonly BandwidthTester bandwidthTester = new();
        private readonly System.Windows.Forms.Timer networkMonitorTimer = new() { Interval = 5_000 };
        private readonly System.Windows.Forms.Timer bandwidthUpdateTimer = new() { Interval = 1_000 };

        private bool bandwidthMonitorDataReady;
        private bool bandwidthAnalyzerDataReady;
        private bool bandwidthTesterDataReady;

        private DataGridView? dgvStatusOverview;
        private Panel? mainPanel;

        private UDPMonitor udpMonitor = new();
        private ICMPMonitor icmpMonitor = new();
        private PortscanDetector portscanDetector = new();
        private TrafficAnomalyDetector trafficAnomalyDetector = new();

        private Panel? pnlSecurityOverview;
        private ListView? lvSecurityOverview;

        // Beispiel für UDPMonitor
        public event EventHandler<UdpPacketEventArgs>? UdpPacketDetected;

        public MainForm()
        {
            InitializeComponent();

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(mainPanel);

            lvDeviceScanner.View = View.Details;
            lvDeviceScanner.FullRowSelect = true;
            lvDeviceScanner.Columns.Clear();
            lvDeviceScanner.Columns.Add("Device Name", 150);
            lvDeviceScanner.Columns.Add("IP Address", 120);
            lvDeviceScanner.Columns.Add("MAC Address", 150);
            lvDeviceScanner.Columns.Add("Manufacturer", 150);
            lvDeviceScanner.Columns.Add("Status", 100);

            lvBandwidthOverview.View = View.Details;
            lvBandwidthOverview.FullRowSelect = true;
            lvBandwidthOverview.Columns.Clear();
            lvBandwidthOverview.Columns.Add("Kategorie", 200);
            lvBandwidthOverview.Columns.Add("Schnittstelle / Gerät", 300);
            lvBandwidthOverview.Columns.Add("Wert", 300);

            lvTcpConnections.View = View.Details;
            lvTcpConnections.FullRowSelect = true;
            lvTcpConnections.Columns.Clear();
            lvTcpConnections.Columns.Add("Prozess", 200);
            lvTcpConnections.Columns.Add("Lokale Adresse", 150);
            lvTcpConnections.Columns.Add("Remote Adresse", 150);
            lvTcpConnections.Columns.Add("Status", 100);

            lvNetworkMonitor.View = View.Details;
            lvNetworkMonitor.FullRowSelect = true;
            lvNetworkMonitor.Columns.Clear();
            lvNetworkMonitor.Columns.Add("Gerät", 200);
            lvNetworkMonitor.Columns.Add("IP Adresse", 150);
            lvNetworkMonitor.Columns.Add("MAC Adresse", 150);

            lvBottleneckAnalysis.View = View.Details;
            lvBottleneckAnalysis.FullRowSelect = true;
            lvBottleneckAnalysis.Columns.Clear();
            lvBottleneckAnalysis.Columns.Add("Kategorie", 200);
            lvBottleneckAnalysis.Columns.Add("Ergebnis", 600);

            pnlSecurityOverview = new Panel { Dock = DockStyle.Fill };
            lvSecurityOverview = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            lvSecurityOverview.Columns.Add("Zeit", 140);
            lvSecurityOverview.Columns.Add("Typ", 120);
            lvSecurityOverview.Columns.Add("Beschreibung", 600);
            pnlSecurityOverview.Controls.Add(lvSecurityOverview);

            btnShowBandwidthOverview.Click += BtnShowBandwidthOverview_Click;
            btnShowTcpConnections.Click += BtnShowTcpConnections_Click;
            btnShowNetworkMonitor.Click += BtnShowNetworkMonitor_Click;
            btnShowBottleneckAnalysis.Click += BtnShowBottleneckAnalysis_Click;
            btnShowDeviceScanner.Click += BtnShowDeviceScanner_Click;
            btnSaveLogs.Click += BtnSaveLogs_Click;
            btnTaskManager.Click += BtnTaskManager_Click;

            btnShowBottleneckAnalysis.Click += async (s, e) =>
            {
                ShowPanel(pnlBottleneckAnalysis!);
                await UpdateBottleneckAnalysisAsync();
            };

            networkMonitorTimer.Tick += NetworkMonitorTimer_Tick;
            bandwidthUpdateTimer.Tick += BandwidthUpdateTimer_Tick;

            bandwidthMonitor.OnBandwidthUpdate += BandwidthMonitor_OnBandwidthUpdate;
            bandwidthMonitor.Start();

            dgvStatusOverview = new DataGridView
            {
                Location = new Point(900, 70),
                Size = new Size(270, 120),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                ColumnHeadersVisible = false,
                ColumnCount = 2,
                EnableHeadersVisualStyles = false
            };
            dgvStatusOverview.Columns[0].Name = "Komponente";
            dgvStatusOverview.Columns[0].Width = 120;
            dgvStatusOverview.Columns[1].Name = "Status";
            dgvStatusOverview.Columns[1].Width = 120;
            dgvStatusOverview.Rows.Add("Monitor", "Warten...");
            dgvStatusOverview.Rows.Add("Analyzer", "Warten...");
            dgvStatusOverview.Rows.Add("Tester", "Warten...");
            dgvStatusOverview.ClearSelection();
            dgvStatusOverview.DefaultCellStyle.SelectionBackColor = dgvStatusOverview.DefaultCellStyle.BackColor;
            dgvStatusOverview.DefaultCellStyle.SelectionForeColor = dgvStatusOverview.DefaultCellStyle.ForeColor;
            pnlBandwidthOverview.Controls.Add(dgvStatusOverview);

            pnlBandwidthOverview.Visible = true;
            lvBandwidthOverview.Visible = true;
            ShowPanel(pnlBandwidthOverview);

            lblMonitorStatus.Text = "Status: Warten auf erste Daten...";
            lblAnalyzerStatus.Text = "Status: Warten auf erste Daten...";
            lblTesterStatus.Text = "Status: Warten auf erste Daten...";

            MinimumSize = new Size(800, 600);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var buttonPanelTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            buttonPanelTop.Controls.Add(btnShowDeviceScanner);
            buttonPanelTop.Controls.Add(btnShowBandwidthOverview);
            buttonPanelTop.Controls.Add(btnShowTcpConnections);
            buttonPanelTop.Controls.Add(btnShowNetworkMonitor);
            buttonPanelTop.Controls.Add(btnShowBottleneckAnalysis);

            var btnShowSecurityOverview = new Button
            {
                Text = "Security-Events",
                AutoSize = true
            };
            btnShowSecurityOverview.Click += BtnShowSecurityOverview_Click;
            buttonPanelTop.Controls.Add(btnShowSecurityOverview);

            var outputPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            outputPanel.Controls.Add(pnlBandwidthOverview);
            outputPanel.Controls.Add(pnlTcpConnections);
            outputPanel.Controls.Add(pnlNetworkMonitor);
            outputPanel.Controls.Add(pnlBottleneckAnalysis);
            outputPanel.Controls.Add(pnlDeviceScanner);
            outputPanel.Controls.Add(pnlSecurityOverview);

            var buttonPanelBottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            buttonPanelBottom.Controls.Add(btnSaveLogs);
            buttonPanelBottom.Controls.Add(btnTaskManager);

            pnlBandwidthOverview.Dock = DockStyle.Fill;
            pnlTcpConnections.Dock = DockStyle.Fill;
            pnlNetworkMonitor.Dock = DockStyle.Fill;
            pnlBottleneckAnalysis.Dock = DockStyle.Fill;
            pnlDeviceScanner.Dock = DockStyle.Fill;
            pnlSecurityOverview.Dock = DockStyle.Fill;

            mainLayout.Controls.Add(buttonPanelTop, 0, 0);
            mainLayout.Controls.Add(outputPanel, 0, 1);
            mainLayout.Controls.Add(buttonPanelBottom, 0, 2);

            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(mainLayout);

            mainPanel.Padding = new Padding(0, 10, 0, 0);

            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
            bandwidthUpdateTimer.Start();

            udpMonitor.UdpPacketDetected += (s, e) =>
                AddSecurityEvent("UDP", DateTime.Now, e.PacketData);
            icmpMonitor.IcmpPacketDetected += (s, e) =>
                AddSecurityEvent("ICMP", e.Timestamp, e.Description);
            portscanDetector.PortscanDetected += (s, e) =>
                AddSecurityEvent("Portscan", e.Timestamp, e.Description);
            trafficAnomalyDetector.AnomalyDetected += (s, e) =>
                AddSecurityEvent("Anomalie", e.Timestamp, e.Description);

            udpMonitor.Start();
            icmpMonitor.Start();
            portscanDetector.Start();
            trafficAnomalyDetector.Start();

            ShowPanel(pnlDeviceScanner!);
            _ = LoadDevicesAsync();
        }
    }
}
