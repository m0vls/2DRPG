using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{ 
    [SerializeField] private TMP_Text currencyText;

    public void ResetUI()
    {
        if (currencyText != null)
            currencyText.text = "";
    }

    public void UpdateCurrency(int amount)
    {
        if (currencyText != null)
            currencyText.text = amount.ToString();
    }
}
