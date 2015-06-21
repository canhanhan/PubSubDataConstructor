namespace PubSubDataConstructor
{
    public class Topic
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Field { get; set; }

        public static explicit operator string(Topic topic)
        {
            return string.Format("{0}.{1}.{2}", topic.Type ?? "*", topic.Id ?? "*", topic.Field ?? "*");
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var target = obj as Topic;
            if (target == null)
                return false;

            return object.Equals(this.Type, target.Type)
                && object.Equals(this.Id, target.Id)
                && object.Equals(this.Field, target.Field);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                if (this.Type != null)
                    hash = hash * 16777619 ^ this.Type.GetHashCode();

                if (this.Id != null)
                    hash = hash * 16777619 ^ this.Id.GetHashCode();

                if (this.Field != null)
                    hash = hash * 16777619 ^ this.Field.GetHashCode();

                return hash;
            }            
        }
    }

}
