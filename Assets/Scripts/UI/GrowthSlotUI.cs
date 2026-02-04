using TMPro;
using UnityEngine;

/*
[승문]
GrowthSlotUI
-성장 탭 슬롯 전용
-표시: (영문명 STR/HP...) (Max Lv.000) (Lv.000)
-하단: (한글명) (+000 -> +000)
*/
public class GrowthSlotUI : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private EnumUI.SlotKey key;

    [Header("Texts")]
    [SerializeField] private TMP_Text shortNameText;     // STR/HP...
    [SerializeField] private TMP_Text maxLvText;         // Max Lv.000
    [SerializeField] private TMP_Text levelText;         // Lv.000
    [SerializeField] private TMP_Text bottomInfoText;    // 공격력 +000 -> +000

    private void Awake()
    {
        ApplyStaticText();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyStaticText();
    }
#endif

    private void ApplyStaticText()
    {
        if (shortNameText != null)
            shortNameText.text = EnumUITables.GetTopLabel(key);
    }

    public void SetMaxLv(int maxLv)
    {
        if (maxLvText == null) return;
        maxLvText.text = "Max Lv." + maxLv.ToString("N0");
    }

    public void SetLevel(int level)
    {
        if (levelText == null) return;
        levelText.text = "Lv." + level.ToString().PadLeft(3, '0');
    }

    public void SetBottomValueChange(long before, long after)
    {
        if (bottomInfoText == null) return;

        string ko = EnumUITables.GetKoreanName(key);
        bottomInfoText.text = ko + " +" + before.ToString("N0") + " -> +" + after.ToString("N0");
    }
}
