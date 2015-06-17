using System;

namespace PubSubDataConstructor.Filters
{
    public class CustomFilter : IFilter
    {
        private readonly Func<DataCandidate, bool> expression;

        public CustomFilter(Func<DataCandidate, bool> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            this.expression = expression;
        }

        public bool Accept(DataCandidate candidate)
        {
            return expression.Invoke(candidate);
        }
    }

    public class CustomFieldFilter : FieldFilter
    {
        private readonly Func<object, bool> expression;

        public CustomFieldFilter(string field, Func<object, bool> expression) : base(field)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            this.expression = expression;
        }

        protected override bool Accept(object value)
        {
            return expression.Invoke(value);
        }
    }

    public class CustomFieldFilter<T> : FieldFilter<T>
    {
        private readonly Func<T, bool> expression;

        public CustomFieldFilter(string field, Func<T, bool> expression): base(field)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            this.expression = expression;
        }

        protected override bool Accept(T value)
        {
            return expression.Invoke(value);
        }
    }
}
