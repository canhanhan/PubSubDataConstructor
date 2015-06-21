using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Tests.Utils
{
    [TestClass]
    public class TopicHelperTests
    {
        [TestMethod]
        public void TopicHelper_IsMatch_ThreeMatchingArgument()
        {
            var filter = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MatchingTypeAndId()
        {
            var filter = new Topic { Type = "Type1", Id = "Id1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MatchingTypeAndField()
        {
            var filter = new Topic { Type = "Type1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MatchingIdAndField()
        {
            var filter = new Topic { Id = "Id1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MatchingType()
        {
            var filter = new Topic { Type = "Type1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));        
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MatchingField()
        {
            var filter = new Topic { Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MatchingId()
        {
            var filter = new Topic { Id = "Id1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingType()
        {
            var filter = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };
            var topic = new Topic { Type = "DifferentType", Id = "Id1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingTypeWithoutId()
        {
            var filter = new Topic { Type = "Type1", Field = "Field1" };
            var topic = new Topic { Type = "DifferentType", Id = "Id1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingTypeWithoutField()
        {
            var filter = new Topic { Type = "Type1", Id = "Id1" };
            var topic = new Topic { Type = "DifferentType", Id = "Id1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingTypeWithoutFieldAndId()
        {
            var filter = new Topic { Type = "Type1" };
            var topic = new Topic { Type = "DifferentType", Id = "Id1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingId()
        {
            var filter = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "DifferentId1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingIdWithoutType()
        {
            var filter = new Topic { Id = "Id1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "DifferentId1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingIdWithoutField()
        {
            var filter = new Topic { Type = "Type1", Id = "Id1" };
            var topic = new Topic { Type = "Type1", Id = "DifferentId1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingIdWithoutFieldAndType()
        {
            var filter = new Topic { Id = "Id1" };
            var topic = new Topic { Type = "Type1", Id = "DifferentId1", Field = "Field1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingField()
        {
            var filter = new Topic { Type = "Type1", Id = "Id1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "DifferentField1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingFieldWithoutType()
        {
            var filter = new Topic { Id = "Id1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "DifferentField1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingFieldWithoutId()
        {
            var filter = new Topic { Type = "Type1", Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "DifferentField1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_MismatchingFieldWithoutIdAndType()
        {
            var filter = new Topic { Field = "Field1" };
            var topic = new Topic { Type = "Type1", Id = "Id1", Field = "DifferentField1" };

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_ThreeMatching()
        {
            var filter = "Type1.Id1.Field1";
            var topic = "Type1.Id1.Field1";

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_MatchingTypeAndId()
        {
            var filter = "Type1.Id1.*";
            var topic = "Type1.Id1.Field1";

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_MatchingTypeAndField()
        {
            var filter = "Type1.*.Field1";
            var topic = "Type1.Id1.Field1";

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_MatchingType()
        {
            var filter = "Type1.*.*";
            var topic = "Type1.Id1.Field1";

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_MatchingIdAndField()
        {
            var filter = "*.Id1.Field1";
            var topic = "Type1.Id1.Field1";

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_MatchingField()
        {
            var filter = "*.*.Field1";
            var topic = "Type1.Id1.Field1";

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_Wildcard()
        {
            var filter = "*.*.*";
            var topic = "Type1.Id1.Field1";

            Assert.IsTrue(TopicHelper.IsMatch(filter, topic));
        }


        [TestMethod]
        public void TopicHelper_IsMatch_String_TypeMismatch()
        {
            var filter = "Type1.Id1.Field1";
            var topic = "DifferentType1.Id1.Field1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_TypeMismatchWithoutId()
        {
            var filter = "Type1.*.Field1";
            var topic = "DifferentType1.Id1.Field1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_TypeMismatchWithoutType()
        {
            var filter = "Type1.Id1.*";
            var topic = "DifferentType1.Id1.Field1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_TypeMismatchWithoutIdAndField()
        {
            var filter = "Type1.*.*";
            var topic = "DifferentType1.Id1.Field1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_IdMismatch()
        {
            var filter = "Type1.Id1.Field1";
            var topic = "Type1.DifferentId1.Field1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_IdMismatchWithoutField()
        {
            var filter = "Type1.Id1.*";
            var topic = "Type1.DifferentId1.Field1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_IdMismatchWithoutTypeAndField()
        {
            var filter = "*.Id1.*";
            var topic = "Type1.DifferentId1.Field1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_FieldMismatch()
        {
            var filter = "Type1.Id1.Field1";
            var topic = "Type1.Id1.DifferentField1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_FieldMismatchWithoutId()
        {
            var filter = "Type1.*.Field1";
            var topic = "Type1.Id1.DifferentField1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_FieldMismatchWithoutType()
        {
            var filter = "*.Id1.Field1";
            var topic = "Type1.Id1.DifferentField1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }

        [TestMethod]
        public void TopicHelper_IsMatch_String_FieldMismatchWithoutTypeAndId()
        {
            var filter = "*.*.Field1";
            var topic = "Type1.Id1.DifferentField1";

            Assert.IsFalse(TopicHelper.IsMatch(filter, topic));
        }
    }
}
