using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Subscribers
{
    public class Subscriber : Client, ISubscriber
    {        
        public event EventHandler<DataCandidateEventArgs> OnReceived;
        public event EventHandler<DataEventArgs> OnConstructed;

        private string topic;
        private IStrategy strategy;

        public string Topic
        {
            get { return topic; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (topic != null && channel == null)
                    channel.Unsubscribe(topic);

                topic = value;

                if (channel != null)
                    channel.Subscribe(topic);
            }
        }
       
        public IStrategy Strategy
        {
            get { return strategy; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                strategy = value;
            }
        }

        public override void Connect(IChannel channel)
        {
            if (Strategy == null)
                throw new InvalidProgramException("Strategy must be set before calling connect.");

            if (Topic == null)
                throw new InvalidProgramException("Topic must be set before calling connect.");

            channel.OnDataAvailable += channel_OnDataAvailable;
            channel.Subscribe(Topic);
            base.Connect(channel);
        }

        public override void Disconnect()
        {
            channel.OnDataAvailable -= channel_OnDataAvailable;
            channel.Unsubscribe(Topic);
            base.Disconnect();
        }
       
        private void channel_OnDataAvailable(object sender, DataCandidateEventArgs e)
        {
            if (OnReceived != null)
                OnReceived.Invoke(this, e);

            var result = Strategy.Run(channel, filters.ToArray(), e.Candidate);

            if (result != null && OnConstructed != null)
                OnConstructed.Invoke(this, new DataEventArgs(result));
        }
    }
}
