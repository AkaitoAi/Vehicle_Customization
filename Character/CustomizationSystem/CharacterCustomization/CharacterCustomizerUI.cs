using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AkaitoAi.Customization;

/// <summary>
/// Strategy interface for handling custom logic per tab (e.g. Shirts, Pants, Shoes, Accessories).
/// Allows each tab to handle item selection, previewing, equipping, purchasing, and mesh/material changes independently.
/// </summary>
public interface ICustomizationStrategy
{
    void Initialize(CharacterCustomizerUI customizerUI, CharacterCustomizerUI.CustomizationTabPanel tabConfig);
    void OnTabOpened();
    void OnTabClosed();
    void OnItemSelected(int itemIndex);
    void OnApply();
    void OnReset();
}

/// <summary>
/// Abstract base class for customization tab strategies.
/// Extend this class to write specific customization logic for any tab.
/// </summary>
public abstract class CustomizationTabStrategy : ICustomizationStrategy
{
    protected CharacterCustomizerUI CustomizerUI { get; private set; }
    protected CharacterCustomizerUI.CustomizationTabPanel TabConfig { get; private set; }

    public virtual void Initialize(CharacterCustomizerUI customizerUI, CharacterCustomizerUI.CustomizationTabPanel tabConfig)
    {
        CustomizerUI = customizerUI;
        TabConfig = tabConfig;
    }

    public virtual void OnTabOpened() { }
    public virtual void OnTabClosed() { }
    public virtual void OnItemSelected(int itemIndex) { }
    public virtual void OnApply() { }
    public virtual void OnReset() { }
}

/// <summary>
/// Manages the UI navigation, tab screen routing, and customization equipping/purchasing for characters.
/// Controls transitions between the main category view and individual tab screens with return navigation,
/// item previewing, and PlayerPrefs persistence via CustomizationSO.
/// </summary>
public class CharacterCustomizerUI : MonoBehaviour
{
    [System.Serializable]
    public class CustomizationTabPanel
    {
        [Header("Tab Configuration")]
        [Tooltip("Unique identifier or category name for this tab (e.g. Head, Glasses, Mask, Shirts, Pants, Shoes).")]
        public string tabId = "Category";

        [Header("Customization Data")]
        [Tooltip("ScriptableObject containing item statuses, prices, and events for this category.")]
        public CustomizationSO customizationData;

        [Tooltip("Default item index to equip for this category when no preference exists (e.g. 0 for None, 1 for default Shirt/Pants/Shoes).")]
        public int defaultItemID = 0;

        [Header("Navigation UI")]
        [Tooltip("Button on the main menu or tab bar that opens this tab screen.")]
        public Button tabButton;

        [Tooltip("The screen/panel containing this tab's options and buttons.")]
        public GameObject screenPanel;

        [Tooltip("Back/Return button inside this tab screen to navigate back to the main category menu.")]
        public Button returnButton;

        [Header("Item Buttons")]
        [Tooltip("List of item/cloth selection buttons inside this tab screen.")]
        public Button[] itemButtons;

        [Header("Custom Item IDs Override (Optional)")]
        [Tooltip("Optional custom item IDs per button. If populated, customItemIDs[buttonIndex] overrides the target itemID.")]
        public int[] customItemIDs;

        [Header("Strategy (Optional)")]
        [Tooltip("Optional custom strategy component to handle equipping, previewing, or purchasing items for this tab.")]
        public CustomizationTabStrategy customStrategy;

        public void SetScreenActive(bool active)
        {
            if (screenPanel != null)
            {
                screenPanel.SetActive(active);
            }
        }
    }

    [Header("Main Category Screen")]
    [Tooltip("The main category selection screen/panel containing all tab buttons.")]
    [SerializeField] private GameObject mainCategoryScreen;

    [Header("Tabs Configuration")]
    [SerializeField] private List<CustomizationTabPanel> tabPanels = new List<CustomizationTabPanel>();

    [Header("Navigation Settings")]
    [Tooltip("If true, hides the main category screen when a tab screen is opened.")]
    [SerializeField] private bool hideMainScreenOnTabOpen = true;

    [Header("Purchase & Currency UI")]
    [Tooltip("Button used to purchase the currently previewed locked item.")]
    [SerializeField] private Button buyButton;

    [Tooltip("Standard UI Text to display item price.")]
    [SerializeField] private Text priceText;

    [Tooltip("TextMeshPro UI to display item price (optional).")]
    [SerializeField] private TextMeshProUGUI priceTextTMP;

    [Tooltip("IntVariable holding player coins / currency.")]
    [SerializeField] private IntVariable coins;

    [Tooltip("Standard UI Text to display player coins.")]
    [SerializeField] private Text coinsText;

