using UnityEngine;

/*
[승문]
IShopItemProvider
-상점 데이터/구매/뽑기 로직 연결용 인터페이스
*/
public interface IShopItemProvider
{
    int GetCount(ShopTab tab);
    ShopItemInfo GetItem(ShopTab tab, int index);
    bool TryBuy(ShopTab tab, int index);
    bool TryBuyBatch(ShopTab tab, int count);//1,11,33회 뽑기
}

/// <summary>
/// 상점 슬롯 1칸 UI에 필요한 최소 정보 구조체
/// </summary>
public struct ShopItemInfo
{
    public string Id;
    public string Name;
    public string SpriteName;
    public string PriceText;
    public bool CanBuy;
}