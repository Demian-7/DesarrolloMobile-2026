namespace Game.Foundation
{
    public readonly struct GlobalUpgradePurchasedEvent
    {
        public readonly string UpgradeId;

        public GlobalUpgradePurchasedEvent(string upgradeId)
        {
            UpgradeId = upgradeId;
        }
    }
}