using UnityEngine;

namespace Game.Foundation
{
    [CreateAssetMenu(menuName = "IdleGenerator/Card Family Definition")]
    public class CardFamilyDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
    }
}