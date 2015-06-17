using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Tests.Filters
{
    [TestClass]
    public class MaxLengthFilterTests
    {

        [TestMethod]
        public void MaxLengthFilter_Null()
        {
            var filter = new MaxLengthFilter("test", 1);

            var result = filter.Accept(new DataCandidate { TargetField = "test", Value= null });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MaxLengthFilter_Shorter()
        {
            var filter = new MaxLengthFilter("test", 2);
            
            var result = filter.Accept(new DataCandidate { TargetField = "test", Value = "a" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MaxLengthFilter_Equal()
        {
            var filter = new MaxLengthFilter("test", 2);
            
            var result= filter.Accept(new DataCandidate { TargetField = "test", Value = "aa" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MaxLengthFilter_Longer()
        {
            var filter = new MaxLengthFilter("test", 2);

            var result = filter.Accept(new DataCandidate { TargetField = "test", Value = "aaa" });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MaxLengthFilter_Zero()
        {
            var filter = new MaxLengthFilter("test", 0);

            var result = filter.Accept(new DataCandidate { TargetField = "test", Value = "" });

            Assert.IsTrue(result);
        }
    }
}
