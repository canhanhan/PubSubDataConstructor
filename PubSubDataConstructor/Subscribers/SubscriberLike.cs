using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Reducers;
using PubSubDataConstructor.Utils;
using System;
using System.Linq.Expressions;

namespace PubSubDataConstructor.Subscribers
{
    public abstract class SubscriberLike<TTarget>
    {
        public class MapBuilder<TProperty>
        {
            private readonly SubscriberLike<TTarget> subscriber;
            private readonly Expression<Func<TTarget, TProperty>> expression;
            private readonly string fieldName;

            internal MapBuilder(SubscriberLike<TTarget> subscriber, Expression<Func<TTarget, TProperty>> expression)
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

        private bool isMapped;
        protected readonly ISubscriber subscriber;
        protected readonly KeyValueStore<string, IReducer> valueReducers;

        public SubscriberLike(ISubscriber subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException("subscriber");

            this.subscriber = subscriber;
            this.valueReducers = new KeyValueStore<string, IReducer>();
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
            if (!isMapped)
            {
                isMapped = true;
                this.subscriber.Subscribe(new Topic { Type = typeof(TTarget).Name }, DataReceived);
            }

            return new MapBuilder<TProperty>(this, expression);
        }

        public void Listen<TProperty>(Expression<Func<TTarget, TProperty>> expression, Action<DataCandidate> callback)
        {
            var fieldName = ExpressionHelper.GetPropertyName(expression.Body);
            this.subscriber.Subscribe(new Topic { Type = typeof(TTarget).Name, Field = fieldName }, callback);
        }

        protected abstract void DataReceived(DataCandidate candidate);
    }
}
