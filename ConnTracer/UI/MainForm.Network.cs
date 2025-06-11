namespace ConnTracer
{
    public partial class MainForm
    {
        private async Task UpdateNetworkMonitorAsync()
        {
            if (lvNetworkMonitor == null) return;

            var devices = await networkScanner.GetNetworkDataAsync();

            lvNetworkMonitor.BeginInvoke(() =>
            {
                lvNetworkMonitor.Items.Clear();

                if (devices.Count == 0)
                {
                    lvNetworkMonitor.Items.Add(new ListViewItem(new[] { "Keine Geräte gefunden.", "", "" }));
                    return;
                }

                foreach (var device in devices)
                {
                    var item = new ListViewItem(new[]
                    {
                        device.Name,
                        device.IPAddress,
                        device.MacAddress
                    });
                    lvNetworkMonitor.Items.Add(item);
                }
            });
        }

        private async Task UpdateTcpConnectionsAsync()
        {
            if (lvTcpConnections == null) return;

            lvTcpConnections.BeginInvoke(() =>
            {
                lvTcpConnections.Items.Clear();
            });

            try
            {
                var tcpConnections = await networkScanner.GetTcpConnectionsAsync();

                lvTcpConnections.BeginInvoke(() =>
                {
                    foreach (var conn in tcpConnections)
                    {
                        var item = new ListViewItem(new[]
                        {
                            "Unbekannt",
                            $"{conn.LocalEndPoint}",
                            $"{conn.RemoteEndPoint}",
                            conn.State
                        });
                        lvTcpConnections.Items.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                lvTcpConnections.BeginInvoke(() =>
                {
                    lvTcpConnections.Items.Add(new ListViewItem(new[]
                    {
                        "Fehler", "", "", ex.Message
                    }));
                });
            }
        }

        private async Task UpdateBottleneckAnalysisAsync()
        {
            if (lvBottleneckAnalysis == null) return;

            lvBottleneckAnalysis.Items.Clear();

            try
            {
                await bandwidthMonitor.MeasureAsync();
                var stats = bandwidthMonitor.GetCurrentStats();

                string result = bandwidthAnalyzer.DetectBottleneck(stats);
                lvBottleneckAnalysis.Items.Add(new ListViewItem(new[] { "Bandwidth Check", result }));
            }
            catch (Exception ex)
            {
                lvBottleneckAnalysis.Items.Add(new ListViewItem(new[] { "Fehler", ex.Message }));
            }
        }

        private async Task LoadDevicesAsync()
        {
            lvDeviceScanner.Items.Clear();

            var devices = await deviceScanner.ScanLocalNetworkAsync(deviceScanner.GetLocalSubnet());

            if (devices.Count == 0)
            {
                lvDeviceScanner.Items.Add(new ListViewItem(new[] { "Keine Geräte gefunden.", "", "", "", "" }));
                return;
            }

            foreach (var d in devices)
            {
                var item = new ListViewItem(new[]
                {
                    d.Name,
                    d.IP,
                    d.MacAddress,
                    d.Manufacturer,
                    d.Status
                });
                lvDeviceScanner.Items.Add(item);
            }
        }
    }
}