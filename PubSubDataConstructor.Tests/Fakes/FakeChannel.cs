using System;
using System.Collections.Generic;

namespace PubSubDataConstructor.Tests.Fakes
{
    class FakeChannel : IChannel
    {
        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;

        public event EventHandler<DataCandidateEventArgs> OnPublish;
        public event EventHandler OnPoll;
        public event EventHandler OnSubscribe;
        public event EventHandler OnUnsubscribe;

        public bool IsConnected { get; set; }

        public void Connect()
        {
            IsConnected = true;

            if (this.OnConnect != null)
                this.OnConnect.Invoke(this, null);
        }

        public void Disconnect()
        {
            IsConnected = false;

            if (this.OnDisconnect != null)
                this.OnDisconnect.Invoke(this, null);
        }

        public void Subscribe(string topic, Action<DataCandidate> callback)
        {
            if (OnSubscribe != null)
                OnSubscribe.Invoke(this, null);
        }

        public void Unsubscribe(string topic, Action<DataCandidate> callback)
        {
            if (OnUnsubscribe != null)
                OnUnsubscribe.Invoke(this, null);
        }

        public IEnumerable<DataCandidate> Poll(string topic) {
            if (OnPoll != null)
                OnPoll.Invoke(this, null);

            return null;
        }

        public void Publish(DataCandidate dataCandidate)
        {
            if (OnPublish != null)
                OnPublish.Invoke(this, new DataCandidateEventArgs(dataCandidate));
        }
    }
}
