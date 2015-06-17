using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Reducers;
using PubSubDataConstructor.Strategies;
using System;
using System.Linq.Expressions;

namespace PubSubDataConstructor.Subscribers
{
    class FluentSubscriber<TTarget> : Subscriber
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

                subscriber.Strategy.Add(fieldName, reducer);

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

        protected IStrategy Strategy { get; set; }

        public FluentSubscriber(IChannel channel) : base(channel) {}
        
        public void Filter(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            AddFilter(filter);
        }

        public void Filter(Func<DataCandidate, bool> filter)
        {
            Filter(new CustomFilter(filter));
        }

        public void SetStrategy(IStrategy strategy)
        {
            var topic = typeof(TTarget).Name + ".*";
            
            if (Strategy != null)
                Unsubscribe(topic);

            Strategy = strategy;

            Subscribe(topic, Strategy);
        }

        public void LoadAndBuild(LoadAndBuildStrategy.LoadDelegate loadCallback)
        {
            this.SetStrategy(new LoadAndBuildStrategy(loadCallback));
        }

        public MapBuilder<TProperty> Map<TProperty>(Expression<Func<TTarget, TProperty>> expression)
        {
            var builder = new MapBuilder<TProperty>(this, expression);

            return builder;
        }        
    }
}
