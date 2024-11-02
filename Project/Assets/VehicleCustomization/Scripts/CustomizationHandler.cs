using System;
using UnityEngine;
using UnityEngine.UI;

namespace AkaitoAi.Customization
{
    public class CustomizationHandler : MonoBehaviour
    {
        [SerializeField] private CustomizationUI[] customizationPanels;

        [Serializable]
        public struct CustomizationUI
        {
            public GameObject panel;
            public Button[] buttons;
        }

        private void Start()
        {
            AddIndexToButtons();

            TogglePanel(0);
        }

        private void TogglePanel(int index)
        {
            foreach (CustomizationUI ui in customizationPanels)
                ui.panel.SetActive(false);
            
            customizationPanels[index].panel.SetActive(true);
        }

        private void AddIndexToButtons()
        {
            foreach (CustomizationUI ui in customizationPanels)
            {
                for (int i = 0; i < ui.buttons.Length; i++)
                {
                    int index = i;
                    ui.buttons[i].onClick.AddListener(() => HandleButtonClick(index));
                }
            }

            void HandleButtonClick(int index)
            {
                Debug.Log($"Button {index} clicked!");
            }
        }

        public void OnBackButton()
        {
            TogglePanel(0);
        }

        public void OnSwitchPanelButton(int index)
        {
            switch (index)
            {
                case 0: 
                    TogglePanel(1);
                    break;
                
                case 1: 
                    TogglePanel(2);
                    break;
                
                case 2: 
                    TogglePanel(3);
                    break;
                
                case 3: 
                    TogglePanel(4);
                    break;
                
                default: break;
            }
        }
    }
}
