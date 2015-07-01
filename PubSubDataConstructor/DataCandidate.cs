using System;

namespace PubSubDataConstructor
{
    public class DataCandidate
    {
        public int Id { get { return GetHashCode(); } }

        public string SourceId { get; set; }
        public string SourceField { get; set; }
        public string TargetType { get; set; }
        public string TargetId { get; set; }
        public string TargetField { get; set; }

        public byte Priority { get; set; }
        public DateTime Freshness { get; set; }

        public object Value { get; set; }

        public Topic ToTopic()
        {
            return new Topic { Type = TargetType, Id = TargetId, Field = TargetField };
        }

        public override bool Equals(object obj)
        {
            var other = obj as DataCandidate;
            if (other == null)
                return false;

            return string.Equals(SourceId, other.SourceId, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(SourceField, other.SourceField, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(TargetType, other.TargetType, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(TargetId, other.TargetId, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(TargetField, other.TargetField, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                if (SourceId != null)
                    hash = hash * 16777619 ^ SourceId.GetHashCode();

                if (SourceField != null)
                    hash = hash * 16777619 ^ SourceField.GetHashCode();

                if (TargetType != null)
                    hash = hash * 16777619 ^ TargetType.GetHashCode();

                if (TargetId != null)
                    hash = hash * 16777619 ^ TargetId.GetHashCode();

                if (TargetField != null)
                    hash = hash * 16777619 ^ TargetField.GetHashCode();

                return hash;
            }
        }
    }
}
