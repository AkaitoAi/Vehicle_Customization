using UnityEngine;

namespace AkaitoAi.Customization
{
    public class ApplyCustomizations : MonoBehaviour
    {
        [SerializeField] private Customize[] customizers;

        [System.Serializable]
        public struct Customize
        {
            public Customization[] customizations;
        }

        private void Start()
        {
            foreach (Customize customizer in customizers)
            {
                foreach(Customization customization in customizer.customizations)
                    customization.ApplyCustomization();
            }
        }
    }
}
