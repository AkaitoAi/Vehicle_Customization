using System;
using UnityEngine;

namespace AkaitoAi.Customization
{
    public class ApplyGameObjectToggleCustomization : Customization
    {
        [SerializeField] private Transform containerTransform;

        protected override void Customize(int itemID)
        {     
            if (!CanCustomize()) return;

            base.Customize(itemID);

            if (containerTransform == null)
            {
                Debug.Log("Container not set");
                
                return;
            }

            if (itemID >= CustomizationData.Status.Length 
                || itemID < 0)
            {
                Debug.Log("itemID is out of range.");
                
                return;
            }

            int slotCount = containerTransform.childCount;
            int typeCount = CustomizationData.Status.Length;
            int totalItems = slotCount * typeCount;

            Transform wheelsTransform = containerTransform;

            for (int i = 0; i < totalItems; i++)
            {
                Transform wheelSlot = wheelsTransform.GetChild(i / typeCount);
                GameObject wheel = wheelSlot.GetChild(i % typeCount).gameObject;
                wheel.SetActive(i % typeCount == itemID);
            }
        }

        private void OnEnable()
        {
            CustomizationData.Actions.OnItemIDChanged += Customize;
        }

        private void OnDisable()
        {
            CustomizationData.Actions.OnItemIDChanged -= Customize;
        }
    }
}