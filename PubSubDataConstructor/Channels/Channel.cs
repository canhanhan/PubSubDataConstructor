using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PubSubDataConstructor.Channels
{
    public abstract class Channel : IChannel
    {       
        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;

        private readonly KeyValueStore<Action<DataCandidate>> subscriptions;

        public bool IsConnected { get; private set; }

        public Channel()
        {
            subscriptions = new KeyValueStore<Action<DataCandidate>>();
        }

        public virtual void Connect()
        {
            if (IsConnected)
                throw new InvalidOperationException("The channel is already connected.");

            ConnectExecute();

            IsConnected = true;

            if (OnConnect != null)
                OnConnect.Invoke(this, null);
        }

        public virtual void Disconnect()
        {
            if (!IsConnected)
                throw new InvalidOperationException("The channel is not connected.");

            Reset();
            DisconnectExecute();

            IsConnected = false;

            if (OnDisconnect != null)
                OnDisconnect.Invoke(this, null);
        }

        public IEnumerable<DataCandidate> Poll(string topic)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Channel is not connected");

            return PollExecute(topic);
        }

        protected virtual void ConnectExecute() { }
        protected virtual void DisconnectExecute() { }
        protected abstract IEnumerable<DataCandidate> PollExecute(string topic);
        protected abstract void PublishExecute(DataCandidate candidate);

        public void Publish(DataCandidate candidate)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Channel is not connected");

            PublishExecute(candidate);

            foreach (var channel in subscriptions.Where(x => IsMatch(candidate.TargetId, x.Key)).SelectMany(x => x.Value))
            {
                channel.Invoke(candidate);
            }
        }

        public void Subscribe(string topic, Action<DataCandidate> callback)
        {
            subscriptions.Add(topic, callback);
        }

        public void Unsubscribe(string topic, Action<DataCandidate> callback)
        {
            subscriptions.Remove(topic, callback);
        }

        protected static bool IsMatch(string target, string topic)
        {
            return Regex.IsMatch(target, topic, RegexOptions.IgnoreCase);
        }

        public void Reset()
        {
            foreach (var subscription in subscriptions.Values)
            {
                subscription.Clear();
            }

            subscriptions.Clear();
        }
    }
}
