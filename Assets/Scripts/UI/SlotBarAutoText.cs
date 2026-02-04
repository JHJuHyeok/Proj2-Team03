using TMPro;
using UnityEngine;

/*
[승문]
SlotBarAutoText
-수동 배치된 슬롯바에 붙여서 문구만 자동 세팅
-EnumUI.SlotKey 사용(강화/성장 고유 키, 승급은 접두어 없음)
-색상/폰트/스타일은 프리팹 설정 그대로 유지
*/
public class SlotBarAutoText : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private EnumUI.SlotKey key;

    [Header("Text References")]
    [SerializeField] private TMP_Text topLabelText;    // 상단: STR / HP / 등급
    [SerializeField] private TMP_Text bottomNameText;  // 하단: 공격력 / 체력 / 등급명

    private void Awake()
    {
        ApplyText();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyText();
    }
#endif

    public void ApplyText()
    {
        if (topLabelText != null)
            topLabelText.text = EnumUITables.GetTopLabel(key);

        if (bottomNameText != null)
            bottomNameText.text = EnumUITables.GetKoreanName(key);
    }
}
