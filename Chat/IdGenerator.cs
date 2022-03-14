using System.Collections;
using System.Numerics;

namespace Chat
{
    public static class IdGenerator
    {
        private const uint timestampBitCount = 50;
        private const uint nodeNumberBitCount = 15;
        private const uint sequenceNumberBitCount = 20;

        private const ulong maxTimestampNumber = 1125899906842624; //2 ^ timestampBitCount
        private const ulong maxNodeNumber = 32768; //2 ^ nodeNumberBitCount
        private const ulong maxSequenceNumber = 1048576; //2 ^ sequenceNumberBitCount

        private static ulong? nodeNumber = 1;
        private static readonly object nextAvailableSequenceNumberLock = new object();
        private static ulong _nextAvailableSequenceNumber = 0;
        private static ulong NextAvailableSequenceNumber
        {
            get
            {
                lock (nextAvailableSequenceNumberLock)
                {
                    Interlocked.CompareExchange(ref _nextAvailableSequenceNumber, 0, maxSequenceNumber);
                    return Interlocked.Increment(ref _nextAvailableSequenceNumber);
                }
            }
        }

        public static BigInteger GenerateId()
        {
            if (nodeNumber == null)
            {
                return BigInteger.Zero;
            }
            BitArray timestampBits = GenerateIdPart(Time.UtcMillisecondsSinceEpoch(), maxTimestampNumber, timestampBitCount);
            BitArray nodeNumberBits = GenerateIdPart(Convert.ToUInt64(nodeNumber), maxNodeNumber, nodeNumberBitCount);
            BitArray sequenceNumberBits = GenerateIdPart(NextAvailableSequenceNumber, maxSequenceNumber, sequenceNumberBitCount);
            if (BitConverter.IsLittleEndian == false)
            {
                timestampBits = Reverse(timestampBits);
                nodeNumberBits = Reverse(nodeNumberBits);
                sequenceNumberBits = Reverse(sequenceNumberBits);
            }
            BitArray[] bitArrays = { sequenceNumberBits, nodeNumberBits, timestampBits }; //In little-endian. Reverse order if output must be big-endian
            BitArray idBits = CombineBitArrays(bitArrays);
            BigInteger idBigInt = ConvertToBigInteger(idBits, true, false);
            return idBigInt;
        }

        public static BitArray GenerateIdPart(ulong number, ulong maxNumber, uint bitCount)
        {
            if (number > maxNumber)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(number)}' cannot be greater than {bitCount} bits");
            }
            BitArray bitArray = ConvertToBitArray(number);
            bitArray = LimitToBitCount(bitArray, bitCount);
            return bitArray;
        }

        public static BitArray ConvertToBitArray(ulong number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            BitArray bits = new BitArray(bytes);
            return bits;
        }

        public static byte[] ConvertToBytes(BitArray bitArray)
        {
            if (bitArray == null || bitArray.Length == 0)
            {
                return new byte[0];
            }
            const int bitsPerByte = 8;
            byte[] bytes = new byte[(bitArray.Length - 1) / bitsPerByte + 1];
            bitArray.CopyTo(bytes, 0);
            return bytes;
        }

        public static BigInteger ConvertToBigInteger(BitArray bitArray, bool addSignByte, bool negative)
        {
            if (bitArray == null || bitArray.Length == 0)
            {
                return BigInteger.Zero;
            }
            BitArray bitArrayCopy = new BitArray(bitArray);
            byte[] bytes = ConvertToBytes(bitArrayCopy);
            if (addSignByte)
            {
                byte[] tempBytes = new byte[bytes.Length + 1];
                Array.Copy(bytes, tempBytes, bytes.Length);
                bytes = tempBytes;
                const int bitsPerByte = 8;
                BitArray signBit = new BitArray(bitsPerByte);
                signBit.Set(signBit.Length - 1, negative);
                byte signByte = ConvertToBytes(signBit)[0];
                bytes[bytes.Length - 1] = signByte;
            }
            BigInteger bigInteger = new BigInteger(bytes);
            return bigInteger;
        }

        public static BitArray LimitToBitCount(BitArray bitArray, uint bitCount)
        {
            bitArray.Length = Convert.ToInt32(bitCount);
            return bitArray;
        }

        public static BitArray CombineBitArrays(BitArray[] bitArrays)
        {
            int bitLength = 0;
            foreach (BitArray bitArray in bitArrays)
            {
                bitLength += bitArray.Length;
            }
            BitArray combinedBitArray = new BitArray(bitLength);
            int currentIndex = 0;
            foreach (BitArray bitArray in bitArrays)
            {
                for (int i = 0; i < bitArray.Length; i++)
                {
                    combinedBitArray.Set(currentIndex++, bitArray[i]);
                }
            }
            return combinedBitArray;
        }

        public static BitArray Reverse(BitArray bitArray)
        {
            BitArray reversedBitArray = new BitArray(bitArray.Length);
            for (int i = bitArray.Length - 1; i >= 0; i--)
            {
                reversedBitArray.Set(bitArray.Length - i - 1, bitArray[i]);
            }
            return reversedBitArray;
        }
    }
}
