using System.Text; // Namespace hinzufügen

namespace ConnTracer
{
    public partial class MainForm
    {
        #nullable enable
        private async void BandwidthUpdateTimer_Tick(object? sender, EventArgs e)
        {
            await bandwidthMonitor.MeasureAsync();
            bandwidthMonitorDataReady = true;
            UpdateBandwidthMonitorListView();

            if (!bandwidthAnalyzerDataReady)
            {
                await UpdateBandwidthAnalyzerAsync();
            }

            if (!bandwidthTesterDataReady)
            {
                await UpdateBandwidthTesterAsync();
            }
        }
        #nullable disable

        private void BandwidthMonitor_OnBandwidthUpdate(string analysis)
        {
            Color color = analysis.Contains("Warnung") ? Color.Red :
                          analysis.Contains("Hohe") ? Color.Orange :
                          analysis.Contains("Mittlere") ? Color.Yellow : Color.LightGreen;
            if (dgvStatusOverview?.InvokeRequired == true)
                dgvStatusOverview.Invoke(() => SetStatus("Monitor", analysis, color));
            else
                SetStatus("Monitor", analysis, color);

            if (lblMonitorStatus.InvokeRequired)
                lblMonitorStatus.Invoke(() => lblMonitorStatus.Text = $"Status: {analysis}");
            else
                lblMonitorStatus.Text = $"Status: {analysis}";

            if (lvBandwidthOverview.InvokeRequired)
                lvBandwidthOverview.Invoke(UpdateBandwidthMonitorListView);
            else
                UpdateBandwidthMonitorListView();
        }

        #nullable enable
        private void BtnShowBandwidthOverview_Click(object? sender, EventArgs e) => ShowPanel(pnlBandwidthOverview!);
        private void BtnShowNetworkMonitor_Click(object? sender, EventArgs e) => ShowPanel(pnlNetworkMonitor!);
        private void BtnShowTcpConnections_Click(object? sender, EventArgs e) => ShowPanel(pnlTcpConnections!);
        private void BtnShowBottleneckAnalysis_Click(object? sender, EventArgs e) => ShowPanel(pnlBottleneckAnalysis!);
        private void BtnShowDeviceScanner_Click(object? sender, EventArgs e) => ShowPanel(pnlDeviceScanner!);
        private void BtnSaveLogs_Click(object? sender, EventArgs e)
        {
            try
            {
                string logContent = ExportLogs();

                SaveFileDialog sfd = new SaveFileDialog()
                {
                    Filter = "Textdatei|*.txt",
                    FileName = $"ConnTracer_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    Title = "Logs speichern"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, logContent, Encoding.UTF8);
                    MessageBox.Show("Logs wurden erfolgreich gespeichert.", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der Logs:\n{ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #nullable disable

        #nullable enable
        private void BtnTaskManager_Click(object? sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("taskmgr.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Starten des Task-Managers:\n{ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #nullable disable

        #nullable enable
        private void BtnShowSecurityOverview_Click(object? sender, EventArgs e) => ShowPanel(pnlSecurityOverview!);
        #nullable disable

        #nullable enable
        private void NetworkMonitorTimer_Tick(object? sender, EventArgs e) => _ = UpdateNetworkMonitorAsync();
        #nullable disable
    }
}