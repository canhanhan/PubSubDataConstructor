using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IChannel
    {
        event EventHandler OnConnect;
        event EventHandler OnDisconnect;
        event EventHandler<DataCandidateEventArgs> OnDataAvailable;

        bool IsConnected { get; }

        void Connect();
        void Disconnect();

        IEnumerable<DataCandidate> Poll(string topic);
        void Subscribe(string topic);
        void Unsubscribe(string topic);
        void Publish(DataCandidate dataCandidate);
    }
}
