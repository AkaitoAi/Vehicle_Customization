using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AkaitoAi.Customization
{
    public class ApplyColorCustomization : Customization
    {
        [SerializeField] private MeshRenderer[] meshRenderers;
        [SerializeField] private Color[] colors;
        private void Update()
        {
            Customize(CustomizationData.itemID);
        }

        protected override void Customize(int itemID)
        {
            if (!CanCustomize()) return;

            base.Customize(itemID);


            if (itemID >= CustomizationData.status.Length
                || itemID < 0)
            {
                Debug.Log("itemID is out of range.");

                return;
            }

            
        }
    }
}
