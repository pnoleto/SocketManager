using SoocketManager;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketManagerTest
{
    public class Tests
    {

        CancellationTokenSource cancellationTokenSource = new();
        private readonly byte[] LOCALHOST = [127, 0, 0, 1];
        private const int PORT = 1448;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task SendMessageBetweenSockets()
        {
            Socket? connectedSocket = null;

            Progress<Socket> _notify = new((sock) => { connectedSocket = sock; });

            var _address = new IPEndPoint(new IPAddress(LOCALHOST), PORT);

            Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            client.ListenerAsync(_address, _notify, cancellationTokenSource.Token);

            server.ConnectAsync(_address, cancellationTokenSource.Token, null);

            while (connectedSocket == null)
            {
                //Waiting connection
            }

            await connectedSocket.SendAsync(Encoding.UTF8.GetBytes("Hello word!"), cancellationTokenSource.Token);

            byte[] receivedBytes = await server.ReceiveAsync(cancellationTokenSource.Token);

            Assert.That(receivedBytes, Is.Not.Empty);
        }
    }
}