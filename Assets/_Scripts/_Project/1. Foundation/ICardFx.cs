using DG.Tweening;

namespace Game.Foundation
{
    /// <summary>
    /// Shared surface so game code doesn't care whether a card is drawn by a UI Image or SpriteRenderer.
    /// </summary>
    public interface ICardFx
    {
        void SetTier(CardTier tier);
        void SetGlow(float value);
        void SetUnpowered(bool on, bool instant = false);
        Tween Pulse();
        Tween DissolveIn(float duration = 0.5f, float delay = 0f);
        Tween DissolveOut(float duration = 0.5f, float delay = 0f);
    }
}