using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Strategies
{
    public class LoadAndBuildStrategy : IStrategy
    {
        public delegate object LoadDelegate(string id);

        private readonly LoadDelegate loadCallback;
        private readonly KeyValueStore<IReducer> valueReducers;

        public bool ResetFields { get; set; }

        public LoadAndBuildStrategy(LoadDelegate loadCallback)
        {
            if (loadCallback == null)
                throw new ArgumentNullException("loadCallback");

            this.loadCallback = loadCallback;
            this.valueReducers = new KeyValueStore<IReducer>();
        }

        public void Add(string field, IReducer reducer)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            if (reducer == null)
                throw new ArgumentNullException("reducer");

            valueReducers.Add(field, reducer);
        }

        public object Run(IChannel channel, IFilter[] filters, DataCandidate candidate)
        {
            var obj = loadCallback(candidate.TargetId);
            if (obj == null)
                return null;

            var allCandidates = channel.Poll(candidate.TargetId);
            var fieldCandidates = allCandidates.GroupBy(x => x.TargetField);
            var type = obj.GetType();
            foreach(var field in fieldCandidates)
            {
                var candidates = field
                    .OrderByDescending(x => x.Freshness)
                    .OrderByDescending(x => x.Priority)
                    .Where(x => filters.All(filter => filter.Accept(x))).ToArray();

                List<IReducer> reducers;
                if (valueReducers.TryGetValue(field.Key, out reducers))
                {
                    foreach(var reducer in reducers)
                    {
                        candidates = reducer.Reduce(candidates);
                    }
                }

                var selectedCandidate = candidates.FirstOrDefault();

                var property = type.GetProperty(field.Key);
                if (property == null)
                    throw new InvalidOperationException("Property '" + field.Key + "' does not exist in type '" + type.Name + "'");

                if (selectedCandidate == null) 
                {
                    if (!ResetFields)
                        continue;
                    else 
                        selectedCandidate = new DataCandidate { Value = TypeHelper.Default(property.PropertyType) };
                }

                property.SetValue(obj, selectedCandidate.Value, null);
            }

            return obj;
        }
    }
}
