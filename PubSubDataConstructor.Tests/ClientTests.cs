using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PubSubDataConstructor.Tests.Fakes;
using System;

namespace PubSubDataConstructor.Tests
{
    [TestClass]
    public class ClientTests
    {
        private FakeChannel channel;
        private FakeRepository repository;
        private FakeClient client;

        [TestInitialize]
        public void Setup()
        {
            channel = new FakeChannel();
            repository = new FakeRepository();
            client = new FakeClient(repository);
        }

        [TestMethod]
        public void Client_Connect_TriggersEvent()
        {
            client.Attach(channel);
            channel.Connect();

            Assert.IsTrue(client.OnChannelConnectTriggered);
        }

        [TestMethod]
        public void Client_Disconnect_TriggersEvent()
        {
            client.Attach(channel);
            channel.Disconnect();

            Assert.IsTrue(client.OnChannelDisconnectTriggered);
        }

        [TestMethod]
        public void Client_Disconnect_DetachedChannel_DoesNotTriggerEvent()
        {
            client.Attach(channel);
            client.Detach();
            channel.Disconnect();

            Assert.IsFalse(client.OnChannelDisconnectTriggered);
        }

        [TestMethod]
        public void Client_Connect_DetachedChannel_DoesNotTriggerEvent()
        {
            client.Attach(channel);
            client.Detach();
            channel.Connect();

            Assert.IsFalse(client.OnChannelConnectTriggered);
        }

        [TestMethod]
        public void Client_ChannelPublish_TriggersCallback()
        {
            var wasCalled = false;
            var candidate = new DataCandidate();

            client.Attach(channel);
            client.Subscribe(candidate.ToTopic(), x => wasCalled = true);
            channel.FakePublish(candidate);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void Client_Unsubscribe_RemovesCallback()
        {
            Action<DataCandidate> callback1 = null;

            var channel = new Mock<IChannel>();
            channel.Setup(x => x.Subscribe(It.IsAny<Topic>(), It.IsAny<Action<DataCandidate>>()))
                .Callback<Topic, Action<DataCandidate>>((_, x) => callback1 = x);

            channel.Setup(x => x.Unsubscribe(It.IsAny<Topic>(), It.IsAny<Action<DataCandidate>>()))
                .Callback<Topic, Action<DataCandidate>>((_, x) => Assert.AreSame(callback1, x));

            var topic = new Topic();

            client.Attach(channel.Object);
            client.Subscribe(topic, x => { });
            client.Unsubscribe(topic);

            Assert.IsNotNull(callback1);
        }


        [TestMethod]
        public void Client_Detach_RemovesCallback()
        {
            Action<DataCandidate> callback1 = null;

            var channel = new Mock<IChannel>();
            channel.Setup(x => x.Subscribe(It.IsAny<Topic>(), It.IsAny<Action<DataCandidate>>()))
                .Callback<Topic, Action<DataCandidate>>((_, x) => callback1 = x);

            channel.Setup(x => x.Unsubscribe(It.IsAny<Topic>(), It.IsAny<Action<DataCandidate>>()))
                .Callback<Topic, Action<DataCandidate>>((_, x) => Assert.AreSame(callback1, x));

            var topic = new Topic();

            client.Attach(channel.Object);
            client.Subscribe(topic, x => { });
            client.Detach();

            Assert.IsNotNull(callback1);
        }

        [TestMethod]
        public void Client_Publish_WhenSuspended_DoesNotPushToChannel()
        {
            var candidate = new DataCandidate();
            channel.OnPublish += (e, args) => Assert.Fail("Publisher called Push when suspended");

            client.Attach(channel);
            client.Suspend();
            client.Publish(candidate);
        }

        [TestMethod]
        public void Client_Publish_WhenNotSuspended_PushesToChannel()
        {
            var candidate = new DataCandidate();
            var pushCalled = false;
            channel.OnPublish += (e, args) => pushCalled = true;

            client.Attach(channel);
            client.Publish(candidate);

            Assert.IsTrue(pushCalled);
        }

        [TestMethod]
        public void Client_Publish_WhenResumed_PushesToChannel()
        {
            var candidate = new DataCandidate();
            var pushCalled = false;
            channel.OnPublish += (e, args) => pushCalled = true;

            client.Attach(channel);
            client.Suspend();
            client.Publish(candidate);
            client.Resume();

            Assert.IsTrue(pushCalled);
        }
    }
}
