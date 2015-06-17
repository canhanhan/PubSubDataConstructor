﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Publishers
{
    public class Publisher : Client, IPublisher
    {
        public event EventHandler<DataCandidateEventArgs> OnPublished;
        public event EventHandler<DataCandidateEventArgs> OnQueued;

        private readonly ConcurrentQueue<DataCandidate> queue;

        public Publisher()
        {
            this.queue = new ConcurrentQueue<DataCandidate>();
        }

        public void Publish(DataCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            if (filters.Any(x => !x.Accept(candidate)))
                return;

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

        public override void Resume()
        {
            base.Resume();

            DataCandidate candidate;
            while(queue.TryDequeue(out candidate))
            {
                channel.Publish(candidate);
            }
        }

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

            channel.Publish(candidate);
            if (OnPublished != null)
                OnPublished.Invoke(this, new DataCandidateEventArgs(candidate));
        }
    }
}
