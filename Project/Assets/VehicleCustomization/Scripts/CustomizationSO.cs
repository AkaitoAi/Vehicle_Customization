using UnityEngine;

namespace AkaitoAi.Customization
{
    [CreateAssetMenu(fileName = "Customization", menuName = "ScriptableObjects/Customization", order = 1)]
    public class CustomizationSO : ScriptableObject
    {
        public int id;
        public CustomizationType Type;
        public int itemID;
        public ItemStatus[] Status;
        public Prefernces Prefs;
        public Actions Actions;

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
            PlayerPrefs.SetInt(Prefs.selected, itemID);
        }

        public void LoadUnlocked()
        {
            for (int i = 0; i < Status.Length; i++)
            {
                if (CheckUnlocked() == i)
                {
                    Status[i].isUnlocked = true;
                }
            }
        }

        public void SaveUnlocked()
        {
            PlayerPrefs.SetInt(Prefs.unLocked, itemID);
        }

        public int LoadSelected()
        {
            return PlayerPrefs.GetInt(Prefs.selected, itemID);
        }
        
        private int CheckUnlocked()
        {
            return PlayerPrefs.GetInt(Prefs.unLocked, itemID);
        }
    }
}
