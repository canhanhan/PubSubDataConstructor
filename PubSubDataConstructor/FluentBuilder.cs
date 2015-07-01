using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Reducers;
using PubSubDataConstructor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PubSubDataConstructor
{
    public abstract class FluentBuilder<TTarget> : IBuilder
    {
        public class MapBuilder<TProperty>
        {
            private readonly FluentBuilder<TTarget> builder;
            private readonly Expression<Func<TTarget, TProperty>> expression;
            private readonly string fieldName;

            internal MapBuilder(FluentBuilder<TTarget> subscriber, Expression<Func<TTarget, TProperty>> expression)
            {
                if (subscriber == null)
                    throw new ArgumentNullException("subscriber");

                if (expression == null)
                    throw new ArgumentNullException("expression");

                this.builder = subscriber;
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

                builder.valueReducers.Add(fieldName, reducer);

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
                builder.Filter(filter);

                return this;
            }
        }

        public event EventHandler<DataEventArgs> OnConstructed;

        private readonly List<IFilter> filters;
        private readonly List<Tuple<Topic, Action<DataCandidate>>> customTopics;
        private readonly KeyValueStore<string, IReducer> valueReducers;

        private bool resetFields;
        private Topic topic;
        private IClient client;
        private IDictionary<string, object> context;

        protected IClient Client { get { return client; } }

        public FluentBuilder()
        {            
            this.filters = new List<IFilter>();
            this.customTopics = new List<Tuple<Topic, Action<DataCandidate>>>();
            this.valueReducers = new KeyValueStore<string, IReducer>();
        }

        public void Start(IClient client, IDictionary<string, object> context)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (context == null)
                throw new ArgumentNullException("context");

            if (this.client != null)
                throw new InvalidOperationException(GetType().Name + " is already started.");

            this.topic = new Topic { Type = typeof(TTarget).Name };
            this.context = context;
            this.client = client;
            if (customTopics.Count == 0)
            {
                this.client.Subscribe(topic, DataReceived);
            }
            else
            {
                foreach (var pair in customTopics)
                    this.client.Subscribe(pair.Item1, pair.Item2);
            }
        }

        public void Stop()
        {
            if (client == null)
                throw new InvalidOperationException(GetType().Name + " is not started.");

            if (customTopics.Count == 0)
            {
                this.client.Unsubscribe(topic);
            }
            else
            {
                foreach (var pair in customTopics)
                    this.client.Unsubscribe(pair.Item1);
            }

            this.client = null;
            this.context = null;
        }

        protected void ResetFields(bool value)
        {
            resetFields = value;
        }

        protected void Filter(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            filters.Add(filter);
        }

        protected void Filter(Func<DataCandidate, bool> filter)
        {
            Filter(new CustomFilter(filter));
        }

        protected MapBuilder<TProperty> Map<TProperty>(Expression<Func<TTarget, TProperty>> expression)
        {
            return new MapBuilder<TProperty>(this, expression);
        }

        protected void Listen<TProperty>(Expression<Func<TTarget, TProperty>> expression, Action<DataCandidate> callback)
        {
            var fieldName = ExpressionHelper.GetPropertyName(expression.Body);

            customTopics.Add(Tuple.Create(new Topic { Type = typeof(TTarget).Name, Field = fieldName }, callback));
        }

        protected virtual void SetId(object obj, DataCandidate candidate)
        {
            typeof(TTarget).GetProperty("Id").SetValue(obj, candidate.TargetId);
        }

        protected virtual void DataReceived(DataCandidate candidate)
        {
            object obj;
            if (!context.TryGetValue(candidate.TargetId, out obj))
            {
                obj = Activator.CreateInstance<TTarget>();
                SetId(obj, candidate);
                context.Add(candidate.TargetId, obj);
            }

            var allCandidates = client.Poll(candidate.ToTopic());
            var fieldCandidates = allCandidates.GroupBy(x => x.TargetField);
            var type = obj.GetType();
            foreach (var field in fieldCandidates)
            {
                IEnumerable<DataCandidate> candidates = field.Where(x => filters.All(filter => filter.Accept(x))).ToArray();

                if (candidates.Count() > 0)
                {
                    var maxPriority = candidates.Max(x => x.Priority);
                    candidates = candidates
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
