using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Tests.Fakes;

namespace PubSubDataConstructor.Tests
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void Client_Connect_TriggersEvent()
        {
            var channel = new FakeChannel();
            var client = new FakeClient(channel);
            channel.Connect();

            Assert.IsTrue(client.OnChannelConnectTriggered);
        }

        [TestMethod]
        public void Client_Disconnect_TriggersEvent()
        {
            var channel = new FakeChannel();
            var client = new FakeClient(channel);

            channel.Disconnect();

            Assert.IsTrue(client.OnChannelDisconnectTriggered);
        }
    }
}
