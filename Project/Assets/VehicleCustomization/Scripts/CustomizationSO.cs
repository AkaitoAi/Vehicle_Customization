using UnityEngine;

namespace AkaitoAi.Customization
{
    [CreateAssetMenu(fileName = "Customization", menuName = "ScriptableObjects/Customization", order = 1)]
    public class CustomizationSO : ScriptableObject
    {
        public CustomizationType type;
        public int itemID;
        public ItemStatus[] status;
        public Prefernces Prefs;
        public Actions Actions;

        private void OnValidate()
        {
            Prefs.selected = "SelectedCustom" + type;
            Prefs.unLocked = "UnlockdCustom" + type;
        }
    }
}
