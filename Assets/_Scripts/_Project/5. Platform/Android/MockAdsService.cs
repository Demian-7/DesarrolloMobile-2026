using System.Threading.Tasks;
using Game.Foundation;
using UnityEngine;

namespace Game.Platform.Android
{
    public class MockAdsService : IAdsService
    {
        private readonly int _simulatedAdMs;

        public bool IsRewardedAdReady => true;

        public MockAdsService(int simulatedAdMs = 1000)
        {
            _simulatedAdMs = simulatedAdMs;
        }
        
        public async Task<bool> TryShowRewardedAdAsync()
        {
            Debug.Log("[MockAdsService] Showing simulated rewarded ad...");
            await Task.Delay(_simulatedAdMs);
            Debug.Log("[MockAdsService] Rewarded ad completed.");
            return true;
        }
    }
}