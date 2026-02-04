using TMPro;
using UnityEngine;

/*
[승문]
EnhanceSlotUI
-강화 탭 슬롯 전용
-표시: (한글명) (Lv.000) (000 -> 000) (골드)
*/
public class EnhanceSlotUI : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private EnumUI.SlotKey key;

    [Header("Texts")]
    [SerializeField] private TMP_Text abilityNameText;   // 공격력/체력...
    [SerializeField] private TMP_Text levelText;         // Lv.000
    [SerializeField] private TMP_Text deltaText;         // 000 -> 000

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
        if (abilityNameText != null)
            abilityNameText.text = EnumUITables.GetKoreanName(key);
    }

    public void SetLevel(int level)
    {
        if (levelText == null) return;
        levelText.text = "Lv." + level.ToString().PadLeft(3, '0');
    }

    public void SetValueChange(long before, long after)
    {
        if (deltaText == null) return;
        deltaText.text = before.ToString("N0") + " -> " + after.ToString("N0");
    }
}
