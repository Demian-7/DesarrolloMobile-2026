using System;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public class AdBoostButtonView : MonoBehaviour
    {
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private GameObject _offerRoot;
        [SerializeField] private GameObject _activeTimerRoot;
        [SerializeField] private TextMeshProUGUI _timerText;
        
        private BoostService _boostService;

        public void Initialize(BoostService boostService)
        {
            _boostService = boostService;
            
            if (_watchAdButton != null)
                _watchAdButton.onClick.AddListener(OnWatchAdClicked);

            Refresh();
        }

        private void OnDestroy()
        {
            if (_watchAdButton != null)
                _watchAdButton.onClick.RemoveListener(OnWatchAdClicked);
        }

        private void Update()
        {
            if (_boostService == null)
                return;

            Refresh();
        }

        private async void OnWatchAdClicked()
        {
            if (_watchAdButton != null)
                _watchAdButton.interactable = false;

            await _boostService.RequestAdBoostAsync();
            Refresh();
        }

        private void Refresh()
        {
            if (_offerRoot != null)
                _offerRoot.SetActive(_boostService.IsAdOfferAvailable);

            if (_watchAdButton != null)
                _watchAdButton.interactable = _boostService.IsAdOfferAvailable;
            
            if (_activeTimerRoot != null)
                _activeTimerRoot.SetActive(_boostService.IsBoostActive);

            if (_boostService.IsBoostActive && _timerText != null)
            {
                int totalSeconds = Mathf.CeilToInt(_boostService.RemainingSeconds);
                _timerText.text = $"x2 {totalSeconds / 60:00}:{totalSeconds % 60:00}";
            }
        }
    }
}