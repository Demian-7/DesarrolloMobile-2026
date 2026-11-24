using System;
using Game.Foundation;

namespace Game.Platform.Android
{
    public class SystemTimeService : ITimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}