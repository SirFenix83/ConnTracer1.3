#nullable enable
using ConnTracer.Models; // Fügen Sie den Namespace hinzu, der UdpPacketEventArgs enthält.

public class UDPMonitor
{
    public event EventHandler<UdpPacketEventArgs>? UdpPacketDetected;

    protected void OnUdpPacketDetected(UdpPacketEventArgs e)
    {
        UdpPacketDetected?.Invoke(this, e);
    }

    public void Start()
    {
        // Implementierung des Start-Logik für UDP-Monitoring
        // Beispiel: Initialisierung von Netzwerkressourcen oder Start eines Überwachungs-Threads
        Console.WriteLine("UDP-Monitor gestartet.");
    }
}
#nullable disable