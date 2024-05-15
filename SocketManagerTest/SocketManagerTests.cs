using SoocketManager;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketManagerTest
{
    public class Tests
    {

        CancellationToken cancellationToken = new CancellationToken();
        private readonly byte[] LOCALHOST = new byte[] { 127, 0, 0, 1 };
        private const int PORT = 1448;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task SendMEssageBetweenSockets()
        {
            Socket? connectedSocket = null;

            var _notify = new Progress<Socket>((x) => { connectedSocket = x; });

            var _address = new IPEndPoint(new IPAddress(LOCALHOST), PORT);

            Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            client.ListenerAsync(_address, _notify, cancellationToken);

            server.ConnectAsync(_address, cancellationToken, null);

            while (connectedSocket == null)
            {
                //Waiting connection
            }

            await connectedSocket.SendAsync(Encoding.UTF8.GetBytes("Hello word!"), cancellationToken);

            byte[] receivedBytes = await server.ReceiveAsync(cancellationToken);

            Assert.That(receivedBytes, Is.Not.Empty);
        }
    }
}