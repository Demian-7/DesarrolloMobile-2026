using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Foundation;

namespace Game.Core
{
    public interface IGlobalUpgradeEffect { }

    [Serializable] public class GenerationMultiplierEffect : IGlobalUpgradeEffect { public float Multiplier; }
    [Serializable] public class ConcurrentZoneSimulationEffect : IGlobalUpgradeEffect { }
    [Serializable] public class MultiplicativeStackingEffect : IGlobalUpgradeEffect { }

    [CreateAssetMenu(menuName = "IdleGenerator/Global Upgrade Definition")]
    public class GlobalUpgradeDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public CurrencyValue Cost;

        [SerializeReference]
        public IGlobalUpgradeEffect Effect;
    }

    public class GlobalUpgradeService
    {
        private readonly IEventBus _eventBus;
        private readonly HashSet<IGlobalUpgradeEffect> _activeEffects = new();

        public GlobalUpgradeService(IEventBus eventBus) => _eventBus = eventBus;

        public bool TryPurchase(GlobalUpgradeDefinition upgrade, CurrencyWallet currencySource)
        {
            if (!currencySource.TrySpend(upgrade.Cost))
                return false;

            _activeEffects.Add(upgrade.Effect);
            _eventBus.Publish(new GlobalUpgradePurchasedEvent(upgrade.Id));
            return true;
        }

        public bool HasEffect<T>() where T : IGlobalUpgradeEffect
        {
            foreach (var effect in _activeEffects)
                if (effect is T) return true;
            return false;
        }
    }
}