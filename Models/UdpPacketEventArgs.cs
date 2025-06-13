namespace ConnTracer.Models
{
    public class UdpPacketEventArgs : EventArgs
    {
        public string PacketData { get; set; }

        public UdpPacketEventArgs(string packetData)
        {
            PacketData = packetData;
        }
    }
}