using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IChannel
    {
        event EventHandler OnConnect;
        event EventHandler OnDisconnect;

        bool IsConnected { get; }

        void Connect();
        void Disconnect();

        void Subscribe(Topic topic, Action<DataCandidate> callback);
        void Unsubscribe(Topic topic, Action<DataCandidate> callback);
        void Publish(DataCandidate dataCandidate);
    }
}
