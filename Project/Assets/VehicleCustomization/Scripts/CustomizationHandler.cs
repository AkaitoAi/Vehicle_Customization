using System;
using UnityEngine;
using UnityEngine.UI;

namespace AkaitoAi.Customization
{
    public class CustomizationHandler : MonoBehaviour
    {
        [SerializeField] private CustomizationUI[] customizationPanels;
        [SerializeField] private CustomizationData[] customizationDatas;
        private int currentSelectedIndex;
        private CustomizationSO currenCustomizationSO;

        [Serializable]
        public struct CustomizationUI
        {
            public GameObject panel;
            public Button[] buttons;
            internal CustomizationSO customizationData;
        }
        
        [Serializable]
        public struct CustomizationData
        {
            public CustomizationSO[] data;
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
            
            if(customizationPanels[index].customizationData != null)
                customizationPanels[index].customizationData.Actions.OnEntered?.Invoke();
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

                if (currenCustomizationSO == null)
                {
                    OnSwitchPanelButton(index);

                    return;
                }

                if (currenCustomizationSO != null)
                {
                    OnIndexButton(index);

                    return;
                }
            }
        }

        public void OnBackButton()
        {
            TogglePanel(0);
            currenCustomizationSO = null;
        }

        public void OnSwitchPanelButton(int index)
        {
            switch (index)
            {
                case 0:
                    TogglePanel(1);
                    currenCustomizationSO = customizationPanels[1].customizationData;
                    break;
                
                case 1: 
                    TogglePanel(2);
                    currenCustomizationSO = customizationPanels[2].customizationData;
                    break;
                
                case 2: 
                    TogglePanel(3);
                    currenCustomizationSO = customizationPanels[3].customizationData;
                    break;
                
                case 3: 
                    TogglePanel(4);
                    currenCustomizationSO = customizationPanels[4].customizationData;
                    break;
                
                default: break;
            }
        }

        public void OnIndexButton(int index)
        {
            if(currenCustomizationSO == null) return;
            
            if(currenCustomizationSO.id != currentSelectedIndex) return;

            currenCustomizationSO.Actions.OnItemIDChanged?.Invoke(index);

            if (currenCustomizationSO.status[index].isUnlocked)
                currenCustomizationSO.itemID = index;

        }

        private void SetupCustomizationSO(int dataIndex)
        {
            currentSelectedIndex = dataIndex;
            
            if (dataIndex < customizationDatas.Length)
            {
                for (int i = 1; i < customizationPanels.Length; i++)
                {
                    if (i - 1 < customizationDatas[dataIndex].data.Length)
                    {
                        customizationPanels[i].customizationData = customizationDatas[dataIndex].data[i - 1];

                        customizationDatas[dataIndex].data[i - 1].Actions.OnItemIDChanged?.
                            Invoke(customizationDatas[dataIndex].data[i - 1].itemID);
                    }
                    else
                    {
                        Debug.LogWarning($"Not enough data in customizationDatas[{dataIndex}].data to assign to customizationPanels[{i}].");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Invalid dataIndex provided for SetupCustomizationSO.");
            }
        }

        private void OnEnable()
        {
            SelectionHandler.OnCurrentIndexUpdate += SetupCustomizationSO;
        }

        private void OnDisable()
        {
            SelectionHandler.OnCurrentIndexUpdate -= SetupCustomizationSO;
        }
    }
}
