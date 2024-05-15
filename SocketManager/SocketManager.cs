using System.Net.Sockets;
using System.Net;

namespace SoocketManager
{
    public static class SocketManager
    {
        private const int RECEIVE_BUFFER_SIZE = 2048;
        private const int ZERO = 0;

        /// <summary>
        /// Use this method to create a task and try to connect to a specific host. this task is a long runner.
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="endpoint"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyConnection"</param>
        /// <returns></returns>
        public static Task ConnectAsync(
            this Socket webSocket,
            IPEndPoint endpoint,
            CancellationToken cancellationToken,
            IProgress<Socket>? notifyConnection = null)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await webSocket.ConnectAsync(endpoint, cancellationToken);
                        notifyConnection?.Report(webSocket);
                    }
                    catch
                    {
                        Thread.Sleep(5000);
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// This method creates a task to listen for remote socket connections and notify each connected socket to the specified method. this method is a long runner
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="endpoint"></param>
        /// <param name="notifyConnection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task ListenerAsync(
            this Socket webSocket,
            IPEndPoint endpoint,
            IProgress<Socket> notifyConnection,
            CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                webSocket.Bind(endpoint);
                webSocket.Listen(10);

                while (true)
                {
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
            }, cancellationToken);
        }


        /// <summary>
        /// This method is used to read the stream sent from the remote web socket 
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>
        public static Task<byte[]> ReceiveAsync(
            this Socket webSocket,
            CancellationToken cancellationToken,
            IProgress<long>? notifyProgress = null)
        {
            return Task.Run(async () =>
            {
                int readBytes;
                byte[] buffer = new byte[RECEIVE_BUFFER_SIZE];

                using MemoryStream stream = new();

                do
                {
                    readBytes = await webSocket.ReceiveAsync(buffer.AsMemory(ZERO, buffer.Length), SocketFlags.None, cancellationToken);

                    await stream.WriteAsync(buffer.AsMemory(ZERO, readBytes), cancellationToken);

                    notifyProgress?.Report(readBytes);
                }
                while (StreamAvaliable(webSocket));

                return stream.ToArray();
            }, cancellationToken);
        }

        /// <summary>
        /// This method is used to write a buffer into the remote web socket connection
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>
        public static Task SendAsync(
            this Socket webSocket,
            byte[] buffer,
            CancellationToken cancellationToken,
            IProgress<long>? notifyProgress = null)
        {
            return Task.Run(async () =>
            {
                int sent = await webSocket.SendAsync(buffer.AsMemory(ZERO, buffer.Length), SocketFlags.None, cancellationToken);
                notifyProgress?.Report(sent);
            });
        }

        /// <summary>
        /// This method will return true if the remote socket connection has sent some information
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public static bool StreamAvaliable(this Socket webSocket)
        {
            return webSocket.Available > uint.MinValue;
        }
    }
}