using UnityEngine;

/*
[승문]
ShopManager
-하이어라키에 배치된 다이아/에메랄드 상품 패널에
-순서대로 데이터 세팅
*/
public class ShopManager : MonoBehaviour
{
    [Header("Diamond Panels (순서대로 6개)")]
    [SerializeField] private ShopSlotUI[] diamondPanels;

    [Header("Emerald Panels (순서대로 6개)")]
    [SerializeField] private ShopSlotUI[] emeraldPanels;

    [Header("Diamond Data")]
    [SerializeField] private int[] diamondPrice = { 3500, 6000, 9900, 30000, 60000, 100000 };
    [SerializeField] private int[] diamondReward = { 1400, 2700, 5000, 20000, 40000, 80000 };

    [Header("Emerald Data")]
    [SerializeField] private int[] emeraldPrice = { 3500, 6000, 9900, 30000, 60000, 100000 };
    [SerializeField] private int[] emeraldReward = { 350, 620, 1200, 3800, 7500, 15000 };

    private void Start()
    {
        ApplyDiamond();
        ApplyEmerald();

        // 재화 탭에서 둘 다 보이게
        ShowAllGoods();
    }

    private void ApplyDiamond()
    {
        int count = Mathf.Min(diamondPanels.Length, diamondPrice.Length, diamondReward.Length);
        for (int i = 0; i < count; i++)
        {
            if (diamondPanels[i] != null)
            {
                // Set(price, reward)
                diamondPanels[i].Set(diamondPrice[i], diamondReward[i]);
            }
        }
    }

    private void ApplyEmerald()
    {
        int count = Mathf.Min(emeraldPanels.Length, emeraldPrice.Length, emeraldReward.Length);
        for (int i = 0; i < count; i++)
        {
            if (emeraldPanels[i] != null)
            {
                // Set(price, reward)
                emeraldPanels[i].Set(emeraldPrice[i], emeraldReward[i]);
            }
        }
    }

    public void ShowAllGoods()
    {
        SetGroupActive(diamondPanels, true);
        SetGroupActive(emeraldPanels, true);
    }

    private void SetGroupActive(ShopSlotUI[] panels, bool active)
    {
        if (panels == null) return;
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                panels[i].gameObject.SetActive(active);
            }
        }
    }
}
