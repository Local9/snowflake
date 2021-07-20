using System;
using System.IO;
using JetBrains.Annotations;
using Lusive.Snowflake.Models;

namespace Lusive.Snowflake
{
    /// <summary>
    /// Represents a unique identifier based on Twitter's <see href="https://developer.twitter.com/en/docs/twitter-ids">Snowflake</see> format. Internally uses unsigned 64 bit integers.
    /// </summary>
    [PublicAPI]
    public struct SnowflakeId : IEquatable<SnowflakeId>
    {
        /// <summary>
        /// A read-only <see cref="SnowflakeId"/> whose value is zero.
        /// </summary>
        public static readonly SnowflakeId Empty = new SnowflakeId(0);

        /// <summary>
        /// Creates a new <see cref="SnowflakeId"/> based on the current timestamp and instance id.
        /// </summary>
        /// <returns>A new <see cref="SnowflakeId"/> object.</returns>
        public static SnowflakeId Next()
        {
            return SnowflakeGenerator.Instance.Next();
        }

        /// <summary>
        /// Converts the string representation of a Snowflake to the equivalent <see cref="SnowflakeId"/> structure.
        /// </summary>
        /// <param name="id">The string to convert.</param>
        /// <returns>The converted structure.</returns>
        public static SnowflakeId Parse(string id) => Parse(ulong.Parse(id));

        /// <summary>
        /// Converts the ulong representation of a Snowflake to the equivalent <see cref="SnowflakeId"/> structure.
        /// </summary>
        /// <param name="id">The integer to convert.</param>
        /// <returns>The converted structure.</returns>
        public static SnowflakeId Parse(ulong id)
        {
            return new SnowflakeId(id);
        }
        
        private readonly ulong _value;

        public SnowflakeId(ulong value)
        {
            _value = value;
        }

        public SnowflakeId(long value) : this((ulong) value)
        {
        }

        public SnowflakeId(string value)
        {
            _value = (ulong) long.Parse(value);
        }

        public SnowflakeId(BinaryReader reader)
        {
            _value = reader.ReadUInt64();
        }

        /// <summary>
        /// Deconstructs the value in this instance into <see cref="SnowflakeFragments">fragments</see>.
        /// </summary>
        /// <returns>The deconstructed values represented by a <see cref="SnowflakeFragments"/> instance.</returns>
        public SnowflakeFragments Deconstruct()
        {
            var instance = SnowflakeGenerator.Instance;

            return instance.Deconstruct((long) _value);
        }
        
        public void PackSerializedBytes(BinaryWriter writer)
        {
            writer.Write(_value);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public ulong ToInt64()
        {
            return _value;
        }

        public bool Equals(SnowflakeId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                ulong unsigned => _value == unsigned,
                long signed => _value == (ulong) signed,
                string serialized => ToString() == serialized,
                _ => obj is SnowflakeId other && Equals(other)
            };
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(SnowflakeId first, SnowflakeId second)
        {
            return first.Equals(Empty) ? second.Equals(Empty) : first.Equals(second);
        }

        public static bool operator !=(SnowflakeId first, SnowflakeId second)
        {
            return !(first == second);
        }
    }
}