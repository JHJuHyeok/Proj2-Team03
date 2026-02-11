/*
[승문]
EnumUITables
-EnumUI.SlotKey -> 표시문구 테이블
-슬롯 키 "순서" 테이블(강화/성장/승급)도 여기서 관리
*/
public static class EnumUITables
{
    // 강화 슬롯 키 순서
    private static readonly EnumUI.SlotKey[] EnhanceKeys =
    {
        EnumUI.SlotKey.ENH_STR,
        EnumUI.SlotKey.ENH_HP,
        EnumUI.SlotKey.ENH_VIT,
        EnumUI.SlotKey.ENH_CRI_DMG,
        EnumUI.SlotKey.ENH_CRI_PROB,
        EnumUI.SlotKey.ENH_BLOW_DMG,
        EnumUI.SlotKey.ENH_BLOW_PROB
    };

    // 성장 슬롯 키 순서
    private static readonly EnumUI.SlotKey[] GrowthKeys =
    {
        EnumUI.SlotKey.GRO_STR,
        EnumUI.SlotKey.GRO_HP,
        EnumUI.SlotKey.GRO_VIT,
        EnumUI.SlotKey.GRO_CRI,
        EnumUI.SlotKey.GRO_LUK,
        EnumUI.SlotKey.GRO_ACC,
        EnumUI.SlotKey.GRO_DODGE
    };

    // 승급 슬롯 키 순서
    private static readonly EnumUI.SlotKey[] PromotionKeys =
    {
        EnumUI.SlotKey.STONE,
        EnumUI.SlotKey.BRONZE,
        EnumUI.SlotKey.IRON,
        EnumUI.SlotKey.SILVER,
        EnumUI.SlotKey.GOLD,
        EnumUI.SlotKey.MITHRIL,
        EnumUI.SlotKey.ORICHALCUM,
        EnumUI.SlotKey.ARCANITE,
        EnumUI.SlotKey.ADAMANTITE,
        EnumUI.SlotKey.ETHER
    };

    // 컨테이너가 그룹만 선택하면, 해당 그룹 키 배열을 반환
    public static EnumUI.SlotKey[] GetKeysByGroup(EnumUI.GroupType group)
    {
        if (group == EnumUI.GroupType.Enhance) return EnhanceKeys;
        if (group == EnumUI.GroupType.Growth) return GrowthKeys;
        return PromotionKeys;
    }

    public static EnumUI.TabType GetTab(EnumUI.SlotKey key)
    {
        // 강화
        if (key == EnumUI.SlotKey.ENH_STR) return EnumUI.TabType.Enhance;
        if (key == EnumUI.SlotKey.ENH_HP) return EnumUI.TabType.Enhance;
        if (key == EnumUI.SlotKey.ENH_VIT) return EnumUI.TabType.Enhance;
        if (key == EnumUI.SlotKey.ENH_CRI_DMG) return EnumUI.TabType.Enhance;
        if (key == EnumUI.SlotKey.ENH_CRI_PROB) return EnumUI.TabType.Enhance;
        if (key == EnumUI.SlotKey.ENH_BLOW_DMG) return EnumUI.TabType.Enhance;
        if (key == EnumUI.SlotKey.ENH_BLOW_PROB) return EnumUI.TabType.Enhance;

        // 성장
        if (key == EnumUI.SlotKey.GRO_STR) return EnumUI.TabType.Growth;
        if (key == EnumUI.SlotKey.GRO_HP) return EnumUI.TabType.Growth;
        if (key == EnumUI.SlotKey.GRO_VIT) return EnumUI.TabType.Growth;
        if (key == EnumUI.SlotKey.GRO_CRI) return EnumUI.TabType.Growth;
        if (key == EnumUI.SlotKey.GRO_LUK) return EnumUI.TabType.Growth;
        if (key == EnumUI.SlotKey.GRO_ACC) return EnumUI.TabType.Growth;
        if (key == EnumUI.SlotKey.GRO_DODGE) return EnumUI.TabType.Growth;

        // 나머지 = 승급
        return EnumUI.TabType.Promotion;
    }

    // 상단 짧은 라벨(STR/HP/등급 등)
    public static string GetTopLabel(EnumUI.SlotKey key)
    {
        // 강화
        if (key == EnumUI.SlotKey.ENH_STR) return "STR";
        if (key == EnumUI.SlotKey.ENH_HP) return "HP";
        if (key == EnumUI.SlotKey.ENH_VIT) return "VIT";
        if (key == EnumUI.SlotKey.ENH_CRI_DMG) return "CRI DMG";
        if (key == EnumUI.SlotKey.ENH_CRI_PROB) return "CRI PROB";
        if (key == EnumUI.SlotKey.ENH_BLOW_DMG) return "BLOW DMG";
        if (key == EnumUI.SlotKey.ENH_BLOW_PROB) return "BLOW PROB";

        // 성장
        if (key == EnumUI.SlotKey.GRO_STR) return "STR";
        if (key == EnumUI.SlotKey.GRO_HP) return "HP";
        if (key == EnumUI.SlotKey.GRO_VIT) return "VIT";
        if (key == EnumUI.SlotKey.GRO_CRI) return "CRI";
        if (key == EnumUI.SlotKey.GRO_LUK) return "LUK";
        if (key == EnumUI.SlotKey.GRO_ACC) return "ACC";
        if (key == EnumUI.SlotKey.GRO_DODGE) return "DODGE";

        // 승급
        if (key == EnumUI.SlotKey.STONE) return "STONE";
        if (key == EnumUI.SlotKey.BRONZE) return "BRONZE";
        if (key == EnumUI.SlotKey.IRON) return "IRON";
        if (key == EnumUI.SlotKey.SILVER) return "SILVER";
        if (key == EnumUI.SlotKey.GOLD) return "GOLD";
        if (key == EnumUI.SlotKey.MITHRIL) return "MITHRIL";
        if (key == EnumUI.SlotKey.ORICHALCUM) return "ORICHALCUM";
        if (key == EnumUI.SlotKey.ARCANITE) return "ARCANITE";
        if (key == EnumUI.SlotKey.ADAMANTITE) return "ADAMANTITE";
        if (key == EnumUI.SlotKey.ETHER) return "ETHER";

        return key.ToString();
    }

