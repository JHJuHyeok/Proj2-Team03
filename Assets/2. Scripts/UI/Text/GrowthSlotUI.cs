using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public EnumUI.SlotKey key;

    [Header("Icon (Prefab Local)")]
    [SerializeField] private Image iconImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text shortNameText;
    [SerializeField] private TMP_Text maxLvText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text bottomInfoText;

    [Header("Format")]
    [SerializeField] private string maxLvPrefix = "Max Lv.";
    [SerializeField] private string lvPrefix = "Lv.";
    [SerializeField] private string arrow = " -> ";
    [SerializeField] private string numFormat = "N0";

    [HideInInspector]
    public List<int> maxLevels = new List<int> { 1000, 1000, 1000, 200, 1000, 200, 200 };

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
        {
            shortNameText.text = EnumUITables.GetTopLabel(key);
        }
    }

    // 컨테이너 자동 배정에서 호출
    public void SetKey(EnumUI.SlotKey newKey, Sprite iconSprite)
    {
        key = newKey;
        ApplyStaticText();

        if (iconImage != null && iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }
    }

    public void SetMaxLv(int maxLv)
    {
        if (maxLvText == null) return;
        maxLvText.text = maxLvPrefix + maxLv.ToString("N0");
    }

    public void SetLevel(int level)
    {
        if (levelText == null) return;
        levelText.text = lvPrefix + level.ToString("000");
    }

    public void SetBottomValueChange(double before, double after)
    {
        if (bottomInfoText == null) return;

        string ko = EnumUITables.GetKoreanName(key);
        bottomInfoText.text = ko + " +" + before.ToString(numFormat) + arrow + "+" + after.ToString(numFormat);
    }
}
