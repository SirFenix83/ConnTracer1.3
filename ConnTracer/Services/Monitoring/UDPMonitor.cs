#nullable enable
using ConnTracer.Models; // F�gen Sie den Namespace hinzu, der UdpPacketEventArgs enth�lt.

public class UDPMonitor
{
    public event EventHandler<UdpPacketEventArgs>? UdpPacketDetected;

    protected void OnUdpPacketDetected(UdpPacketEventArgs e)
    {
        UdpPacketDetected?.Invoke(this, e);
    }

    public void Start()
    {
        // Implementierung des Start-Logik f�r UDP-Monitoring
        // Beispiel: Initialisierung von Netzwerkressourcen oder Start eines �berwachungs-Threads
        Console.WriteLine("UDP-Monitor gestartet.");
    }
}
#nullable disable