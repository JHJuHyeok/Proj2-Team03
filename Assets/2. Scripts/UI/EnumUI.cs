/*
[승문]
EnumUI
-UI에서 사용하는 Enum 전용 데이터
-강화/성장만 고유키 접두어 사용, 승급은 접두어 없음
*/
public static class EnumUI
{
    public enum TabType
    {
        Enhance,
        Growth,
        Promotion,
        Weapon,
        Accessory,
        Relic,
        Summon,
        Goods
    }

    // SlotKeyIconBinder에서 선택할 그룹
    public enum GroupType
    {
        Enhance,
        Growth,
        Promotion
    }

    public enum SlotKey
    {
        // Enhance(강화)
        ENH_STR,
        ENH_HP,
        ENH_VIT,
        ENH_CRI_DMG,
        ENH_CRI_PROB,
        ENH_BLOW_DMG,
        ENH_BLOW_PROB,

        // Growth(성장)
        GRO_STR,
        GRO_HP,
        GRO_VIT,
        GRO_CRI,
        GRO_LUK,
        GRO_ACC,
        GRO_DODGE,

        // Promotion(승급)
        STONE,
        BRONZE,
        IRON,
        SILVER,
        GOLD,
        MITHRIL,
        ORICHALCUM,
        ARCANITE,
        ADAMANTITE,
        ETHER
    }

    //상점재화
    public enum CurrencyType
    {
        Diamond,
        Emerald
    }

    [System.Serializable]
    public class ShopProductData
    {
        public CurrencyType currencyType; // 다이아 / 에메랄드
        public int price;                 // 지불 재화
        public int reward;                // 지급 재화
    }
}
