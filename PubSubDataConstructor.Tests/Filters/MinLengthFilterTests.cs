using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Tests.Filters
{
    [TestClass]
    public class MinLengthFilterTests
    {

        [TestMethod]
        public void MinLengthFilter_Null()
        {
            var filter = new MinLengthFilter("test", 1);

            var result = filter.Accept(new DataCandidate { TargetField = "test", Value= null });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MinLengthFilter_Shorter()
        {
            var filter = new MinLengthFilter("test", 2);
            
            var result = filter.Accept(new DataCandidate { TargetField = "test", Value = "a" });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MinLengthFilter_Equal()
        {
            var filter = new MinLengthFilter("test", 2);
            
            var result= filter.Accept(new DataCandidate { TargetField = "test", Value = "aa" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MinLengthFilter_Longer()
        {
            var filter = new MinLengthFilter("test", 2);

            var result = filter.Accept(new DataCandidate { TargetField = "test", Value = "aaa" });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MinLengthFilter_Zero()
        {
            var filter = new MinLengthFilter("test", 0);

            var result = filter.Accept(new DataCandidate { TargetField = "test", Value = "" });

            Assert.IsTrue(result);
        }
    }
}
