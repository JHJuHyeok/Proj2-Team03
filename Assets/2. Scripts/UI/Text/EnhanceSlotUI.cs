using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
EnhanceSlotUI
-강화 탭 슬롯 전용(기본 틀)
-표시:
  어빌리티 이름(한글: 공격력/체력...)
  현재레벨(Lv.000)
  강화했을때 바뀔수치(000 -> 000)
  강화에 필요한 골드(코스트)
-컨테이너 자동 배정을 위해 SetKey(EnumUI.SlotKey) 제공
*/
public class EnhanceSlotUI : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private EnumUI.SlotKey key;

    [Header("Icon (Prefab Local)")]
    [SerializeField] private Image iconImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text changeText;
    [SerializeField] private TMP_Text costGoldText;

    [Header("Format")]
    [SerializeField] private string lvPrefix = "Lv.";
    [SerializeField] private string arrow = " -> ";

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

    private void ApplyStaticText()
    {
        if (abilityNameText != null)
        {
            abilityNameText.text = EnumUITables.GetKoreanName(key);
        }
    }

    public void SetLevel(int level)
    {
        if (levelText == null) return;
        levelText.text = lvPrefix + level.ToString("000");
    }

    public void SetValueChange(long before, long after)
    {
        if (changeText == null) return;
        changeText.text = before.ToString("N0") + arrow + after.ToString("N0");
    }

    public void SetCostGold(long cost)
    {
        if (costGoldText == null) return;
        costGoldText.text = cost.ToString("N0");
    }
}
