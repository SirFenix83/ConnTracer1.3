using System.Text;

namespace ConnTracer
{
    public partial class MainForm
    {
        private string ExportLogs()
        {
            StringBuilder sb = new();

            sb.AppendLine("=== TCP Connections ===");
            foreach (ListViewItem item in lvTcpConnections.Items)
            {
                sb.AppendLine(string.Join(" | ", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)));
            }

            sb.AppendLine("\n=== Network Monitor ===");
            foreach (ListViewItem item in lvNetworkMonitor.Items)
            {
                sb.AppendLine(string.Join(" | ", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)));
            }

            sb.AppendLine("\n=== Bottleneck Analysis ===");
            foreach (ListViewItem item in lvBottleneckAnalysis.Items)
            {
                sb.AppendLine(string.Join(" | ", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)));
            }

            sb.AppendLine("\n=== Security Events ===");
            if (lvSecurityOverview != null)
            {
                foreach (ListViewItem item in lvSecurityOverview.Items)
                {
                    sb.AppendLine(string.Join(" | ", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)));
                }
            }

            return sb.ToString();
        }

#nullable enable
        private void AddSecurityEvent(string typ, DateTime zeit, string beschreibung, string? ipAdresse = null)
        {
            var values = new List<string>
            {
                zeit.ToString("yyyy-MM-dd HH:mm:ss"),
                typ,
                beschreibung
            };
            if (ipAdresse != null)
                values.Add(ipAdresse);

            if (lvSecurityOverview?.InvokeRequired == true)
            {
                lvSecurityOverview.Invoke(() =>
                    lvSecurityOverview.Items.Insert(0, new ListViewItem(values.ToArray())));
            }
            else
            {
                lvSecurityOverview?.Items.Insert(0, new ListViewItem(values.ToArray()));
            }
        }
#nullable disable

        private void SetStatus(string component, string status, Color color)
        {
            if (dgvStatusOverview == null) return;

            foreach (DataGridViewRow row in dgvStatusOverview.Rows)
            {
                if (row.Cells[0].Value?.ToString() == component)
                {
                    row.Cells[1].Value = status;
                    row.Cells[1].Style.BackColor = color;
                    break;
                }
            }
        }

#nullable enable
        private void ShowPanel(Panel? panelToShow)
        {
            if (panelToShow == null) return;
            pnlBandwidthOverview.Visible = false;
            pnlTcpConnections.Visible = false;
            pnlNetworkMonitor.Visible = false;
            pnlBottleneckAnalysis.Visible = false;
            pnlDeviceScanner.Visible = false;
            if (pnlSecurityOverview != null)
                pnlSecurityOverview.Visible = false;

            panelToShow.Visible = true;

            if (panelToShow == pnlNetworkMonitor)
            {
                networkMonitorTimer.Start();
                _ = UpdateNetworkMonitorAsync();
            }
            else
            {
                networkMonitorTimer.Stop();
            }

            if (panelToShow == pnlTcpConnections)
            {
                _ = UpdateTcpConnectionsAsync();
            }
        }
#nullable disable
    }
}