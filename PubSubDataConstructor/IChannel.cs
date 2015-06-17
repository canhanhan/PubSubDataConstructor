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

        IEnumerable<DataCandidate> Poll(string topic);
        void Subscribe(string topic, Action<DataCandidate> callback);
        void Unsubscribe(string topic, Action<DataCandidate> callback);
        void Publish(DataCandidate dataCandidate);
    }
}
