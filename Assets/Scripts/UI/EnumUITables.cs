/*
[승문]
EnumUITables
-EnumUI.SlotKey -> 표시문구 테이블
*/
public static class EnumUITables
{
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
}
