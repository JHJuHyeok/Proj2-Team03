using TMPro;
using UnityEngine;

/*
[승문]
StatHeaderUI
-탭(EnumUI.TabType)에 따라 헤더 문구 자동 세팅
-색상은 프리팹/Variant에서 유지
-수치는 SetMaxLv로 외부 데이터에서 주입
*/
public class StatHeaderUI : MonoBehaviour
{
    [Header("Tab")]
    [SerializeField] private EnumUI.TabType tabType;

    [Header("Text References")]
    [SerializeField] private TMP_Text statText;   // 좌측 헤더 텍스트
    [SerializeField] private TMP_Text maxLvText;  // 우측 Max Lv

    [Header("Text Style")]
    [SerializeField] private Color statColor = Color.white;
    [SerializeField] private Color maxLvColor = Color.cyan;

    [Header("Debug")]
    [SerializeField] private long debugMaxLv = 1000;

    private void Awake()
    {
        ApplyStyle();
        ApplyHeaderText();
    }

    private void Start()
    {
        SetMaxLv(debugMaxLv);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyStyle();
        ApplyHeaderText();
    }
#endif

    private void ApplyStyle()
    {
        if (statText != null) statText.color = statColor;
        if (maxLvText != null) maxLvText.color = maxLvColor;
    }

    //탭에 따른 헤더 텍스트 자동
    private void ApplyHeaderText()
    {
        if (statText == null) return;

        if (tabType == EnumUI.TabType.Enhance)
        {
            statText.text = "강화";
        }
        else if (tabType == EnumUI.TabType.Growth)
        {
            statText.text = "성장";
        }
        else if (tabType == EnumUI.TabType.Promotion)
        {
            statText.text = "승급";
        }
    }

    //Max Lv 수치 갱신(데이터 연동 지점)
    public void SetMaxLv(long maxLv)
    {
        if (maxLvText == null) return;
        maxLvText.text = "Max Lv." + maxLv.ToString("N0");
    }
}
