using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Tests.Filters
{
    [TestClass]
    public class BlankFilterTests 
    {
        private const string FIELD = "TestField";

        [TestMethod]
        public void BlankFilter_ReferenceType_Null_DifferentField_ReturnsTrue()
        {
            var filter = new BlankFilter(FIELD);

            var result = filter.Accept(new DataCandidate { TargetField = "DifferentField", Value = null });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void BlankFilter_ReferenceType_Null_ReturnsFalse()
        {
            var filter = new BlankFilter(FIELD);

            var result = filter.Accept(new DataCandidate { TargetField = FIELD, Value = null });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BlankFilter_ReferenceType_NonNull_ReturnsTrue()
        {
            var filter = new BlankFilter(FIELD);

            var result = filter.Accept(new DataCandidate { TargetField = FIELD, Value = "NonNullValue" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void BlankFilter_ValueType_Default_ReturnsFalse()
        {
            var filter = new BlankFilter(FIELD);

            var result = filter.Accept(new DataCandidate { TargetField = FIELD, Value = default(int) });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BlankFilter_ValueType_NonDefault_ReturnsTrue()
        {
            var filter = new BlankFilter(FIELD);

            var result = filter.Accept(new DataCandidate { TargetField = FIELD, Value = 1 });

            Assert.IsTrue(result);
        }
    }
}
