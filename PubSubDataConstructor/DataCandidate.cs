using System;

namespace PubSubDataConstructor
{
    public class DataCandidate
    {
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
    }
}
