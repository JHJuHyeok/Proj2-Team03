using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ShopSlotUI
-상점 상품 패널에 가격/보상 텍스트 세팅
-구매 버튼 클릭 시 "구매 요청 이벤트"만 발생 (실제 처리 = 외부)
*/
public class ShopSlotUI : MonoBehaviour
{
    [Header("Product")]
    [SerializeField] private EnumUI.CurrencyType currencyType; // Diamond / Emerald
    [SerializeField] private int price;                        // 결제 금액(원)
    [SerializeField] private int reward;                       // 지급 재화

    [Header("UI")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;   // 지불 금액
    [SerializeField] private TMP_Text rewardText;  // 지급 재화

    [Header("Currency Symbol")]
    [SerializeField] private string currencySymbol = "₩";

    // 외부에서 구독할 구매요청 이벤트
    public event Action<EnumUI.CurrencyType, int, int> OnPurchaseRequested;

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(HandleClickBuy);
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(HandleClickBuy);
        }
    }

    private void HandleClickBuy()
    {
        OnPurchaseRequested?.Invoke(currencyType, price, reward);
    }

    public void Set(EnumUI.CurrencyType type, int newPrice, int newReward)
    {
        currencyType = type;
        price = newPrice;
        reward = newReward;

        if (priceText != null)
        {
            priceText.text = $"{currencySymbol} {price:N0}";
        }
        if (rewardText != null)
        {
            rewardText.text = $"{reward:N0}";
        }
    }

    public EnumUI.CurrencyType GetCurrencyType() => currencyType;
    public int GetPrice() => price;
    public int GetReward() => reward;
}