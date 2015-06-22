using System;
using System.Linq;
using System.Collections.Generic;
using PubSubDataConstructor.Subscribers.Repositories;

namespace PubSubDataConstructor
{
    public class Subscriber : Publisher, ISubscriber
    {       
        private readonly KeyValueStore<Topic, Action<DataCandidate>> subscriptions;
        protected readonly List<IFilter> filters;

        public IEnumerable<IFilter> Filters { get { return filters; } }
        public IRepository Repository { get; private set; }

        public Subscriber(IChannel channel, IRepository repository) : base(channel) 
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            this.Repository = repository;
            this.filters = new List<IFilter>();            
            this.subscriptions = new KeyValueStore<Topic, Action<DataCandidate>>();
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

        public virtual IEnumerable<DataCandidate> Poll(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            return Repository.List(topic);
        }

        public virtual void Subscribe(Topic topic, Action<DataCandidate> callback)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            if (callback == null)
                throw new ArgumentNullException("callback");

            this.CheckConnection();

            Action<DataCandidate> wrappedCallback = candidate => channel_OnDataAvailable(candidate, callback);
            subscriptions.Add(topic, wrappedCallback);
            channel.Subscribe(topic, wrappedCallback);
        }

        public virtual void Unsubscribe(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            this.CheckConnection();

            var callbacks = subscriptions[topic];
            foreach (var callback in callbacks)
                channel.Unsubscribe(topic, callback);

            subscriptions.Remove(topic);
        }
      
        private void channel_OnDataAvailable(DataCandidate candidate, Action<DataCandidate> callback)
        {
            if (filters.Any(x => !x.Accept(candidate)))
                return;

            Repository.Add(candidate);            
            callback.Invoke(candidate);
        }
    }
}
