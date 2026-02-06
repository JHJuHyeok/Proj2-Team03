using TMPro;
using UnityEngine;

/*
[승문]
PromotionSlotUI
-승급 탭 슬롯 전용
-표시: (등급 한글) (승급 시 효과 문구) (권장 레벨 00)
*/
public class PromotionSlotUI : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private EnumUI.SlotKey key; // STONE/BRONZE...

    [Header("Texts")]
    [SerializeField] private TMP_Text gradeNameText;     // 스톤/브론즈...
    [SerializeField] private TMP_Text effectText;        // 공격력 x1, 체력 x1
    [SerializeField] private TMP_Text recommendedLvText; // 권장 레벨 00

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
        if (gradeNameText != null)
            gradeNameText.text = EnumUITables.GetKoreanName(key);
    }

    public void SetEffectText(string text)
    {
        if (effectText == null) return;
        effectText.text = text;
    }

    public void SetRecommendedLevel(int level)
    {
        if (recommendedLvText == null) return;
        recommendedLvText.text = "권장 레벨 " + level.ToString("N0");
    }
}
