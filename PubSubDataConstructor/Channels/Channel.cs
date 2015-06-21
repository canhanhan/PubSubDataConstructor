using System;

namespace PubSubDataConstructor.Channels
{
    public abstract class Channel : IChannel
    {       
        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;

        public bool IsConnected { get; private set; }

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

            DisconnectExecute();

            IsConnected = false;

            if (OnDisconnect != null)
                OnDisconnect.Invoke(this, null);
        }

        public virtual void Publish(DataCandidate candidate)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Channel is not connected");

            PublishExecute(candidate);
        }

        public abstract void Subscribe(Topic topic, Action<DataCandidate> callback);
        public abstract void Unsubscribe(Topic topic, Action<DataCandidate> callback);

        protected virtual void ConnectExecute() { }
        protected virtual void DisconnectExecute() { }
        protected abstract void PublishExecute(DataCandidate candidate);
    }
}
