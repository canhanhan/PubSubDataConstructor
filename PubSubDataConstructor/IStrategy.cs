namespace PubSubDataConstructor
{
    public interface IStrategy
    {
        object Run(IChannel channel, IFilter[] filters, DataCandidate candidate);

        void Add(string field, IReducer reducer);
    }
}
