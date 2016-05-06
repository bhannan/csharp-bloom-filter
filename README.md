# csharp-bloom-filter
###Generic Bloom Filter implementation in C#

####Example usage:
```C#
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
```

####Notes:

- The default implementation uses SHA1, which can slower as it's a cryptographic hash function.  There
is an implementation of FNV1a32 which is faster.  This may or may not be an issue depending on how
frequently you are inserting/checking items (~30 seconds for 2 million inserts/checks on my machine
with SHA1, ~15 seconds for the same with FNV1a32).

- Objects are converted to a byte array with `BinaryFormatter.Serialize()`.  Objects that
differ only by non-serializable properties are considered equivalent.