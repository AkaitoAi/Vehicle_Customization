using System;

namespace AkaitoAi.Customization
{
    public enum CustomizationType
    {
        Color,
        Decal,
        Glass,
        Light,
        Wheel,
        Rim,
        Spoiler,
        Flag
    }

    public struct Actions
    {

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
