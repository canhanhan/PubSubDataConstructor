using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Subscribers
{
    public class Subscriber : Client, ISubscriber
    {        
        public event EventHandler<DataCandidateEventArgs> OnReceived;
        public event EventHandler<DataEventArgs> OnConstructed;

        private readonly KeyValueStore<Action<DataCandidate>> subscriptions;

        public Subscriber(IChannel channel) : base(channel) 
        {
            this.subscriptions = new KeyValueStore<Action<DataCandidate>>();
        }

        public virtual void Subscribe(string topic, IStrategy strategy)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            if (strategy == null)
                throw new ArgumentNullException("strategy");

            this.CheckConnection();

            Action<DataCandidate> callback = x => channel_OnDataAvailable(x, topic, strategy);       
            subscriptions.Add(topic, callback);
            channel.Subscribe(topic, callback);
        }

        public virtual void Unsubscribe(string topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            this.CheckConnection();

            var callbacks = subscriptions[topic];
            foreach (var callback in callbacks)
                channel.Unsubscribe(topic, callback);
        }
      
        private void channel_OnDataAvailable(DataCandidate candidate, string topic, IStrategy strategy)
        {
            if (OnReceived != null)
                OnReceived.Invoke(this, new DataCandidateEventArgs(candidate));

            var result = strategy.Run(channel, filters.ToArray(), candidate);

            if (result != null && OnConstructed != null)
                OnConstructed.Invoke(this, new DataEventArgs(result));
        }
    }
}
