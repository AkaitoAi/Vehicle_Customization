using UnityEngine;

namespace AkaitoAi.Customization
{
    public class ApplyWheelCustomization : Customization
    {
        [SerializeField] private Transform wheelContainerTransform;

        protected override void Customize(int itemID)
        {
            if (!CanCustomize()) return;

            base.Customize(itemID);

            int rimSlotCount = wheelContainerTransform.transform.childCount;
            int totalRims = rimSlotCount * CustomizationData.status.Length;

            // Cache the wheels transform
            Transform wheelsTransform = wheelContainerTransform.transform;

            // Single loop to deactivate all rims except the selected one
            for (int i = 0; i < totalRims; i++)
            {
                Transform rimSlot = wheelsTransform.GetChild(i / CustomizationData.status.Length);
                GameObject rim = rimSlot.GetChild(i % CustomizationData.status.Length).gameObject;

                // Set active state based on whether this is the selected rim
                rim.SetActive(i % CustomizationData.status.Length == itemID);
            }
        }
    }
}