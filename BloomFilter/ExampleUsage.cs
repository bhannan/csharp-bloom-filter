using System.Collections.Generic;
using System.Security.Cryptography;

namespace BloomFilter
{
    class ExampleUsage
    {
        static void Main(string[] args)
        {
            //standard usage using SHA1 hashing
            using (var bf = new BloomFilter<string>(MaxItems: 1000, FalsePositiveProbability: .001))
            {
                //add an item to the filter
                bf.Add("My Text");

                //checking for existence in the filter
                bool b;
                b = bf.Contains("My Text"); //true
                b = bf.Contains("Never been seen before"); //false (usually ;))
            }

            //using a different hash algorithm (such as the provided FNV1a-32 implementation)
            using (var bf = new BloomFilter<string, FNV1a32>(MaxItems: 1000, FalsePositiveProbability: .001))
            {
                //add, check for existence, etc.
            }
        }
    }
}
