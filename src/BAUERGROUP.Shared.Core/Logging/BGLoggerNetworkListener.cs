using BAUERGROUP.Shared.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BAUERGROUP.Shared.Core.Logging
{
    [Obsolete("Superseded by BGLogger.Configuration.AddLiveSink / BGLogViewBuffer (in-process, no UDP socket). " +
              "This type will be removed in a future major version.")]
    public class BGLoggerNetworkListener : PropertyChangedBase, IDisposable
    {
        private CancellationTokenSource _cts;

        public BGLoggerNetworkListener(Boolean automaticEnableSenderOnLocalHost = false, IPAddress? address = null, UInt16 port = 0)
        {
            _cts = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    UdpClient client = new UdpClient(port == 0 ? BGLoggerConfiguration.DefaultNetworkPort : port);
                    IPEndPoint endpoint = new IPEndPoint(address == null ? IPAddress.Loopback : address, port == 0 ? BGLoggerConfiguration.DefaultNLogViewerPort : port);

                    while (!_cts.IsCancellationRequested)
                    {
                        byte[] data = client.Receive(ref endpoint);
                        OnPropertyChanged(String.Format("{0}{1}", Encoding.UTF8.GetString(data, 0, data.Length), Environment.NewLine));
                    }

                    client.Close();
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    BGLogger.Error(ex, $"BGLoggerNetworkListener() -> Exception with Reason: '{ex.Message}'.");
                }
            }, _cts.Token);

            if (automaticEnableSenderOnLocalHost == true)
                BGLogger.Configuration.Network = true;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
