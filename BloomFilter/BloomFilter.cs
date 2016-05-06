using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Collections.Generic
{
    /// <summary>
    /// Bloom filter implementation using SHA1 for the hash function.
    /// </summary>
    /// <typeparam name="T">The type of objects to store in the filter.</typeparam>
    public class BloomFilter<T> : BloomFilter<T, SHA1Managed>
    {
        public BloomFilter(long MaxItems, double FalsePositiveProbability = .001) : base(MaxItems, FalsePositiveProbability)
        {
        }
    }
    /// <summary>
    /// Bloom filter implementation.
    /// </summary>
    /// <typeparam name="T">The type of objects to store in the filter.</typeparam>
    /// <typeparam name="THash">The hash algorithm type to use. Must be newable and inherit from <c cref="HashAlgorithm">HashAlgorithm</c>.</typeparam>
    public class BloomFilter<T, THash> : IDisposable where THash : HashAlgorithm, new()
    {
        BitArray _bloom;
        int _bitsInFilter;
        int _numHashFunctions;
        long _itemCount;
        THash _hashFunction;

        public double FalsePositiveProbability { get; private set; }
        public long MaximumItems { get; private set; }

        /// <summary>
        /// Constructor for the bloom filter.
        /// </summary>
        /// <param name="MaxItems">Maximum number of items that can be stored in the filter.</param>
        /// <param name="FalsePositiveProbability">Probability of a false positive result for <c cref="Contains">.Contains()</c> 
        /// when the maximum number of items has been added.</param>
        public BloomFilter(long MaxItems, double FalsePositiveProbability = .001)
        {
            if (MaxItems < 1)
                throw new ArgumentException("Maximum items must be positive.");
            if (FalsePositiveProbability <= 0 || FalsePositiveProbability >= 1)
                throw new ArgumentException("False Positive Probability must be between 0 and one.");

            this.MaximumItems = MaxItems;
            this.FalsePositiveProbability = FalsePositiveProbability;

            //for reasoning behind the following calculations see: https://en.wikipedia.org/wiki/Bloom_filter
            //and http://hur.st/bloomfilter
            _bitsInFilter = (int)Math.Ceiling((MaxItems * Math.Log(FalsePositiveProbability)) / Math.Log(1.0 / (Math.Pow(2.0, Math.Log(2.0)))));
            _numHashFunctions = (int)Math.Round(Math.Log(2.0) * _bitsInFilter / this.MaximumItems);

            _bloom = new BitArray(_bitsInFilter);
            _hashFunction = new THash();
        }

        /// <summary>
        /// Check to see if an item is in the filter within the probability of false positive specified during creation.
        /// </summary>
        /// <param name="Item">The item to check for presence in the filter.</param>
        /// <returns>False if the item is not present in the filter, true if the item has been added to the filter (within false positive probability).</returns>
        public bool Contains(T Item)
        {
            var indices = computeIndices(Item);
            foreach (var index in indices)
            {
                if (!_bloom[index])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Adds an item to the bloom filter.
        /// </summary>
        /// <param name="Item">The item to be added to the filter.</param>
        public void Add(T Item)
        {
            if (_itemCount == MaximumItems)
                throw new ArgumentException(String.Format("The maximum number of items ({0}) has already been reached.", MaximumItems));
            _itemCount++;
            var indices = computeIndices(Item);
            foreach (var i in indices)
                _bloom.Set(i, true);
        }

        /// <summary>
        /// Serializes an object and prepends it with the 4 null bytes be filled in to create a unique string 
        /// to be fed to the different hashing functions.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <param name="iteration">The current iteration in the loop to prepend to the serialized byte array.</param>
        /// <returns>Byte array containing the iteration and the serialized object.</returns>
        private byte[] serializeObjectWithPadding(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var ms = new System.IO.MemoryStream())
            {
                //prepend the array with four blank bytes to randomize results of "multiple" hash functions
                ms.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// For a given object, compute the indexes for <c cref="_bloom">_bloom</c> to either set or check.
        /// </summary>
        /// <param name="Item">The item to compute the indices for.</param>
        /// <returns>An array of integers containing the indices.</returns>
        /// <remarks>Only the first four bytes of the hash function are used, so if supplying a custom hash function, 
        /// be sure it has a good avalanche effect.</remarks>
        private int[] computeIndices(T Item)
        {
            var indices = new int[_numHashFunctions];
            //serialize the object and leave space for four bytes to prepend
            byte[] bytes = serializeObjectWithPadding(Item);
            for (int i = 0; i < _numHashFunctions; i++)
            {
                //prepend the four bytes of the int for iteration
                Array.Copy(BitConverter.GetBytes(i), bytes, 4);
                byte[] hashBytes;

                hashBytes = _hashFunction.ComputeHash(bytes);
                //modulo the first four bytes (UInt32) of the hash to find the index into the bloom filter
                var index = (int)(BitConverter.ToUInt32(hashBytes, 0) % _bloom.Length);
                indices[i] = index;
            }
            return indices;
        }

        public void Dispose()
        {
            _hashFunction.Dispose();
        }
    }
}
