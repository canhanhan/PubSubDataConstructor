namespace PubSubDataConstructor.Tests.Fakes
{
    class FakeFilter : IFilter
    {
        public bool ExpectedResult { get; set; }
        public bool Accept(DataCandidate candidate) { return ExpectedResult; }
    }
}
