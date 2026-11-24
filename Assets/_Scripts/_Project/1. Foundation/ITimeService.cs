using System;

namespace Game.Foundation
{
    public interface ITimeService
    {
        DateTime UtcNow { get; }
    }
}