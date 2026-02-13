using TMPro;
using UnityEngine;

/*
[승문]
ShopSlotUI
-이미 배치된 상점 상품 패널에 가격/보상 텍스트 세팅
*/
public class ShopSlotUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text priceText;   // 지불 재화
    [SerializeField] private TMP_Text rewardText;  // 지급 재화

    [Header("Currency Symbol")]
    [SerializeField] private string currencySymbol = "₩";  // 인스펙터에서 변경 가능

    public void Set(int price, int reward)
    {
        if (priceText != null)
        {
            priceText.text = currencySymbol + " " + price.ToString("N0");
        }

        if (rewardText != null)
        {
            rewardText.text = reward.ToString("N0");
        }
    }
}
