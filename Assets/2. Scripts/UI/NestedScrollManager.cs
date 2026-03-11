using UnityEngine;
using UnityEngine.UI;

/*[승문]
NestedScrollManager
-하단 BTN Tab 버튼 클릭으로만 탭을 전환
-탭 개수는 btnRect.Length 기준으로 자동 결정
-선택 표시는 tabSlider로만 처리
*/
public class NestedScrollManager : MonoBehaviour
{
    [Header("Select 표시")]
    [SerializeField] private Slider tabSlider;

    [Header("Tabs(개수 기준)")]
    [SerializeField] private RectTransform[] btnRect;

    [Header("Pages")]
    [SerializeField] private GameObject[] pages;

    [Header("Skill Grid (Optional)")]
    [SerializeField] private SkillGridPanelController skillGridPanel;

    [Tooltip("스킬탭 인덱스(하단 탭 기준). 스킬이 1번이면 1")]
    [SerializeField] private int skillTabIndex = 1;

    [Header("Bottom Panel Refresh (Optional)")]
    [SerializeField] private BottomPanelSlideToggle bottomPanelSlideToggle;

    private float[] pos;
    private int size;
    private int currentIndex;

    private void Awake()
    {
        BuildPositions();
        ValidateCounts();
        TabClick(0);
    }

    private void BuildPositions()
    {
        size = btnRect != null ? btnRect.Length : 0;

        if (size <= 0)
        {
            pos = null;
            return;
        }

        pos = new float[size];

        if (size == 1)
        {
            pos[0] = 0f;
            return;
        }

        float distance = 1f / (size - 1);
        for (int i = 0; i < size; i++)
        {
            pos[i] = distance * i;
        }
    }

    private void ValidateCounts()
    {
        int a = btnRect != null ? btnRect.Length : 0;

        if (a == 0)
        {
            Debug.LogError("[NestedScrollManager] btnRect is empty");
        }

        if (pages != null && pages.Length != 0 && pages.Length != a)
        {
            Debug.LogError("[NestedScrollManager] pages length must match tab count");
        }
    }

    public void TabClick(int n)
    {
        if (pos == null || pos.Length == 0) return;

        if (n < 0) n = 0;
        if (n > size - 1) n = size - 1;

        currentIndex = n;

        // 페이지 전환
        if (pages != null && pages.Length > 0)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == currentIndex);
                }
            }
        }

        // 선택 표시
        if (tabSlider != null)
        {
            tabSlider.value = pos[currentIndex];
        }

        // 스킬탭 여부 전달
        if (skillGridPanel != null)
        {
            bool inSkillTab = (currentIndex == skillTabIndex);
            skillGridPanel.SetIsSkillTab(inSkillTab);
        }

        // 탭 내용이 달라졌으니 BottomPanel 숨김 위치 재계산
        if (bottomPanelSlideToggle != null)
        {
            Canvas.ForceUpdateCanvases();
            bottomPanelSlideToggle.RefreshLayoutAndPositions();
        }
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}