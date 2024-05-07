using System.Net.Sockets;
using System.Net;

namespace SoocketManager
{
    public class SocketManager
    {
        private const int MAX_RECEIVE_BUFFER_SIZE = 8192;
        private const int ZERO = 0;

        public SocketManager() { }

        /// <summary>
        /// Use this method to create a task and try to connect to a specific host.
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="endpoint"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyConnection"</param>
        /// <returns></returns>
        public static async Task ConnectAsync(
            Socket webSocket,
            IPEndPoint endpoint,
            CancellationToken cancellationToken,
            IProgress<Socket>? notifyConnection = null)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!webSocket.Connected)
                    {
                        await webSocket.ConnectAsync(endpoint, cancellationToken);
                        notifyConnection?.Report(webSocket);
                    }
                }
                catch { }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// This method creates a task to listen for remote socket connections and notify each connected socket to the specified method
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="endpoint"></param>
        /// <param name="notifyConnection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ConnectionListenerAsync(
            Socket webSocket,
            IPEndPoint endpoint,
            IProgress<Socket> notifyConnection,
            CancellationToken cancellationToken)
        {
            webSocket.Bind(endpoint);
            webSocket.Listen();

            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    Socket client = await webSocket.AcceptAsync(cancellationToken);
                    notifyConnection.Report(client);
                }
                catch
                {
                    webSocket.Close();
                    throw;
                }
            }
        }


        /// <summary>
        /// This method is used to read the stream sent from the remote web socket 
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>
        public async static Task<byte[]> SocketReceiveAsync(
            Socket webSocket,
            CancellationToken cancellationToken,
            IProgress<long>? notifyProgress = null)
        {
            int readBytes;
            byte[] buffer = new byte[MAX_RECEIVE_BUFFER_SIZE];

            using (MemoryStream stream = new())
            {
                if (StreamAvaliable(webSocket))
                {
                    while ((readBytes = await webSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken)) > ZERO)
                    {
                        await stream.WriteAsync(buffer.AsMemory(ZERO, readBytes), cancellationToken);

                        notifyProgress?.Report(stream.Length);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// This method is used to write a buffer into the remote web socket connection
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>
        public static async Task SocketSendAsync(
            Socket webSocket,
            byte[] buffer,
            CancellationToken cancellationToken,
            IProgress<long>? notifyProgress = null)
        {
            long sent;

            while ((sent = await webSocket.SendAsync(buffer, SocketFlags.None, cancellationToken)) > ZERO)
            {
                notifyProgress?.Report(sent);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// This method will return true if the remote socket connection has sent some information
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public static bool StreamAvaliable(Socket webSocket)
        {
            return webSocket.Available > uint.MinValue;
        }
    }
}