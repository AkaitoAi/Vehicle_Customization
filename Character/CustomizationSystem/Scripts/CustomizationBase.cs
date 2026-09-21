using System;

namespace AkaitoAi.Customization
{
    public enum CustomizationType
    {
        Head,
        Glasses,
        Mask,
        Shirt,
        Pant,
        Shoes
    }

    public struct Actions
    {
        public Action OnEntered;
        public Action<int> OnItemIDChanged;
    }

    [Serializable]
    public struct Prefernces
    {
        public string selected;
        public string unLocked;
    }

    [Serializable]
    public struct ItemStatus
    {
        public bool isUnlocked;
        public int price;
    }
}
