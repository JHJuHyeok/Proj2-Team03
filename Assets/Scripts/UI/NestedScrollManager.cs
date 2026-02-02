using UnityEngine;
using UnityEngine.UI;

/*[승문]
NestedScrollManager
-하단 BTN Tab 버튼 클릭으로만 탭을 전환
-탭 개수는 btnRect.Length 기준으로 자동 결정
-선택 표시는 tabSlider로만 처리
-탭 전환 시 세로 스크롤 초기화는 verticalScrollbars로 처리(옵션)
*/
public class NestedScrollManager : MonoBehaviour
{
    [Header("Select 표시")]
    [SerializeField] private Slider tabSlider;        //0~1로 위치 표시(선택바)

    [Header("Tabs(개수 기준)")]
    [SerializeField] private RectTransform[] btnRect; //탭 개수(6개)

    [Header("Pages")]
    [SerializeField] private GameObject[] pages;             //각 탭 패널(Character/Skill/..)

    private float[] pos;
    private int size;
    private int currentIndex;

    private void Awake()
    {
        BuildPositions();
        ValidateCounts();
        TabClick(0);
    }

    //탭 개수에 맞춰 0~1 위치 배열 만들기
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

    //배열 실수 방지
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

    //하단 버튼에서 호출
    public void TabClick(int n)
    {
        if (pos == null || pos.Length == 0) return;

        if (n < 0) n = 0;
        if (n > size - 1) n = size - 1;

        currentIndex = n;

        //페이지 전환(겹쳐둔 패널 중 하나만 활성화)
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

        //선택 표시(슬라이더)
        if (tabSlider != null)
        {
            tabSlider.value = pos[currentIndex];
        }
    }

}