    // 하단 한글 표시(공격력/체력/등급명 등)
    public static string GetKoreanName(EnumUI.SlotKey key)
    {
        // 강화
        if (key == EnumUI.SlotKey.ENH_STR) return "공격력";
        if (key == EnumUI.SlotKey.ENH_HP) return "체력";
        if (key == EnumUI.SlotKey.ENH_VIT) return "체력 회복량";
        if (key == EnumUI.SlotKey.ENH_CRI_DMG) return "치명타 피해";
        if (key == EnumUI.SlotKey.ENH_CRI_PROB) return "치명타 확률";
        if (key == EnumUI.SlotKey.ENH_BLOW_DMG) return "회심의 일격 피해";
        if (key == EnumUI.SlotKey.ENH_BLOW_PROB) return "회심의 일격 확률";

        // 성장
        if (key == EnumUI.SlotKey.GRO_STR) return "공격력";
        if (key == EnumUI.SlotKey.GRO_HP) return "체력";
        if (key == EnumUI.SlotKey.GRO_VIT) return "체력 회복량";
        if (key == EnumUI.SlotKey.GRO_CRI) return "치명타";
        if (key == EnumUI.SlotKey.GRO_LUK) return "행운";
        if (key == EnumUI.SlotKey.GRO_ACC) return "명중";
        if (key == EnumUI.SlotKey.GRO_DODGE) return "회피";

        // 승급(등급명 한글)
        if (key == EnumUI.SlotKey.STONE) return "스톤";
        if (key == EnumUI.SlotKey.BRONZE) return "브론즈";
        if (key == EnumUI.SlotKey.IRON) return "아이언";
        if (key == EnumUI.SlotKey.SILVER) return "실버";
        if (key == EnumUI.SlotKey.GOLD) return "골드";
        if (key == EnumUI.SlotKey.MITHRIL) return "미스릴";
        if (key == EnumUI.SlotKey.ORICHALCUM) return "오리하르콘";
        if (key == EnumUI.SlotKey.ARCANITE) return "아케나이트";
        if (key == EnumUI.SlotKey.ADAMANTITE) return "아다만타이트";
        if (key == EnumUI.SlotKey.ETHER) return "에테르";

        return key.ToString();
    }

    // 승급 텍스트UI
    public struct PromotionInfo
    {
        public int growthMul;        // 공격력 / 체력 배수 (xN)
        public int recommendLevel;   // 권장 레벨 (Stone은 0 처리)
    }

    public static PromotionInfo GetPromotionInfo(EnumUI.SlotKey key)
    {
        if (key == EnumUI.SlotKey.STONE) return Make(1, 0);       // 기본
        if (key == EnumUI.SlotKey.BRONZE) return Make(2, 50);
        if (key == EnumUI.SlotKey.IRON) return Make(5, 90);
        if (key == EnumUI.SlotKey.SILVER) return Make(18, 180);
        if (key == EnumUI.SlotKey.GOLD) return Make(25, 300);
        if (key == EnumUI.SlotKey.MITHRIL) return Make(100, 450);
        if (key == EnumUI.SlotKey.ORICHALCUM) return Make(300, 600);
        if (key == EnumUI.SlotKey.ARCANITE) return Make(550, 700);
        if (key == EnumUI.SlotKey.ADAMANTITE) return Make(1000, 850);
        if (key == EnumUI.SlotKey.ETHER) return Make(2000, 1000);

        return Make(1, 0);
    }

    private static PromotionInfo Make(int mul, int recommendLv)
    {
        PromotionInfo info = new PromotionInfo();
        info.growthMul = mul;
        info.recommendLevel = recommendLv;
        return info;
    }

    // 승급 버튼UI 순서
    private static readonly EnumUI.SlotKey[] PromotionOrder =
    {
        EnumUI.SlotKey.STONE,
        EnumUI.SlotKey.BRONZE,
        EnumUI.SlotKey.IRON,
        EnumUI.SlotKey.SILVER,
        EnumUI.SlotKey.GOLD,
        EnumUI.SlotKey.MITHRIL,
        EnumUI.SlotKey.ORICHALCUM,
        EnumUI.SlotKey.ARCANITE,
        EnumUI.SlotKey.ADAMANTITE,
        EnumUI.SlotKey.ETHER
    };

    public static int GetPromotionIndex(EnumUI.SlotKey key)
    {
        for (int i = 0; i < PromotionOrder.Length; i++)
        {
            if (PromotionOrder[i] == key) return i;
        }
        return -1;
    }

    public static EnumUI.SlotKey GetPromotionKeyByIndex(int index)
    {
        if (index < 0) index = 0;
        if (index >= PromotionOrder.Length) index = PromotionOrder.Length - 1;
        return PromotionOrder[index];
    }

    public static EnumUI.SlotKey GetNextPromotionKey(EnumUI.SlotKey currentKey)
    {
        int idx = GetPromotionIndex(currentKey);
        if (idx < 0) return PromotionOrder[0];

        int next = idx + 1;
        if (next >= PromotionOrder.Length) return PromotionOrder[PromotionOrder.Length - 1];
        return PromotionOrder[next];
    }
}
