using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Subscribers
{
    public class Subscriber : Client, ISubscriber
    {
        public event EventHandler<DataCandidateEventArgs> OnReceived;

        private readonly KeyValueStore<Action<DataCandidate>> subscriptions;
        protected readonly List<IFilter> filters;

        public IEnumerable<IFilter> Filters { get { return filters; } }

        public Subscriber(IChannel channel) : base(channel) 
        {
            this.filters = new List<IFilter>();
            this.subscriptions = new KeyValueStore<Action<DataCandidate>>();
        }

        public void AddFilter(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            filters.Add(filter);
        }

        public void RemoveFilter(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            filters.Remove(filter);
        }

        public IEnumerable<DataCandidate> Poll(string topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            this.CheckConnection();

            return channel.Poll(topic);
        }

        public virtual void Subscribe(string topic, Action<DataCandidate> callback)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            if (callback == null)
                throw new ArgumentNullException("callback");

            this.CheckConnection();

            Action<DataCandidate> wrappedCallback = candidate => channel_OnDataAvailable(candidate, callback);       
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
      
        private void channel_OnDataAvailable(DataCandidate candidate, Action<DataCandidate> callback)
        {
            if (OnReceived != null)
                OnReceived.Invoke(this, new DataCandidateEventArgs(candidate));

            callback.Invoke(candidate);
        }
    }
}
