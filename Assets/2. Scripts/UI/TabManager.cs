using UnityEngine;
using UnityEngine.UI;

/*
[승문]
TabManager
-탭 클릭으로 패널 전환
-추가로 같이 전환돼야 하는 보조 패널(extraTab)도 동일 인덱스로 전환
-ScrollRect content 자동 교체
-스크롤 사이즈 자동 갱신
-기본 탭/마지막 탭 유지 옵션
*/
public class TabManager : MonoBehaviour
{
    [Header("Tab Panels")]
    [SerializeField] private GameObject[] tab;//메인 탭 패널들

    [Header("Extra Panels(Optional)")]
    [SerializeField] private GameObject[] extraTab;//메인탭과 같이 전환될 보조 패널들

    [Header("Tab Button")]
    [SerializeField] private Image[] tabBtnImage;//탭 버튼 이미지

    [Header("Tab Button Colors")]
    [SerializeField] private Color selectedColor = Color.white;//선택 색
    [SerializeField] private Color dimColor = new Color(1f, 1f, 1f, 0.45f);//비선택 색

    [Header("ScrollRect(선택)")]
    [SerializeField] private ScrollRect scrollRect;//스크롤렉트

    [Header("Default/Keep")]
    [SerializeField, Min(0)] private int defaultIndex = 0;//기본 탭 인덱스
    [SerializeField] private bool keepLastIndexOnEnable = false;//OnEnable시 마지막 탭 유지

    private int currentIndex = -1;

    private void OnEnable()
    {
        int idx = defaultIndex;

        if (keepLastIndexOnEnable && currentIndex >= 0)
        {
            idx = currentIndex;
        }

        TabClick(idx);
    }

    //탭 클릭 처리
    public void TabClick(int index)
    {
        if (tab == null || tab.Length == 0) return;
        if (index < 0 || index >= tab.Length) return;

        currentIndex = index;

        ApplyTabPanels(index);
        ApplyExtraPanels(index);
        ApplyButtonColors(index);
        ApplyScrollContent(index);
    }

    //메인 탭 패널 전환
    private void ApplyTabPanels(int index)
    {
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i] == null)
            {
                continue;
            }

            tab[i].SetActive(i == index);
        }
    }

    //보조 패널 전환(같은 인덱스로 동기화)
    private void ApplyExtraPanels(int index)
    {
        if (extraTab == null || extraTab.Length == 0)
        {
            return;
        }

        if (extraTab.Length != tab.Length)
        {
            Debug.LogWarning("[TabManager] extraTab length mismatch. extraTab will be clamped by min length.");
        }

        int len = extraTab.Length < tab.Length ? extraTab.Length : tab.Length;

        for (int i = 0; i < len; i++)
        {
            if (extraTab[i] == null)
            {
                continue;
            }

            extraTab[i].SetActive(i == index);
        }
    }

    //버튼 색상 처리
    private void ApplyButtonColors(int index)
    {
        if (tabBtnImage == null || tabBtnImage.Length == 0)
        {
            return;
        }

        for (int i = 0; i < tabBtnImage.Length; i++)
        {
            if (tabBtnImage[i] == null)
            {
                continue;
            }

            tabBtnImage[i].color = (i == index) ? selectedColor : dimColor;
        }
    }

    //ScrollRect content 자동 교체
    private void ApplyScrollContent(int index)
    {
        if (scrollRect == null)
        {
            return;
        }

        if (tab[index] == null)
        {
            return;
        }

        RectTransform newContent = tab[index].GetComponentInChildren<RectTransform>(true);
        if (newContent == null)
        {
            Debug.LogWarning("[TabManager] Scroll content not found in tab panel.");
            return;
        }

        scrollRect.content = newContent;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(newContent);
        scrollRect.verticalNormalizedPosition = 1f;
    }
}