    [Tooltip("TextMeshPro UI to display player coins (optional).")]
    [SerializeField] private TextMeshProUGUI coinsTextTMP;

    [Header("Defaults & Reset Options")]
    [Tooltip("Optional UI button to reset all customization categories to their configured default items.")]
    [SerializeField] private Button resetDefaultsButton;

    private int _currentActiveTabIndex = -1;
    private int _selectedItemID = -1;

    public int CurrentActiveTabIndex => _currentActiveTabIndex;
    public CustomizationTabPanel CurrentActiveTab => (_currentActiveTabIndex >= 0 && _currentActiveTabIndex < tabPanels.Count) ? tabPanels[_currentActiveTabIndex] : null;

    public static event Action OnPriceChanged;

    private void Awake()
    {
        InitializeTabs();
        SetupBuyButton();
        SetupResetDefaultsButton();
    }

    private void Start()
    {
        ApplyAllSavedCustomizations();
        UpdateCoinsUI();
    }

    private void OnEnable()
    {
        UpdateCoinsUI();
        ReturnToMain(suppressSound: true);
        CustomizationHandler.OnPriceChanged += UpdateCoinsUI;
    }

    private void OnDisable()
    {
        CustomizationHandler.OnPriceChanged -= UpdateCoinsUI;
    }

    private void OnValidate()
    {
        if (tabPanels != null)
        {
            foreach (var tab in tabPanels)
            {
                if (tab != null && tab.customizationData != null)
                {
                    tab.defaultItemID = tab.customizationData.defaultItemID;
                }
            }
        }
    }

    /// <summary>
    /// Configures the buy button listener.
    /// </summary>
    private void SetupBuyButton()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
            buyButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Configures the reset defaults button listener if assigned.
    /// </summary>
    private void SetupResetDefaultsButton()
    {
        if (resetDefaultsButton != null)
        {
            resetDefaultsButton.onClick.RemoveAllListeners();
            resetDefaultsButton.onClick.AddListener(ResetToDefaults);
        }
    }

    /// <summary>
    /// Sets up listeners on all tab buttons, return buttons, and item buttons.
    /// </summary>
    private void InitializeTabs()
    {
        for (int i = 0; i < tabPanels.Count; i++)
        {
            int tabIndex = i;
            var tab = tabPanels[i];

            // Sync defaultItemID from CustomizationSO if available
            if (tab.customizationData != null)
            {
                tab.defaultItemID = tab.customizationData.defaultItemID;
            }

            // Tab button click -> Open tab screen
            if (tab.tabButton != null)
            {
                tab.tabButton.onClick.RemoveAllListeners();
                tab.tabButton.onClick.AddListener(() => OpenTab(tabIndex));
            }

            // Tab return button click -> Return to main category screen
            if (tab.returnButton != null)
            {
                tab.returnButton.onClick.RemoveAllListeners();
                tab.returnButton.onClick.AddListener(ReturnToMain);
            }

            // Item buttons click -> trigger customization selection
            if (tab.itemButtons != null)
            {
                for (int j = 0; j < tab.itemButtons.Length; j++)
                {
                    int buttonIndex = j;
                    if (tab.itemButtons[j] != null)
                    {
                        tab.itemButtons[j].onClick.RemoveAllListeners();
                        tab.itemButtons[j].onClick.AddListener(() => OnItemButtonClicked(tabIndex, buttonIndex));
                    }
                }
            }

            // Initialize custom strategy if attached
            if (tab.customStrategy != null)
            {
                tab.customStrategy.Initialize(this, tab);
            }
        }
    }

    /// <summary>
    /// Iterates through all tabs and applies the saved itemID to each CustomizationSO.
    /// Ensures character models reflect the saved loadout on load.
    /// </summary>
    public void ApplyAllSavedCustomizations()
    {
        foreach (var tab in tabPanels)
        {
            if (tab.customizationData != null)
            {
                tab.defaultItemID = tab.customizationData.defaultItemID;
                tab.customizationData.LoadUnlocked();
                tab.customizationData.itemID = tab.customizationData.LoadSelected();
                tab.customizationData.Actions.OnItemIDChanged?.Invoke(tab.customizationData.itemID);
            }
        }
    }

    /// <summary>
    /// Converts a button index to its corresponding itemID / offset index.
    /// Button index maps directly to offset/item index (0 -> 0, 1 -> 1, 2 -> 2, etc.),
    /// and the last button simply resets/returns to the default item.
    /// </summary>
    public int GetItemIDFromButtonIndex(int tabIndex, int buttonIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabPanels.Count) return buttonIndex;

