using PubSubDataConstructor.Utils;
using System;
using System.Linq;

namespace PubSubDataConstructor.Channels
{
    public class InMemoryChannel : Channel
    {
        private readonly KeyValueStore<Topic, Action<DataCandidate>> subscriptions;

        public InMemoryChannel()
        {
            subscriptions = new KeyValueStore<Topic, Action<DataCandidate>>();
        }

        public override void Subscribe(Topic topic, Action<DataCandidate> callback)
        {
            subscriptions.Add(topic, callback);
        }

        public override void Unsubscribe(Topic topic, Action<DataCandidate> callback)
        {
            subscriptions.Remove(topic, callback);
        }

        protected override void DisconnectExecute()
        {
            foreach (var subscription in subscriptions.Values)
            {
                subscription.Clear();
            }

            subscriptions.Clear();
        }

        protected override void PublishExecute(DataCandidate candidate)
        {
            foreach (var callback in subscriptions.Where(x => TopicHelper.IsMatch(x.Key, candidate.ToTopic())).SelectMany(x => x.Value))
            {
                callback.Invoke(candidate);
            }
        }
    }
}
