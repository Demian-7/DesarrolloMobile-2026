using System;
using System.Threading.Tasks;
using Game.Foundation;

namespace Game.Core
{
    /// <summary>
    /// Handles the x2 multiplier, the optional ad availability cooldown,
    /// and the UTC-based expiry so it's ready for save persistence.
    /// </summary>
    public class BoostService
    {
        private readonly IAdsService _adsService;
        private readonly ITimeService _timeService;
        private readonly IEventBus _eventBus;
        
        private readonly float _boostMultiplier;
        private readonly float _boostDurationSeconds;
        private readonly float _adCooldownSeconds;

        private DateTime? _boostExpiresAtUtc;
        private DateTime? _nextAdAvailableAtUtc;
        private bool _isRequestingAd;
        private bool _wasActive;
        
        public bool IsBoostActive => _boostExpiresAtUtc.HasValue && _timeService.UtcNow < _boostExpiresAtUtc.Value;
        public float CurrentMultiplier => IsBoostActive ? _boostMultiplier : 1f;
        public float RemainingSeconds => IsBoostActive
            ? (float)(_boostExpiresAtUtc.Value - _timeService.UtcNow).TotalSeconds
            : 0f;

        /// <summary> True when the cooldown has elapsed, no boost is active, and an ad is ready. </summary>
        public bool IsAdOfferAvailable =>
            !IsBoostActive &&
            !_isRequestingAd &&
            _timeService.UtcNow >= _nextAdAvailableAtUtc &&
            _adsService.IsRewardedAdReady;
        
        public DateTime? BoostExpiresAtUtc => _boostExpiresAtUtc;

        public BoostService(
            IAdsService adsService,
            ITimeService timeService,
            IEventBus eventBus,
            float boostMultiplier = 2f,
            float boostDurationSeconds = 120f,
            float adCooldownSeconds = 60f)
        {
            _adsService = adsService;
            _timeService = timeService;
            _eventBus = eventBus;
            _boostMultiplier = boostMultiplier;
            _boostDurationSeconds = boostDurationSeconds;
            _adCooldownSeconds = adCooldownSeconds;

            _nextAdAvailableAtUtc = _timeService.UtcNow.AddSeconds(_adCooldownSeconds);
        }

        /// <summary> Called on load to restore an active boost in real time </summary>
        public void RestoreExpiryUtc(DateTime? expiresAtUtc)
        {
            if (expiresAtUtc.HasValue && expiresAtUtc.Value > _timeService.UtcNow)
            {
                _boostExpiresAtUtc = expiresAtUtc;
                _wasActive = true;
                _eventBus.Publish(new BoostStateChangedEvent(true, _boostMultiplier, RemainingSeconds));
            }
        }

        public async Task<bool> RequestAdBoostAsync()
        {
            if (!IsAdOfferAvailable)
                return false;

            _isRequestingAd = true;
            try
            {
                bool rewarded = await _adsService.TryShowRewardedAdAsync();
                if (!rewarded)
                    return false;

                _boostExpiresAtUtc = _timeService.UtcNow.AddSeconds(_boostDurationSeconds);
                _wasActive = true;
                _eventBus.Publish(new BoostStateChangedEvent(true, _boostMultiplier, _boostDurationSeconds));
                return true;
            }
            finally
            {
                _isRequestingAd = false;
            }
        }

        public void Tick()
        {
            if (_wasActive && !IsBoostActive)
            {
                _wasActive = false;
                _boostExpiresAtUtc = null;
                _nextAdAvailableAtUtc = _timeService.UtcNow.AddSeconds(_adCooldownSeconds);
                _eventBus.Publish(new BoostStateChangedEvent(false, 1f, 0f));
            }
        }
    }
}