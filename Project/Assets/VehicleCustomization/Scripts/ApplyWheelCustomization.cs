using System;
using UnityEngine;

namespace AkaitoAi.Customization
{
    public class ApplyWheelCustomization : Customization
    {
        [SerializeField] private Transform wheelContainerTransform;

        // private void Update()
        // {
        //     Customize(CustomizationData.itemID);
        // }

        protected override void Customize(int itemID)
        {     
            if (!CanCustomize()) return;

            base.Customize(itemID);
            
            if(wheelContainerTransform == null) return;
            
            int wheelSlotCount = wheelContainerTransform.childCount;
            int wheelTypeCount = CustomizationData.status.Length;
            int totalWheels = wheelSlotCount * wheelTypeCount;

            Transform wheelsTransform = wheelContainerTransform;

            for (int i = 0; i < totalWheels; i++)
            {
                Transform wheelSlot = wheelsTransform.GetChild(i / wheelTypeCount);
                GameObject wheel = wheelSlot.GetChild(i % wheelTypeCount).gameObject;
                wheel.SetActive(i % wheelTypeCount == itemID);
            }
        }
    }
}