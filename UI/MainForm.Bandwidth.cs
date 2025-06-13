namespace ConnTracer
{
    public partial class MainForm
    {
        private void UpdateBandwidthMonitorListView()
        {
            for (int i = lvBandwidthOverview.Items.Count - 1; i >= 0; i--)
            {
                if (lvBandwidthOverview.Items[i].SubItems[0].Text == "Monitor")
                    lvBandwidthOverview.Items.RemoveAt(i);
            }

            if (!bandwidthMonitorDataReady) return;

            var stats = bandwidthMonitor.GetCurrentStats();
            foreach (var kvp in stats)
            {
                lvBandwidthOverview.Items.Add(new ListViewItem(new[]
                {
                    "Monitor", kvp.Key, kvp.Value + " Kbps"
                }));
            }
        }

        private async Task UpdateBandwidthAnalyzerAsync()
        {
            for (int i = lvBandwidthOverview.Items.Count - 1; i >= 0; i--)
            {
                if (lvBandwidthOverview.Items[i].SubItems[0].Text == "Analyzer")
                    lvBandwidthOverview.Items.RemoveAt(i);
            }

            try
            {
                var analyzerData = await GetCurrentBandwidthUsageAsync();
                bandwidthAnalyzerDataReady = true;

                string analysis = bandwidthAnalyzer.DetectBottleneck(analyzerData);
                Color color = analysis.Contains("Warnung") ? Color.Red :
                              analysis.Contains("Hohe") ? Color.Orange :
                              analysis.Contains("Mittlere") ? Color.Yellow : Color.LightGreen;
                if (dgvStatusOverview?.InvokeRequired == true)
                    dgvStatusOverview.Invoke(() => SetStatus("Analyzer", analysis, color));
                else
                    SetStatus("Analyzer", analysis, color);

                if (lblAnalyzerStatus.InvokeRequired)
                    lblAnalyzerStatus.Invoke(() => lblAnalyzerStatus.Text = $"Status: {analysis}");
                else
                    lblAnalyzerStatus.Text = $"Status: {analysis}";

                foreach (var kvp in analyzerData)
                {
                    lvBandwidthOverview.Items.Add(new ListViewItem(new[]
                    {
                        "Analyzer", kvp.Key, kvp.Value + " Kbps"
                    }));
                }
            }
            catch (Exception ex)
            {
                lvBandwidthOverview.Items.Add(new ListViewItem(new[]
                {
                    "Fehler", "Analyzer", ex.Message
                }));
                if (lblAnalyzerStatus.InvokeRequired)
                    lblAnalyzerStatus.Invoke(() => lblAnalyzerStatus.Text = $"Status: Fehler: {ex.Message}");
                else
                    lblAnalyzerStatus.Text = $"Status: Fehler: {ex.Message}";
            }
        }

        private async Task UpdateBandwidthTesterAsync()
        {
            for (int i = lvBandwidthOverview.Items.Count - 1; i >= 0; i--)
            {
                if (lvBandwidthOverview.Items[i].SubItems[0].Text == "Tester")
                    lvBandwidthOverview.Items.RemoveAt(i);
            }

            try
            {
                string testHost = "speedtest.net";
                var results = await bandwidthTester.RunTestAsync(testHost, 443, 5);

                bandwidthTesterDataReady = true;

                string downloadText = results.DownloadResult.Success
                    ? $"{results.DownloadResult.SpeedMbps:F2} Mbps"
                    : $"Fehler: {results.DownloadResult.Message}";

                string uploadText = results.UploadResult.Success
                    ? $"{results.UploadResult.SpeedMbps:F2} Mbps"
                    : $"Fehler: {results.UploadResult.Message}";

                lvBandwidthOverview.Items.Add(new ListViewItem(new[]
                {
                    "Tester", "Download", downloadText
                }));
                lvBandwidthOverview.Items.Add(new ListViewItem(new[]
                {
                    "Tester", "Upload", uploadText
                }));

                string testerStatus = results.DownloadResult.Success && results.UploadResult.Success
                    ? "Test erfolgreich."
                    : $"Fehler: {results.DownloadResult.Message} {results.UploadResult.Message}";
                Color testerColor = results.DownloadResult.Success && results.UploadResult.Success
                    ? Color.LightGreen
                    : Color.Red;
                if (dgvStatusOverview?.InvokeRequired == true)
                    dgvStatusOverview.Invoke(() => SetStatus("Tester", testerStatus, testerColor));
                else
                    SetStatus("Tester", testerStatus, testerColor);

                if (lblTesterStatus.InvokeRequired)
                    lblTesterStatus.Invoke(() => lblTesterStatus.Text = $"Status: {testerStatus}");
                else
                    lblTesterStatus.Text = $"Status: {testerStatus}";
            }
            catch (Exception ex)
            {
                lvBandwidthOverview.Items.Add(new ListViewItem(new[]
                {
                    "Fehler", "Tester", ex.Message
                }));
                if (lblTesterStatus.InvokeRequired)
                    lblTesterStatus.Invoke(() => lblTesterStatus.Text = $"Status: Fehler: {ex.Message}");
                else
                    lblTesterStatus.Text = $"Status: Fehler: {ex.Message}";
            }
        }

        private async Task<Dictionary<string, long>> GetCurrentBandwidthUsageAsync()
        {
            var before = bandwidthAnalyzer.GetCurrentBytes();
            await Task.Delay(5000);
            var after = bandwidthAnalyzer.GetCurrentBytes();

            return bandwidthAnalyzer.CalculateBandwidthUsage(before, after, 5.0);
        }
    }
}