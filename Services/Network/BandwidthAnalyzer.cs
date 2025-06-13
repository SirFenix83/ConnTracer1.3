#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ConnTracer.Services.Network
{
    public class BandwidthAnalyzer
    {
        private readonly SnmpBandwidthCollector snmpCollector = new SnmpBandwidthCollector();

        /// <summary>
        /// Holt aktuelle Bytes (Empfangen + Gesendet) pro Netzwerk-Interface.
        /// Key: Interface-Name, Value: Summe der Bytes (Rx + Tx).
        /// </summary>
        public Dictionary<string, long> GetCurrentBytes()
        {
            var stats = new Dictionary<string, long>();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up
                              && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var nic in interfaces)
            {
                var statsInterface = nic.GetIPv4Statistics();
                long totalBytes = statsInterface.BytesReceived + statsInterface.BytesSent;
                stats[nic.Name] = totalBytes;
            }

            return stats;
        }

        /// <summary>
        /// Berechnet die Bandbreitennutzung in Kbps zwischen zwei Messzeitpunkten.
        /// </summary>
        /// <param name="before">Bytes vor dem Intervall.</param>
        /// <param name="after">Bytes nach dem Intervall.</param>
        /// <param name="seconds">Intervalldauer in Sekunden.</param>
        /// <returns>Dictionary mit Interface-Name und Bandbreite in Kbps.</returns>
        public Dictionary<string, long> CalculateBandwidthUsage(
            Dictionary<string, long> before,
            Dictionary<string, long> after,
            double seconds)
        {
            var usage = new Dictionary<string, long>();

            foreach (var kvp in after)
            {
                string iface = kvp.Key;
                if (before.TryGetValue(iface, out long beforeBytes))
                {
                    long bytesDiff = kvp.Value - beforeBytes;
                    if (bytesDiff < 0) bytesDiff = 0; // Schutz gegen Zähler-Reset
                    long kbps = (long)((bytesDiff * 8) / 1000.0 / seconds); // Kilobits pro Sekunde
                    usage[iface] = kbps;
                }
            }

            return usage;
        }

        /// <summary>
        /// Holt lokale und (optional) SNMP-basierte Bandbreitennutzung.
        /// </summary>
        /// <param name="before">Bytes vor dem Intervall.</param>
        /// <param name="after">Bytes nach dem Intervall.</param>
        /// <param name="seconds">Intervalldauer in Sekunden.</param>
        /// <param name="snmpDevices">Liste von SNMP-Geräten (optional).</param>
        /// <returns>Dictionary mit Interface-Name und Bandbreite in Kbps.</returns>
        public async Task<Dictionary<string, long>> GetCombinedBandwidthUsageAsync(
            Dictionary<string, long> before,
            Dictionary<string, long> after,
            double seconds,
            List<string>? snmpDevices = null)
        {
            var usage = CalculateBandwidthUsage(before, after, seconds);

            if (snmpDevices != null)
            {
                foreach (var ip in snmpDevices)
                {
                    var snmpData = await snmpCollector.GetInterfaceBandwidthAsync(ip);
                    foreach (var kvp in snmpData)
                    {
                        // SNMP-Interfaces als eigene Einträge ergänzen
                        string key = $"SNMP-{ip}-{kvp.Key}";
                        usage[key] = kvp.Value;
                    }
                }
            }
            return usage;
        }

        /// <summary>
        /// Einfacher Engpass-Detektor basierend auf Schwellenwerten.
        /// </summary>
        /// <param name="bandwidthUsage">Bandbreitennutzung pro Interface in Kbps.</param>
        /// <returns>Textuelle Analyse.</returns>
        public string DetectBottleneck(Dictionary<string, long> bandwidthUsage)
        {
            if (bandwidthUsage == null || bandwidthUsage.Count == 0)
                return "Keine Daten für Engpassanalyse verfügbar.";

            long maxUsage = bandwidthUsage.Values.Max();

            // Beispiel-Schwellenwerte (kann man anpassen)
            if (maxUsage > 50000)
                return "Warnung: Sehr hohe Auslastung erkannt! Möglicher Engpass vorhanden.";
            if (maxUsage > 20000)
                return "Hohe Auslastung, bitte überprüfen Sie Ihre Verbindung.";
            if (maxUsage > 5000)
                return "Mittlere Auslastung.";

            return "Geringe Auslastung, alles im grünen Bereich.";
        }
    }
}