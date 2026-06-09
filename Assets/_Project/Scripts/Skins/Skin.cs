using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// A cosmetic skin for one <see cref="SkinSlot"/>, authored as a ScriptableObject asset
    /// under Resources/Skins. Most slots use a single <see cref="Sprite"/>; the
    /// <see cref="SkinSlot.BunSkin"/> slot also uses <see cref="SecondarySprite"/> for the bottom bun.
    /// </summary>
    [CreateAssetMenu(fileName = "Skin", menuName = "Dogtor Burguer/Skin")]
    public class Skin : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private SkinSlot _slot;
        [Tooltip("The base skin served when no other is selected/owned for this slot.")]
        [SerializeField] private bool _isDefault;

        [Header("Art")]
        [SerializeField] private Sprite _sprite;
        [Tooltip("Bun slot only: the bottom-bun sprite. Ignored by every other slot.")]
        [SerializeField] private Sprite _secondarySprite;
        [Tooltip("Shown in the skin-selection UI. Falls back to the main sprite if unset.")]
        [SerializeField] private Sprite _preview;

        [Header("Unlock")]
        [SerializeField] private UnlockMethod _unlock = UnlockMethod.Free;
        [SerializeField] private int _gemCost;

        public string Id => _id;
        public string DisplayName => _displayName;
        public SkinSlot Slot => _slot;
        public bool IsDefault => _isDefault;
        public Sprite Sprite => _sprite;
        public Sprite SecondarySprite => _secondarySprite;
        public Sprite Preview => _preview != null ? _preview : _sprite;
        public UnlockMethod Unlock => _unlock;
        public int GemCost => _gemCost;
    }
}
