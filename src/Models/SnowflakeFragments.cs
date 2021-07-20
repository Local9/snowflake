using JetBrains.Annotations;

namespace Lusive.Snowflake.Models
{
    [PublicAPI]
    public class SnowflakeFragments
    {
        public long Timestamp { get; set; }
        public long Instance { get; set; }
        public long Sequence { get; set; }
    }
}