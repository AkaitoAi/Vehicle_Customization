using UnityEngine;

namespace AkaitoAi.Customization
{
    public abstract class Customization : MonoBehaviour
    {
        [SerializeField] private CustomizationType type;
        [SerializeField] private CustomizationSO customizationData;

        [Tooltip("If true, automatically loads and applies the saved/default item from CustomizationSO on Start.")]
        [SerializeField] private bool applyOnStart = true;

        [Tooltip("If true, automatically loads and applies the saved/default item from CustomizationSO whenever this GameObject is enabled.")]
        [SerializeField] private bool applyOnEnable = true;

        public CustomizationType Type => type;
        public CustomizationSO CustomizationData => customizationData;

        protected virtual void Start()
        {
            if (applyOnStart)
            {
                ApplyCustomization();
            }
        }

        protected virtual void OnEnable()
        {
            if (customizationData != null)
            {
                customizationData.Actions.OnItemIDChanged -= Customize;
                customizationData.Actions.OnItemIDChanged += Customize;

                if (applyOnEnable)
                {
                    ApplyCustomization();
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (customizationData != null)
            {
                customizationData.Actions.OnItemIDChanged -= Customize;
            }
        }

        protected virtual bool CanCustomize()
        {
            if (customizationData == null) return false;

            return true;
        }

        protected virtual void Customize(int itemID)
        {

        }

        /// <summary>
        /// Loads unlocked status, gets the saved or default itemID from CustomizationSO, and applies it.
        /// </summary>
        public void ApplyCustomization()
        {
            if (customizationData != null)
            {
                customizationData.LoadUnlocked();
                int currentItem = customizationData.LoadSelected();
                customizationData.itemID = currentItem;
                Customize(currentItem);
            }
        }

        /// <summary>
        /// Resets and applies the default itemID configured in the CustomizationSO.
        /// </summary>
        [ContextMenu("Apply Default Customization")]
        public void ApplyDefaultCustomization()
        {
            if (customizationData != null)
            {
                customizationData.ResetToDefault();
                Customize(customizationData.defaultItemID);
            }
        }
    }
}
