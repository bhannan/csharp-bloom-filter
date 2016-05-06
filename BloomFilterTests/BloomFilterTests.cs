using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace BloomFilterTests
{
    [TestClass]
    public class BloomFilterTests
    {
        [TestMethod]
        public void Empty_Bloom_Filter_Contains_Nothing()
        {
            var bf = new BloomFilter<string>(MaxItems: 100, FalsePositiveProbability: .001);
            Assert.IsFalse(bf.Contains("Dummy Value"));
        }

        [TestMethod]
        public void One_Item_Contains_Returns_True()
        {
            var bf = new BloomFilter<string>(MaxItems: 100);
            bf.Add("Test String");
            Assert.IsTrue(bf.Contains("Test String"));
          
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The maximum number of items (1) has already been reached.")]
        public void Cant_Add_More_Than_Max_Items()
        {
            var bf = new BloomFilter<string>(MaxItems: 1, FalsePositiveProbability: .001);
            bf.Add("Test String");
            bf.Add("Test String 2");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Maximum items must be positive.")]
        public void Maximum_Items_Must_Be_Positive()
        {
            var bf = new BloomFilter<string>(MaxItems: -1, FalsePositiveProbability: .001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "False Positive Probability must be between 0 and one.")]
        public void False_Positive_Probability_Must_Be_Greater_Than_Zero()
        {
            var bf = new BloomFilter<string>(MaxItems: 1, FalsePositiveProbability: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "False Positive Probability must be between 0 and one.")]
        public void False_Positive_Probability_Must_Be_Less_Than_One()
        {
            var bf = new BloomFilter<string>(MaxItems: 1, FalsePositiveProbability: 1);
        }

        [TestMethod]
        public void Serialize_Object_With_Padding_Returns_Correct_Length_And_Padding_Values()
        {
            var bfbase = new BloomFilter<string>(MaxItems: 100);
            var bf = new PrivateObject(bfbase, new PrivateType(typeof(BloomFilter<string, SHA1Managed>)));
            Int32 i = 1;
            byte[] ba = (byte[])bf.Invoke("serializeObjectWithPadding", new object[] { i });
            Assert.AreEqual(ba.Length, 58);
            Assert.AreEqual(ba[0], 0x00);
            Assert.AreEqual(ba[1], 0x00);
            Assert.AreEqual(ba[2], 0x00);
            Assert.AreEqual(ba[3], 0x00);
        }

        [TestMethod]
        public void Serialize_Object_With_Padding_Returns_Null_When_Given_Null()
        {
            var bfbase = new BloomFilter<string>(MaxItems: 100);
            var bf = new PrivateObject(bfbase, new PrivateType(typeof(BloomFilter<string, SHA1Managed>)));
            object obj = (byte[])bf.Invoke("serializeObjectWithPadding", new object[] { null });
            Assert.IsNull(obj);
        }

        [TestMethod]
        public void Compute_Indices_Returns_Correct_Values_For_SHA1_Integer_One()
        {
            var bfbase = new BloomFilter<int>(MaxItems: 1);
            var bf = new PrivateObject(bfbase, new PrivateType(typeof(BloomFilter<int, SHA1Managed>)));
            int[] ba = (int[])bf.Invoke("computeIndices", new object[] { 1 });
            Assert.AreEqual(ba.Length, 10);
            Assert.AreEqual(ba[0], 12);
            Assert.AreEqual(ba[1], 8);
            Assert.AreEqual(ba[2], 8);
            Assert.AreEqual(ba[3], 7);
            Assert.AreEqual(ba[4], 9);
            Assert.AreEqual(ba[5], 13);
            Assert.AreEqual(ba[6], 0);
            Assert.AreEqual(ba[7], 5);
            Assert.AreEqual(ba[8], 1);
            Assert.AreEqual(ba[9], 11);
        }

        bool FalsePositiveRateIsCorrect(int itemCount, double falsePositiveProbability)
        {
            using (var bf = new BloomFilter<int>(itemCount, falsePositiveProbability))
            {
                for (var i = 0; i < itemCount; i++)
                {
                    bf.Add(i);
                }
                //check the same number of known false values using contains()
                var falsePositives = 0;
                for (var j = itemCount; j < itemCount * 2; j++)
                {
                    if (bf.Contains(j))
                        falsePositives++;
                }
                Console.WriteLine((double)falsePositives / itemCount);
                //we don't have perfect hashing, allow double the false positive probability...
                //I'm more concerned with order of magnitude errors
                return ((double)falsePositives / itemCount <= (falsePositiveProbability * 2));
            }
        }

        [TestMethod]
        public void False_Positive_Probability_For_10000_Items_And_Point_01()
        {
            Assert.IsTrue(FalsePositiveRateIsCorrect(10000, .01));
        }

        [TestMethod]
        public void False_Positive_Probability_For_10000_Items_And_Point_001()
        {
            Assert.IsTrue(FalsePositiveRateIsCorrect(10000, .001));
        }


        [TestMethod]
        public void False_Positive_Probability_For_10000_Items_And_Point_0001()
        {
            Assert.IsTrue(FalsePositiveRateIsCorrect(10000, .0001));
        }

        [TestMethod]
        public void False_Positive_Probability_For_100000_Items_And_Point_1()
        {
            Assert.IsTrue(FalsePositiveRateIsCorrect(100000, .1));
        }
        
        [TestMethod]
        public void False_Positive_Probability_For_100000_Items_And_Point_01()
        {
            Assert.IsTrue(FalsePositiveRateIsCorrect(100000, .01));
        }

        [TestMethod]
        public void False_Positive_Probability_For_100000_Items_And_Point_001()
        {
            Assert.IsTrue(FalsePositiveRateIsCorrect(100000, .001));
        }

        [TestMethod]
        public void False_Positive_Probability_For_1000000_Items_And_Point_001()
        {
            Assert.IsTrue(FalsePositiveRateIsCorrect(1000000, .001));
        }
    }
}
