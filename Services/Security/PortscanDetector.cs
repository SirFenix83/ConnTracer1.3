namespace ConnTracer.Services.Security
{
    public class PortscanDetector
    {
        public event EventHandler<PortscanEventArgs> PortscanDetected;

        public void Start()
        {
            // Logik zum Starten des Portscan-Detektors
        }

        protected virtual void OnPortscanDetected(PortscanEventArgs e)
        {
            PortscanDetected?.Invoke(this, e);
        }
    }

    public class PortscanEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }
}