        var tab = tabPanels[tabIndex];
        int totalButtons = tab.itemButtons != null ? tab.itemButtons.Length : 0;

        // Custom override if explicitly specified in inspector
        if (tab.customItemIDs != null && tab.customItemIDs.Length > buttonIndex)
        {
            return tab.customItemIDs[buttonIndex];
        }

        // Last button simply resets to default item
        if (totalButtons > 1 && buttonIndex == totalButtons - 1)
        {
            return tab.customizationData != null ? tab.customizationData.defaultItemID : tab.defaultItemID;
        }

        // Direct 1-to-1 button index mapping (Button 0 -> Item 0, Button 1 -> Item 1, etc.)
        return buttonIndex;
    }

    /// <summary>
    /// Converts an itemID to its corresponding button index in the tab's UI.
    /// </summary>
    public int GetButtonIndexFromItemID(int tabIndex, int itemID)
    {
        if (tabIndex < 0 || tabIndex >= tabPanels.Count) return itemID;

        var tab = tabPanels[tabIndex];
        int totalButtons = tab.itemButtons != null ? tab.itemButtons.Length : 0;

        for (int i = 0; i < totalButtons; i++)
        {
            if (GetItemIDFromButtonIndex(tabIndex, i) == itemID)
            {
                return i;
            }
        }

        return itemID;
    }

    /// <summary>
    /// Opens the tab screen corresponding to the given index.
    /// </summary>
    public void OpenTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabPanels.Count)
        {
            Debug.LogWarning($"[CharacterCustomizerUI] Invalid tab index: {tabIndex}");
            return;
        }

        PlayClickSound();

        // Notify previous tab strategy of closure
        if (CurrentActiveTab != null && CurrentActiveTab.customStrategy != null)
        {
            CurrentActiveTab.customStrategy.OnTabClosed();
        }

        // Hide all screens first
        CloseAllTabScreens();

        // Hide main category screen if configured
        if (hideMainScreenOnTabOpen && mainCategoryScreen != null)
        {
            mainCategoryScreen.SetActive(false);
        }

        // Activate selected tab screen
        _currentActiveTabIndex = tabIndex;
        var selectedTab = tabPanels[tabIndex];
        selectedTab.SetScreenActive(true);

        // Notify SO and initialize selection state
        if (selectedTab.customizationData != null)
        {
            selectedTab.customizationData.Actions.OnEntered?.Invoke();
            _selectedItemID = selectedTab.customizationData.itemID;

            // Update buy button state based on equipped item
            UpdateBuyButtonState(selectedTab.customizationData, _selectedItemID);
        }
        else
        {
            HideBuyButton();
        }

        // Notify tab strategy of opening
        if (selectedTab.customStrategy != null)
        {
            selectedTab.customStrategy.OnTabOpened();
        }
    }

    /// <summary>
    /// Opens a tab screen by its identifier string.
    /// </summary>
    public void OpenTab(string tabId)
    {
        int index = tabPanels.FindIndex(t => string.Equals(t.tabId, tabId, StringComparison.OrdinalIgnoreCase));
        if (index != -1)
        {
            OpenTab(index);
        }
        else
        {
            Debug.LogWarning($"[CharacterCustomizerUI] Tab with ID '{tabId}' not found.");
        }
    }

    /// <summary>
    /// Returns from the active tab screen back to the main category screen.
    /// Reverts previewed unbought items back to the saved equipped item.
    /// </summary>
    public void ReturnToMain()
    {
        ReturnToMain(suppressSound: false);
    }

    private void ReturnToMain(bool suppressSound)
    {
        if (!suppressSound)
        {
            PlayClickSound();
        }

        // If previewing an unowned item, revert back to the saved equipped item
        if (CurrentActiveTab != null && CurrentActiveTab.customizationData != null)
        {
            int savedItemID = CurrentActiveTab.customizationData.itemID;
            CurrentActiveTab.customizationData.Actions.OnItemIDChanged?.Invoke(savedItemID);
        }

        // Hide buy button
        HideBuyButton();

        // Notify current tab strategy of closure
        if (CurrentActiveTab != null && CurrentActiveTab.customStrategy != null)
        {
            CurrentActiveTab.customStrategy.OnTabClosed();
        }

        // Hide all tab screens
        CloseAllTabScreens();
        _currentActiveTabIndex = -1;

        // Show main category screen
        if (mainCategoryScreen != null)
        {
            mainCategoryScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Hides all tab screens.
    /// </summary>
    public void CloseAllTabScreens()
    {
        foreach (var tab in tabPanels)
        {
            tab.SetScreenActive(false);
        }
    }

    /// <summary>
    /// Called when any item button inside a tab screen is clicked.
    /// </summary>
    private void OnItemButtonClicked(int tabIndex, int buttonIndex)
    {
        PlayClickSound();

        if (tabIndex < 0 || tabIndex >= tabPanels.Count) return;

        var tab = tabPanels[tabIndex];
        int targetItemID = GetItemIDFromButtonIndex(tabIndex, buttonIndex);

        _selectedItemID = targetItemID;

        if (tab.customizationData != null)
        {
            // Trigger live preview on character
            tab.customizationData.Actions.OnItemIDChanged?.Invoke(targetItemID);

            // Handle lock / unlock status
            UpdateBuyButtonState(tab.customizationData, targetItemID);
        }

        // Notify custom strategy if attached
        if (tab.customStrategy != null)
        {
            tab.customStrategy.OnItemSelected(buttonIndex);
        }
    }

    /// <summary>
    /// Updates the buy button visibility and price text according to item unlock state.
    /// </summary>
    private void UpdateBuyButtonState(CustomizationSO data, int itemID)
    {
        if (data == null || itemID < 0)
        {
            HideBuyButton();
            return;
        }

        bool isUnlocked = true;
        int price = 0;

        if (data.Status != null && itemID < data.Status.Length)
        {
            isUnlocked = data.Status[itemID].isUnlocked;
            price = data.Status[itemID].price;
        }

        if (isUnlocked)
        {
            data.itemID = itemID;
            data.SaveSelected();
            HideBuyButton();
        }
        else
        {
            ShowBuyButton(price);
        }
    }

    private void ShowBuyButton(int price)
    {
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(true);
        }

        if (priceText != null)
        {
            priceText.text = price.ToString();
        }

        if (priceTextTMP != null)
        {
            priceTextTMP.text = price.ToString();
        }
    }

    private void HideBuyButton()
    {
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handles purchasing the currently previewed locked item.
    /// </summary>
    private void OnBuyButtonClicked()
    {
        if (CurrentActiveTab == null || CurrentActiveTab.customizationData == null) return;

        var data = CurrentActiveTab.customizationData;
        if (_selectedItemID < 0) return;

        int price = 0;
        if (data.Status != null && _selectedItemID < data.Status.Length)
        {
            price = data.Status[_selectedItemID].price;
        }

        if (coins != null && coins.value >= price)
        {
            // Deduct currency
            coins.value -= price;

            // Unlock and equip item
            if (data.Status != null && _selectedItemID < data.Status.Length)
            {
                data.Status[_selectedItemID].isUnlocked = true;
            }
            data.itemID = _selectedItemID;
            data.SaveUnlocked(_selectedItemID);
            data.SaveSelected();

            // Refresh UI
            HideBuyButton();
            UpdateCoinsUI();

            // Notify listeners
            OnPriceChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("[CharacterCustomizerUI] Not enough coins to purchase item.");
        }
    }

    /// <summary>
    /// Updates coins display UI.
    /// </summary>
    public void UpdateCoinsUI()
    {
        if (coins == null) return;

        if (coinsText != null)
        {
            coinsText.text = coins.value.ToString();
        }

        if (coinsTextTMP != null)
        {
            coinsTextTMP.text = coins.value.ToString();
        }
    }

    /// <summary>
    /// Helper to play UI sound via SoundManager if available.
    /// </summary>
    private void PlayClickSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayOnButtonSound();
        }
    }

    /// <summary>
    /// Dynamically registers or replaces a strategy for a given tab at runtime.
    /// </summary>
    public void SetTabStrategy(int tabIndex, CustomizationTabStrategy strategy)
    {
        if (tabIndex >= 0 && tabIndex < tabPanels.Count && strategy != null)
        {
            tabPanels[tabIndex].customStrategy = strategy;
            strategy.Initialize(this, tabPanels[tabIndex]);
        }
    }

    /// <summary>
    /// Resets all customization categories to their configured default items.
    /// Can be called from a UI button, code, or context menu in the Inspector.
    /// </summary>
    [ContextMenu("Reset All To Defaults")]
    public void ResetToDefaults()
    {
        PlayClickSound();

        foreach (var tab in tabPanels)
        {
            if (tab.customizationData != null)
            {
                if (tab.defaultItemID != 0)
                {
                    tab.customizationData.defaultItemID = tab.defaultItemID;
                }
                tab.customizationData.ResetToDefault();
            }
        }

        if (CurrentActiveTab != null && CurrentActiveTab.customizationData != null)
        {
            _selectedItemID = CurrentActiveTab.customizationData.itemID;
            UpdateBuyButtonState(CurrentActiveTab.customizationData, _selectedItemID);
        }
        else
        {
            HideBuyButton();
        }
    }
}
