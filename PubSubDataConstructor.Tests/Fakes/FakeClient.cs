namespace PubSubDataConstructor.Tests.Fakes
{
    class FakeClient : Client
    {
        public bool OnChannelConnectTriggered { get; set; }
        public bool OnChannelDisconnectTriggered { get; set; }

        public FakeClient(IRepository repository) : base(repository) {}

        protected override void OnChannelConnect()
        {
            OnChannelConnectTriggered = true;
        }

        protected override void OnChannelDisconnect()
        {
            OnChannelDisconnectTriggered = true;
        }
    }
}
