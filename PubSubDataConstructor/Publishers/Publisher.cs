using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public class Publisher : IPublisher
    {
        public event EventHandler<DataCandidateEventArgs> OnPublished;
        public event EventHandler<DataCandidateEventArgs> OnQueued;

        private readonly ConcurrentQueue<DataCandidate> queue;
        protected readonly IChannel channel;

        public bool IsSuspended { get; private set; }

        public Publisher(IChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            this.channel = channel;
            this.channel.OnConnect += channel_OnConnect;
            this.channel.OnDisconnect += channel_OnDisconnect;

            this.queue = new ConcurrentQueue<DataCandidate>();
        }

        public void Resume()
        {
            this.CheckConnection();

            if (!IsSuspended)
                return;

            IsSuspended = false;

            DataCandidate candidate;
            while (queue.TryDequeue(out candidate))
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

        protected void CheckConnection()
        {
            if (!channel.IsConnected)
                channel.Connect();
        }

        protected virtual void OnChannelConnect() { }

        protected virtual void OnChannelDisconnect() { }

        protected void Queue(DataCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            queue.Enqueue(candidate);
            if (OnQueued != null)
                OnQueued.Invoke(this, new DataCandidateEventArgs(candidate));
        }

        protected void Push(DataCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            if (!channel.IsConnected)
                channel.Connect();

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
    }
}
