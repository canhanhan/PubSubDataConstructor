using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Reducers;
using PubSubDataConstructor.Subscribers.Repositories;
using PubSubDataConstructor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PubSubDataConstructor.Subscribers
{
    public class FluentSubscriber<TTarget> : SubscriberLike<TTarget>
    {
        public event EventHandler<DataEventArgs> OnConstructed;

        private bool resetFields;

        public Func<DataCandidate, TTarget> Factory { get; set; }

        public FluentSubscriber(ISubscriber subscriber) : base(subscriber)
        {
            this.Factory = x => Activator.CreateInstance<TTarget>();
        }

        public void ResetFields(bool value)
        {
            resetFields = value;
        }

        protected override void DataReceived(DataCandidate candidate)
        {
            var obj = Factory.Invoke(candidate);
            if (obj == null)
                return;

            var allCandidates = subscriber.Poll(candidate.ToTopic());
            var fieldCandidates = allCandidates.GroupBy(x => x.TargetField);
            var type = obj.GetType();
            foreach (var field in fieldCandidates)
            {
                var acceptedCandidates = field.Where(x => subscriber.Filters.All(filter => filter.Accept(x))).ToArray();
                var maxPriority = acceptedCandidates.Max(x => x.Priority);
                IEnumerable<DataCandidate> candidates = acceptedCandidates
                    .Where(x => x.Priority == maxPriority)
                    .OrderByDescending(x => x.Freshness)
                    .OrderByDescending(x => x.Priority);

                List<IReducer> reducers;
                if (valueReducers.TryGetValue(field.Key, out reducers))
                {
                    foreach (var reducer in reducers)
                    {
                        candidates = reducer.Reduce(candidates);
                        if (candidates == null)
                            throw new InvalidOperationException(reducer.GetType().Name + " returned null. Reducers shall not return null valies");
                    }
                }

                var selectedCandidate = candidates.FirstOrDefault();

                var property = type.GetProperty(field.Key);
                if (property == null)
                    throw new InvalidOperationException("Property '" + field.Key + "' does not exist in type '" + type.Name + "'");

                if (selectedCandidate == null)
                {
                    if (!resetFields)
                        continue;
                    else
                        selectedCandidate = new DataCandidate { Value = TypeHelper.Default(property.PropertyType) };
                }

                property.SetValue(obj, selectedCandidate.Value, null);
            }

            if (OnConstructed != null)
                OnConstructed.Invoke(this, new DataEventArgs(obj));
        }
    }
}
