using System.Threading.Tasks;

namespace Game.Foundation
{
    public interface IAdsService
    {
        bool IsRewardedAdReady { get; }
        Task<bool> TryShowRewardedAdAsync();
    }
}