using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PubSubDataConstructor.Utils
{
    internal static class TopicHelper
    {
        private const int TOPIC_PARTS = 3;

        public static bool IsMatch(string filter, string topic)
        {
            if (filter == null)
                return false;

            if (topic == null)
                return false;

            var filterParts = filter.Split(new[] { '.' });
            if (filterParts.Length != TOPIC_PARTS)
                throw new InvalidOperationException("Filter must have 3 parts. (Received: " + filter + ")");

            var topicParts = topic.Split(new[] { '.' });
            if (topicParts.Length != TOPIC_PARTS)
                throw new InvalidOperationException("Topic must have 3 parts. (Received: " + topic + ")");

            for (var i = 0; i < 3; i++)
            {
                if (filterParts[i] != "*" && filterParts[i] != topicParts[i])
                    return false;
            }

            return true;
        }

        public static bool IsMatch(Topic filter, Topic topic)
        {
            if (filter == null)
                return false;

            if (topic == null)
                return false;

            return (filter.Type == null ||  topic.Type == filter.Type)
                && (filter.Id == null || topic.Id == filter.Id)
                && (filter.Field == null || topic.Field == filter.Field);
        }
    }
}
