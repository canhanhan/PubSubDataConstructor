using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Reducers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PubSubDataConstructor.Subscribers
{
    public class FluentSubscriber<TTarget>
    {
        public class MapBuilder<TProperty>
        {
            private readonly FluentSubscriber<TTarget> subscriber;
            private readonly Expression<Func<TTarget, TProperty>> expression;
            private readonly string fieldName;

            public MapBuilder(FluentSubscriber<TTarget> subscriber, Expression<Func<TTarget, TProperty>> expression)
            {
                if (subscriber == null)
                    throw new ArgumentNullException("subscriber");

                if (expression == null)
                    throw new ArgumentNullException("expression");

                this.subscriber = subscriber;
                this.expression = expression;
                fieldName = ExpressionHelper.GetPropertyName(expression.Body);
            }

            public MapBuilder<TProperty> Filter(Func<object, bool> filter)
            {
                return Filter(new CustomFieldFilter(fieldName, filter));
            }

            public MapBuilder<TProperty> Filter<T>(Func<T, bool> filter)
            {
                return Filter(new CustomFieldFilter<T>(fieldName, filter));
            }

            public MapBuilder<TProperty> NotBlank()
            {
                return Filter(new BlankFilter(fieldName));            
            }

            public MapBuilder<TProperty> MinLength(int length)
            {
                return Filter(new MinLengthFilter(fieldName, length));
            }

            public MapBuilder<TProperty> MaxLength(int length)
            {
                return Filter(new MaxLengthFilter(fieldName, length));
            }

            public MapBuilder<TProperty> SetReducer(IReducer reducer)
            {
                if (reducer == null)
                    throw new ArgumentNullException("reducer");

                subscriber.valueReducers.Add(fieldName, reducer);

                return this;
            }

            public MapBuilder<TProperty> SetReducer<TReducer>() where TReducer : IReducer, new()
            {
                return SetReducer(new TReducer());
            }

            public MapBuilder<TProperty> Min()
            {
                return SetReducer(new MinReducer());
            }

            public MapBuilder<TProperty> Max()
            {
                return SetReducer(new MaxReducer());
            }

            public MapBuilder<TProperty> Union()
            {
                return SetReducer(new UnionReducer());
            }

            public MapBuilder<TProperty> Join()
            {
                return SetReducer(new JoinReducer());
            }

            private MapBuilder<TProperty> Filter(IFilter filter)
            {
                subscriber.Filter(filter);

                return this;
            }
        }

        public event EventHandler<DataEventArgs> OnConstructed;

        private readonly ISubscriber subscriber;
        private readonly KeyValueStore<IReducer> valueReducers;
        private bool resetFields;

        public Func<DataCandidate, TTarget> Factory { get; set; }

        public FluentSubscriber(IChannel channel)
        {
            this.Factory = x => Activator.CreateInstance<TTarget>();
            this.subscriber = new Subscriber(channel);
            this.valueReducers = new KeyValueStore<IReducer>();
        }
        
        public void Filter(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            subscriber.AddFilter(filter);
        }

        public void Filter(Func<DataCandidate, bool> filter)
        {
            Filter(new CustomFilter(filter));
        }

        public MapBuilder<TProperty> Map<TProperty>(Expression<Func<TTarget, TProperty>> expression)
        {
            var builder = new MapBuilder<TProperty>(this, expression);

            return builder;
        }

        public void ResetFields(bool value)
        {
            resetFields = value;
        }

        private void Build(DataCandidate candidate)
        {
            var obj = Factory.Invoke(candidate);
            if (obj == null)
                return;

            var allCandidates = subscriber.Poll(candidate.TargetId);
            var fieldCandidates = allCandidates.GroupBy(x => x.TargetField);
            var type = obj.GetType();
            foreach (var field in fieldCandidates)
            {
                var candidates = field
                    .OrderByDescending(x => x.Freshness)
                    .OrderByDescending(x => x.Priority)
                    .Where(x => subscriber.Filters.All(filter => filter.Accept(x))).ToArray();

                List<IReducer> reducers;
                if (valueReducers.TryGetValue(field.Key, out reducers))
                {
                    foreach (var reducer in reducers)
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
