using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Tftp.Net.Channel
{
    class UdpChannel : ITransferChannel
    {
        public event TftpCommandHandler OnCommandReceived;
        public event TftpChannelErrorHandler OnError;

        private IPEndPoint endpoint;
        private UdpClient client;
        private readonly CommandSerializer serializer = new CommandSerializer();
        private readonly CommandParser parser = new CommandParser();

        private readonly CancellationTokenSource cts;
        private Task _listener = null;

        public UdpChannel(UdpClient client)
        {
            this.client = client;
            this.endpoint = null;
            this.cts = new CancellationTokenSource();
        }

        public void Open()
        {
            if (client == null)
                throw new ObjectDisposedException("UdpChannel");
            if (_listener != null)
                throw new Exception("Socket already open");

            _listener = ListenAsync(cts.Token);
            _listener.Start();
        }

        private async Task ListenAsync(CancellationToken token)
        {
            IPEndPoint endpoint = new IPEndPoint(0, 0);
            ITftpCommand command = null;
            UdpReceiveResult response;

            while(!token.IsCancellationRequested)
            {
                try
                {
                    response = await client.ReceiveAsync().ConfigureAwait(continueOnCapturedContext: false);
                    command = parser.Parse(response.Buffer);
                }
                catch (SocketException e)
                {
                    //Handle receive error
                    RaiseOnError(new NetworkError(e));
                }
                catch (TftpParserException e2)
                {
                    //Handle parser error
                    RaiseOnError(new NetworkError(e2));
                }

                if (command != null)
                {
                    RaiseOnCommand(command, endpoint);
                }
            }
        }

        private void RaiseOnCommand(ITftpCommand command, IPEndPoint endpoint)
        {
            if (OnCommandReceived != null)
                OnCommandReceived(command, endpoint);
        }

        private void RaiseOnError(TftpTransferError error)
        {
            if (OnError != null)
                OnError(error);
        }

        public async Task SendAsync(ITftpCommand command)
        {
            if (client == null)
                throw new ObjectDisposedException("UdpChannel");

            if (endpoint == null)
                throw new InvalidOperationException("RemoteEndpoint needs to be set before you can send TFTP commands.");

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(command, stream);
                ArraySegment<byte> data;
                if (stream.TryGetBuffer(out data))
                {
                    await client.SendAsync(data.Array, (int)stream.Length, endpoint);
                }
                else
                {
                    throw new InvalidDataException("Problem getting buffer from memory stream.");
                }
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (this.client == null)
                    return;

                cts.Cancel();
                client.Dispose();
                this.client = null;
            }
        }

        public EndPoint RemoteEndpoint
        {
            get
            {
                return endpoint;
            }

            set
            {
                if (!(value is IPEndPoint))
                    throw new NotSupportedException("UdpChannel can only connect to IPEndPoints.");

                if (client == null)
                    throw new ObjectDisposedException("UdpChannel");

                this.endpoint = (IPEndPoint)value;
            }
        }
    }
}
