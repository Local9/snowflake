using System;
using JetBrains.Annotations;
using Lusive.Snowflake.Models;

namespace Lusive.Snowflake
{
    /// <summary>
    /// Represents the internal logic of generating unique <see cref="SnowflakeId">Snowflake</see> identifiers.
    /// </summary>
    [PublicAPI]
    public class SnowflakeGenerator
    {
        public SnowflakeConfiguration Configuration { get; set; }
        public long Timestamp => (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

        private readonly long _maskSequence;
        private readonly long _maskTime;
        private readonly long _maskInstance;
        private readonly int _shiftTime;
        private readonly int _shiftInstance;
        private readonly object _lock = new object();
        private long _instanceId;
        private long _sequence;
        private long _lastTimeslot;

        /// <summary>
        /// Creates a new generator instance with a supplied instance id.
        /// </summary>
        /// <param name="instance">A unique instance ("worker") identifier, typically sequential to each process. If you have multiple processes, this ensures that zero collisions occur.</param>
        /// <returns>A new <see cref="SnowflakeGenerator"/> object.</returns>
        public static SnowflakeGenerator Create(short instance)
        {
            var value = new SnowflakeGenerator(instance, new SnowflakeConfiguration());

            _singletonInstance ??= value;

            return value;
        }

        public SnowflakeGenerator(short instance, SnowflakeConfiguration configuration)
        {
            Configuration = configuration;

            _instanceId = instance;
            _maskTime = GetMask(configuration.TimestampBits);
            _maskInstance = GetMask(configuration.InstanceBits);
            _maskSequence = GetMask(configuration.SequenceBits);
            _shiftTime = configuration.InstanceBits + configuration.SequenceBits;
            _shiftInstance = configuration.SequenceBits;
        }

        public SnowflakeId Next(long time)
        {
            lock (_lock)
            {
                var timestamp = time & _maskTime;

                if (_lastTimeslot == timestamp)
                {
                    if (_sequence >= _maskSequence)
                    {
                        while (_lastTimeslot == Timestamp)
                        {
                        }
                    }

                    _sequence++;
                }
                else
                {
                    _lastTimeslot = timestamp;
                    _sequence = 0;
                }

                return new SnowflakeId((timestamp << _shiftTime) + (_instanceId << _shiftInstance) + _sequence);
            }
        }

        public SnowflakeId Next() => Next(Timestamp);

        public SnowflakeFragments Deconstruct(long value)
        {
            var fragments = new SnowflakeFragments
            {
                Sequence = value & _maskSequence,
                Instance = (value >> _shiftInstance) & _maskInstance,
                Timestamp = (value >> _shiftTime) & _maskTime
            };

            return fragments;
        }

        private long GetMask(byte bits) => (1L << bits) - 1;

        public static SnowflakeGenerator Instance
        {
            get
            {
                if (_singletonInstance == null)
                    throw new Exception(
                        "A Snowflake generator was not initialized yet. See the 'SnowflakeGenerator.Create' static method.");

                return _singletonInstance;
            }
        }

        private static SnowflakeGenerator _singletonInstance;
    }
}