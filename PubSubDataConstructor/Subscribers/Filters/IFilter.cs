namespace PubSubDataConstructor
{
    public interface IFilter
    {
        bool Accept(DataCandidate candidate);
    }
}
