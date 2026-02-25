using UnityEngine;
using UnityEngine.UI;

/*
[승문]
TabManager
- 탭 클릭으로 패널 전환
- ScrollRect content 자동 교체
- 스크롤 사이즈 자동 갱신
*/
public class TabManager : MonoBehaviour
{
    [Header("Tab Panels")]
    [SerializeField] private GameObject[] tab;

    [Header("Tab Button")]
    [SerializeField] private Image[] tabBtnImage;

    [Header("Tab Button Colors")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color dimColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("ScrollRect (선택)")]
    [SerializeField] private ScrollRect scrollRect;

    private int currentIndex = -1;

    private void OnEnable()
    {
        TabClick(0);
    }

    public void TabClick(int index)
    {
        if (tab == null || tab.Length == 0) return;
        if (index < 0 || index >= tab.Length) return;

        currentIndex = index;

        // 패널 전환
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i] != null)
                tab[i].SetActive(i == index);
        }

        // 버튼 색상 처리
        for (int i = 0; i < tabBtnImage.Length; i++)
        {
            if (tabBtnImage[i] == null) continue;
            tabBtnImage[i].color = (i == index) ? selectedColor : dimColor;
        }

        // ScrollRect content 자동 교체
        if (scrollRect != null)
        {
            RectTransform newContent = tab[index].GetComponentInChildren<RectTransform>(true);

            if (newContent != null)
            {
                scrollRect.content = newContent;

                // 레이아웃 강제 갱신 (여백 문제 해결)
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(newContent);

                // 스크롤 위치 초기화 (위로 이동)
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}