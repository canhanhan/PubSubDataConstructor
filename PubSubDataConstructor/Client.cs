using PubSubDataConstructor.Repositories;
using PubSubDataConstructor.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubSubDataConstructor
{
    public class Client : IClient
    {
        public event EventHandler<DataCandidateEventArgs> OnPublished;
        public event EventHandler<DataCandidateEventArgs> OnQueued;

        private readonly ConcurrentQueue<DataCandidate> publishQueue;
        private readonly KeyValueStore<Topic, Action<DataCandidate>> subscriptions;
        private readonly IRepository Repository;

        protected readonly IChannel channel;

        public bool IsSuspended { get; private set; }
        
        public Client(IChannel channel, IRepository repository)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            if (repository == null)
                throw new ArgumentNullException("repository");

            this.channel = channel;
            this.channel.OnConnect += channel_OnConnect;
            this.channel.OnDisconnect += channel_OnDisconnect;

            this.Repository = repository;
            this.publishQueue = new ConcurrentQueue<DataCandidate>();
            this.subscriptions = new KeyValueStore<Topic, Action<DataCandidate>>();
        }

        public void Resume()
        {
            this.CheckConnection();

            if (!IsSuspended)
                return;

            IsSuspended = false;

            DataCandidate candidate;
            while (publishQueue.TryDequeue(out candidate))
            {
                channel.Publish(candidate);
            }
        }

        public virtual void Suspend()
        {
            if (IsSuspended)
                return;

            IsSuspended = true;
        }

        public void Publish(DataCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            if (IsSuspended)
            {
                Queue(candidate);
            }
            else
            {
                Push(candidate);
            }
        }

        public void Publish(IEnumerable<DataCandidate> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException("candidates");

            var wasSuspended = IsSuspended;
            if (!wasSuspended)
                Suspend();

            try
            {
                foreach (var candidate in candidates)
                    Publish(candidate);
            }
            finally
            {
                if (!wasSuspended)
                    Resume();
            }
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

        public virtual IEnumerable<DataCandidate> Poll(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            return Repository.List(topic);
        }

        public virtual IEnumerable<DataCandidate> List()
        {
            return Repository.List();
        }

        public virtual void Load(IEnumerable<DataCandidate> candidates)
        {
            foreach (var candidate in candidates)
                Repository.Add(candidate);
        }

        public virtual void Clear()
        {
            Repository.Clear();
        }

        protected void CheckConnection()
        {
            if (!channel.IsConnected)
                channel.Connect();
        }

        protected virtual void OnChannelConnect() { }

        protected virtual void OnChannelDisconnect() 
        {
            subscriptions.Clear();
        }

        protected void Queue(DataCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            publishQueue.Enqueue(candidate);
            if (OnQueued != null)
                OnQueued.Invoke(this, new DataCandidateEventArgs(candidate));
        }

        protected void Push(DataCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            CheckConnection();

            channel.Publish(candidate);

            if (OnPublished != null)
                OnPublished.Invoke(this, new DataCandidateEventArgs(candidate));
        }

        private void channel_OnConnect(object sender, EventArgs e)
        {
            this.OnChannelConnect();
        }

        private void channel_OnDisconnect(object sender, EventArgs e)
        {
            this.OnChannelDisconnect();
        }

        private void channel_OnDataAvailable(DataCandidate candidate, Action<DataCandidate> callback)
        {
            Repository.Add(candidate);
            callback.Invoke(candidate);
        }
    }
}
