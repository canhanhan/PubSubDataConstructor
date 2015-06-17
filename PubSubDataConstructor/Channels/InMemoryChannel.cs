using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PubSubDataConstructor.Channels
{
    public sealed class InMemoryChannel : IChannel
    {
        private static readonly List<DataCandidate> repository = new List<DataCandidate>();
        private static readonly KeyValueStore<InMemoryChannel> subscriptions = new KeyValueStore<InMemoryChannel>();

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;
        public event EventHandler<DataCandidateEventArgs> OnDataAvailable;

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            if (IsConnected)
                throw new InvalidOperationException("The channel is already connected.");

            IsConnected = true;

            if (OnConnect != null)
                OnConnect.Invoke(this, null);
        }

        public void Disconnect()
        {
            if (!IsConnected)
                throw new InvalidOperationException("The channel is not connected.");

            IsConnected = false;
            subscriptions.Remove(this);
            
            if (OnDisconnect!= null)
                OnDisconnect.Invoke(this, null);
        }

        public IEnumerable<DataCandidate> Poll(string topic)
        {
            return repository.Where(x => IsMatch(x.TargetId, topic));
        }

        public void Subscribe(string topic)
        {
            subscriptions.Add(topic, this);
        }

        public void Unsubscribe(string topic)
        {
            subscriptions.Remove(topic, this);
        }

        public void Publish(DataCandidate candidate)
        {
            repository.Add(candidate);
            foreach (var channel in subscriptions.Where(x => IsMatch(candidate.TargetId, x.Key)).SelectMany(x => x.Value))
            {
                channel.TriggerDataAvailable(candidate);
            }
        }

        private void TriggerDataAvailable(DataCandidate candidate)
        {
            if (OnDataAvailable != null)
                this.OnDataAvailable(this, new DataCandidateEventArgs(candidate));
        }

        private static bool IsMatch(string target, string topic)
        {
            return Regex.IsMatch(target, topic, RegexOptions.IgnoreCase);
        }

        public static void Reset()
        {
            repository.Clear();
            foreach(var subscription in subscriptions.Values)
            {
                subscription.Clear();
            }

            subscriptions.Clear();
        }
    }
}
