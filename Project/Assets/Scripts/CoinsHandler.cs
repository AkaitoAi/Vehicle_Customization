using UnityEngine;
using UnityEngine.UI;
using AkaitoAi.Customization;

public class CoinsHandler : MonoBehaviour
{
    [SerializeField] private Text coinsText;
    [SerializeField] private IntVariable coins;

    private void Start()
    {
        UpdateTotalCoins();
    }

    private void UpdateTotalCoins()
    {
        coinsText.text = coins.value.ToString();
    }

    private void OnEnable()
    {
        CustomizationHandler.OnPriceChanged += UpdateTotalCoins;
    }

    private void OnDisable()
    {
        CustomizationHandler.OnPriceChanged -= UpdateTotalCoins;
    }
}
