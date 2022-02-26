namespace Chat
{
    public static class Time
    {
        const ulong EpochMillisecond = 63776592000000; //01/01/2022 12:00AM UTC
        const ulong TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
        const ulong EpochTick = EpochMillisecond * TicksPerMillisecond;

        public static ulong UtcTicksNow()
        {
            return Convert.ToUInt64(DateTime.UtcNow.Ticks);
        }

        public static ulong UtcTicksSinceEpoch()
        {
            return UtcTicksNow() - EpochTick;
        }

        public static ulong UtcMillisecondsNow()
        {
            return UtcTicksNow() / TicksPerMillisecond;
        }

        public static ulong UtcMillisecondsSinceEpoch()
        {
            return UtcMillisecondsNow() - EpochMillisecond;
        }
    }
}
