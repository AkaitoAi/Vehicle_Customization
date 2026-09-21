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

            if (itemID < 0)
            {
                Debug.LogWarning("[ApplyGameObjectToggleCustomization] itemID cannot be negative: " + itemID);
                return;
            }

            int childCount = containerTransform.childCount;
            if (childCount == 0) return;

            // Check if container directly contains item variations (e.g. child 0 = default/none, child 1..N = models)
            // or if it contains sub-slots (e.g. Left/Right shoe slots, 4 wheel slots)
            bool hasSubSlots = containerTransform.GetChild(0).childCount > 0;

            if (hasSubSlots)
            {
                for (int s = 0; s < childCount; s++)
                {
                    Transform slot = containerTransform.GetChild(s);
                    for (int c = 0; c < slot.childCount; c++)
                    {
                        slot.GetChild(c).gameObject.SetActive(c == itemID);
                    }
                }
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    containerTransform.GetChild(i).gameObject.SetActive(i == itemID);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}