using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Publishers;
using PubSubDataConstructor.Tests.Fakes;

namespace PubSubDataConstructor.Tests.Publishers
{
    [TestClass]
    public class PublisherTests
    {
        [TestMethod]
        public void Publisher_Publish_WhenSuspended_DoesNotPushToChannel()
        {
            var candidate = new DataCandidate();
            var channel = new FakeChannel();
            channel.OnPublish += (e, args) => Assert.Fail("Publisher called Push when suspended");
            var publisher = new Publisher();
            publisher.Connect(channel);

            publisher.Suspend();            
            publisher.Publish(candidate);         
        }

        [TestMethod]
        public void Publisher_Publish_WhenNotSuspended_PushesToChannel()
        {
            var candidate = new DataCandidate();
            var channel = new FakeChannel();
            var pushCalled = false;
            channel.OnPublish += (e, args) => pushCalled = true;
            var publisher = new Publisher();
            publisher.Connect(channel);
            
            publisher.Publish(candidate);

            Assert.IsTrue(pushCalled);
        }

        [TestMethod]
        public void Publisher_Publish_WhenResumed_PushesToChannel()
        {
            var candidate = new DataCandidate();
            var channel = new FakeChannel();
            var pushCalled = false;
            channel.OnPublish += (e, args) => pushCalled = true;
            var publisher = new Publisher();
            publisher.Connect(channel);

            publisher.Suspend();
            publisher.Publish(candidate);
            publisher.Resume();

            Assert.IsTrue(pushCalled);
        }
    }
}
