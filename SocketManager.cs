using System.Net.Sockets;
using System.Net;

namespace Shared
{
    [Obsolete("This class is obsolete because it was move to a diferent namespace, please use SoocketManager namespace instead.")]
    public class SocketManager
    {
        public SocketManager() { }

        [Obsolete("This method is obsolete, please use ConnectAsync instead.")]
        public static Task ConnectionPoolingAsync(
            Socket webSocket,
            IPEndPoint endpoint,
            CancellationToken cancellationToken,
            IProgress<Socket>? notifyConnection = null)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (!webSocket.Connected)
                        {
                            await webSocket.ConnectAsync(endpoint, cancellationToken);
                            notifyConnection?.Report(webSocket);
                        }
                    }
                    catch { }
                }

            }, cancellationToken);
        }

        /// <summary>
        /// This method create a task to listen for remote socket connections and notify each connected socket
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="endpoint"></param>
        /// <param name="notifyConnection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// 
        [Obsolete("This method is obsolete, please use ConnectAsync in the SocketManager namespace instead.")]
        public static Task ConnectionListenerAsync(
            Socket webSocket,
            IPEndPoint endpoint,
            IProgress<Socket> notifyConnection,
            CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                webSocket.Bind(endpoint);
                webSocket.Listen();

                while (true)
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

            }, cancellationToken);
        }

        /// <summary>
        /// This method is used to read the stream sent from the remote socket 
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>    
        [Obsolete("This method is obsolete, please use SocketReaderAsync in the SocketManager namespace instead.")]
        public static Task<byte[]> SocketReaderAsync(
           Socket webSocket,
           CancellationToken cancellationToken,
           IProgress<long>? notifyProgress = null)
        {
            return Task.Run(async () =>
            {
                byte[] buffer = Array.Empty<byte>();

                using MemoryStream stream = new();

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    buffer = new byte[webSocket.Available];

                    await webSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

                    await stream.WriteAsync(buffer);

                    notifyProgress?.Report(stream.Length);
                }
                while (webSocket.Available > 0);

                return stream.ToArray();

            }, cancellationToken);
        }

        /// <summary>
        /// This method is used to write a buffer in to a remote socket connection
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete, please use SocketWriterAsync in the SocketManager namespace instead.")]
        public static Task SocketWriterAsync(
            Socket webSocket,
            byte[] buffer,
            CancellationToken cancellationToken,
            IProgress<long>? notifyProgress = null)
        {
            return Task.Run(async () =>
            {
                long sent = 0;

                cancellationToken.ThrowIfCancellationRequested();

                sent = await webSocket.SendAsync(buffer, SocketFlags.None, cancellationToken);

                notifyProgress?.Report(sent);

            }, cancellationToken);
        }

        /// <summary>
        /// This method will return true if the remote socket connection has sent some information
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        [Obsolete("This method is obsolete, please use StreamAvaliable in the SocketManager namespace instead.")]
        public static bool StreamAvaliable(Socket socket)
        {
            return socket.Available > uint.MinValue;
        }
    }
}

namespace SoocketManager
{
    public class SocketManager
    {
        public SocketManager() { }

        /// <summary>
        /// Use this method to create a task to try to connect to an host.
        /// </summary>
        /// <param name="webSocket">Client socket to be used in the connection pooling</param>
        /// <param name="endpoint">Remote host address</param>
        /// <param name="notifyConnection">method that will receive de socket connection after the connection sucssed</param>
        /// <param name="cancellationToken">Cancelation token to stop the connection pooling</param>
        /// <returns></returns>
        public static Task ConnectAsync(
            Socket webSocket,
            IPEndPoint endpoint,
            CancellationToken cancellationToken,
            IProgress<Socket>? notifyConnection = null)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (!webSocket.Connected)
                        {
                            await webSocket.ConnectAsync(endpoint, cancellationToken);
                            notifyConnection?.Report(webSocket);
                        }
                    }
                    catch { }
                }

            }, cancellationToken);
        }

        /// <summary>
        /// This method create a task to listen for remote socket connections and notify each connected socket
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="endpoint"></param>
        /// <param name="notifyConnection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task ConnectionListenerAsync(
            Socket webSocket,
            IPEndPoint endpoint,
            IProgress<Socket> notifyConnection,
            CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                webSocket.Bind(endpoint);
                webSocket.Listen();

                while (true)
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

            }, cancellationToken);
        }


        /// <summary>
        /// This method is used to read the stream sent from the remote socket 
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>
        public static Task<byte[]> SocketReceiveAsync(
            Socket webSocket,
            CancellationToken cancellationToken,
            IProgress<long>? notifyProgress = null)
        {
            return Task.Run(async () =>
            {
                byte[] buffer = Array.Empty<byte>();

                using MemoryStream stream = new();

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    buffer = new byte[webSocket.Available];

                    await webSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

                    await stream.WriteAsync(buffer);

                    notifyProgress?.Report(stream.Length);
                }
                while (webSocket.Available > 0);

                return stream.ToArray();

            }, cancellationToken);
        }

        /// <summary>
        /// This method is used to write a buffer in to a remote socket connection
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="notifyProgress"></param>
        /// <returns></returns>
        public static Task SocketSendAsync(
            Socket webSocket,
            byte[] buffer,
            CancellationToken cancellationToken,
            IProgress<long>? notifyProgress = null)
        {
            return Task.Run(async () =>
            {
                long sent = 0;

                cancellationToken.ThrowIfCancellationRequested();

                sent = await webSocket.SendAsync(buffer, SocketFlags.None, cancellationToken);

                notifyProgress?.Report(sent);

            }, cancellationToken);
        }

        /// <summary>
        /// This method will return true if the remote socket connection has sent some information
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool StreamAvaliable(Socket socket)
        {
            return socket.Available > uint.MinValue;
        }
    }
}