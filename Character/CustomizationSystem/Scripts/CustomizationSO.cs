using UnityEngine;

namespace AkaitoAi.Customization
{
    [CreateAssetMenu(fileName = "Customization", menuName = "ScriptableObjects/Customization", order = 1)]
    public class CustomizationSO : ScriptableObject
    {
        public int id;
        public CustomizationType Type;
        [Tooltip("Default item index to equip when no saved preference exists.")]
        public int defaultItemID = 0;
        public int itemID;
        public ItemStatus[] Status;
        public Prefernces Prefs;
        public Actions Actions;

        public string SelectedPrefKey => string.IsNullOrEmpty(Prefs.selected) ? ("SelectedCustom" + Type + id) : Prefs.selected;
        public string UnlockedPrefKey => string.IsNullOrEmpty(Prefs.unLocked) ? ("UnlockdCustom" + Type + id) : Prefs.unLocked;

        private void OnValidate()
        {
            Prefs.selected = "SelectedCustom" + Type + id;
            Prefs.unLocked = "UnlockdCustom" + Type + id;
        }

        private void OnEnable()
        {
            LoadUnlocked();
            itemID = LoadSelected();
        }

        public void SaveSelected()
        {
            PlayerPrefs.SetInt(SelectedPrefKey, itemID);
        }

        public void LoadUnlocked()
        {
            if (Status == null) return;

            for (int i = 0; i < Status.Length; i++)
            {
                // If it was already unlocked by default in inspector or saved in PlayerPrefs
                if (Status[i].isUnlocked || PlayerPrefs.GetInt(UnlockedPrefKey + "_" + i, 0) == 1 || PlayerPrefs.GetInt(UnlockedPrefKey, -1) == i)
                {
                    Status[i].isUnlocked = true;
                }
            }

            // Always ensure default item is unlocked
            if (defaultItemID >= 0 && defaultItemID < Status.Length)
            {
                Status[defaultItemID].isUnlocked = true;
            }
        }

        public void SaveUnlocked()
        {
            SaveUnlocked(itemID);
        }

        public void SaveUnlocked(int index)
        {
            if (Status != null && index >= 0 && index < Status.Length)
            {
                Status[index].isUnlocked = true;
                PlayerPrefs.SetInt(UnlockedPrefKey + "_" + index, 1);
            }
        }

        public int LoadSelected()
        {
            return PlayerPrefs.GetInt(SelectedPrefKey, defaultItemID);
        }
        
        private int CheckUnlocked()
        {
            return PlayerPrefs.GetInt(UnlockedPrefKey, itemID);
        }

        [ContextMenu("Reset To Default")]
        public void ResetToDefault()
        {
            itemID = defaultItemID;
            SaveSelected();
            if (Status != null && defaultItemID >= 0 && defaultItemID < Status.Length)
            {
                Status[defaultItemID].isUnlocked = true;
                SaveUnlocked(defaultItemID);
            }
            Actions.OnItemIDChanged?.Invoke(itemID);
        }

        [ContextMenu("Clear Saved Prefs (Reset To Default)")]
        public void ClearSavedPrefs()
        {
            PlayerPrefs.DeleteKey(SelectedPrefKey);
            if (Status != null)
            {
                for (int i = 0; i < Status.Length; i++)
                {
                    PlayerPrefs.DeleteKey(UnlockedPrefKey + "_" + i);
                }
            }
            PlayerPrefs.DeleteKey(UnlockedPrefKey);
            itemID = defaultItemID;
            Actions.OnItemIDChanged?.Invoke(itemID);
            Debug.Log($"[{name}] Cleared saved PlayerPrefs. Current itemID set to default: {defaultItemID}");
        }
    }
}
