using System;
using UnityEngine;

/*
[승문]
ShopManager
-하이어라키에 배치된 다이아/에메랄드 상품 패널에 순서대로 데이터 세팅
-각 슬롯의 구매 버튼 클릭을 받아서, 외부(결제/데이터/재화 시스템)로 "구매 요청" 이벤트를 전달
*/
public class ShopManager : MonoBehaviour
{
    [Serializable]
    public struct ShopProduct
    {
        public int reward; // 획득재화
        public int price;  // 금액(₩)
    }

    [Header("Diamond Panels (순서대로 6개)")]
    [SerializeField] private ShopSlotUI[] diamondPanels;

    [Header("Emerald Panels (순서대로 6개)")]
    [SerializeField] private ShopSlotUI[] emeraldPanels;

    [Header("Diamond Products (획득재화 <= 금액)")]
    [SerializeField]
    private ShopProduct[] diamondProducts =
    {
        new ShopProduct { reward = 1400,  price = 3500   },
        new ShopProduct { reward = 2700,  price = 6000   },
        new ShopProduct { reward = 5000,  price = 9900   },
        new ShopProduct { reward = 20000, price = 30000  },
        new ShopProduct { reward = 40000, price = 60000  },
        new ShopProduct { reward = 80000, price = 100000 },
    };

    [Header("Emerald Products (획득재화 <= 금액)")]
    [SerializeField]
    private ShopProduct[] emeraldProducts =
    {
        new ShopProduct { reward = 350,   price = 3500   },
        new ShopProduct { reward = 620,   price = 6000   },
        new ShopProduct { reward = 1200,  price = 9900   },
        new ShopProduct { reward = 3800,  price = 30000  },
        new ShopProduct { reward = 7500,  price = 60000  },
        new ShopProduct { reward = 15000, price = 100000 },
    };

    // 외부에서 구독할 이벤트(ShopManager 하나만 구독하면 됨)
    public event Action<EnumUI.CurrencyType, int, int> OnPurchaseRequested;

    private void Start()
    {
        ApplyGroup(diamondPanels, EnumUI.CurrencyType.Diamond, diamondProducts);
        ApplyGroup(emeraldPanels, EnumUI.CurrencyType.Emerald, emeraldProducts);

        ShowAllGoods();
    }

    private void OnDestroy()
    {
        UnbindGroup(diamondPanels);
        UnbindGroup(emeraldPanels);
    }

    private void ApplyGroup(ShopSlotUI[] panels, EnumUI.CurrencyType type, ShopProduct[] products)
    {
        if (panels == null || products == null) return;

        int count = Mathf.Min(panels.Length, products.Length);
        for (int i = 0; i < count; i++)
        {
            if (panels[i] == null) continue;

            // 데이터 세팅
            panels[i].Set(type, products[i].price, products[i].reward);

            // 슬롯 이벤트를 매니저가 받아서 외부로 전달
            panels[i].OnPurchaseRequested -= HandleSlotPurchaseRequested;
            panels[i].OnPurchaseRequested += HandleSlotPurchaseRequested;
        }
    }

    private void UnbindGroup(ShopSlotUI[] panels)
    {
        if (panels == null) return;

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] == null) continue;
            panels[i].OnPurchaseRequested -= HandleSlotPurchaseRequested;
        }
    }

    private void HandleSlotPurchaseRequested(EnumUI.CurrencyType type, int price, int reward)
    {
        OnPurchaseRequested?.Invoke(type, price, reward);
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