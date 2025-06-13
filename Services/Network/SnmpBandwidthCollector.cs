using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Net;

namespace ConnTracer.Services.Network
{
    public class SnmpBandwidthCollector
    {
        public async Task<Dictionary<string, long>> GetInterfaceBandwidthAsync(string ipAddress, string community = "public")
        {
            return await Task.Run(async () =>
            {
                var result = new Dictionary<string, long>();
                try
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), 161);

                    // OIDs für ifDescr, ifInOctets, ifOutOctets
                    var oids = new List<Variable>
                    {
                        new Variable(new ObjectIdentifier("1.3.6.1.2.1.2.2.1.2")),  // ifDescr
                        new Variable(new ObjectIdentifier("1.3.6.1.2.1.2.2.1.10")), // ifInOctets
                        new Variable(new ObjectIdentifier("1.3.6.1.2.1.2.2.1.16"))  // ifOutOctets
                    };

                    // SNMPv2c-Get
                    IList<Variable> response = await Messenger.GetAsync(
                        VersionCode.V2,
                        endpoint,
                        new OctetString(community),
                        oids
                    );

                    foreach (var vb in response)
                    {
                        // Wert als long, falls möglich
                        if (vb.Data is Integer32 int32)
                            result[vb.Id.ToString()] = int32.ToInt32();
                        else if (vb.Data is Counter32 c32)
                            result[vb.Id.ToString()] = c32.ToUInt32();
                        else if (vb.Data is Gauge32 g32)
                            result[vb.Id.ToString()] = g32.ToUInt32();
                        else if (long.TryParse(vb.Data.ToString(), out long val))
                            result[vb.Id.ToString()] = val;
                        else
                            result[vb.Id.ToString()] = 0;
                    }
                }
                catch
                {
                    // Fehlerbehandlung nach Bedarf
                }
                return result;
            });
        }
    }
}