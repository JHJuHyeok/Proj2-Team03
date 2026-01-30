using UnityEngine;
using UnityEngine.UI;

/*
NestedScrollManager
-하단 탭 버튼 클릭으로만 탭을 전환(가로 드래그/스크롤 사용 안 함)
-탭 개수는 btnRect.Length 기준으로 자동 결정
-선택 표시는 tabSlider(value 0~1)로만 처리
-탭 전환 시 세로 스크롤 초기화는 verticalScrollbars로 처리(옵션)
*/

public class NestedScrollManager : MonoBehaviour
{
    [Header("Select 표시")]
    [SerializeField] private Slider tabSlider; //0~1로 선택 위치 표시

    [Header("Tabs(개수 기준)")]
    [SerializeField] private RectTransform[] btnRect; //탭 개수(예: 6개)

    [Header("Pages")]
    [SerializeField] private GameObject[] pages; //각 탭 패널(Character/Skill/..)

    private float[] pos;
    private int size;
    private int currentIndex;

    public int CurrentIndex => currentIndex;
    public int Size => size;

    private void Awake()
    {
        BuildPositions();
        ValidateCounts();
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

    //팀 작업 중 흔한 실수(배열 길이 불일치)를 조기에 잡기
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

        //verticalScrollbars는 옵션이므로 길이 불일치 경고는 생략
    }

    //외부에서 탭 전환 호출(버튼 클릭 등)
    public void TabClick(int n)
    {
        if (pos == null || pos.Length == 0) return;

        if (n < 0) n = 0;
        if (n > size - 1) n = size - 1;

        currentIndex = n;

        ApplyPages(currentIndex);
        ApplySlider(currentIndex);
    }

    //겹쳐둔 페이지 중 하나만 활성화
    private void ApplyPages(int index)
    {
        if (pages == null || pages.Length == 0) return;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == index);
            }
        }
    }

    //선택 표시(슬라이더)
    private void ApplySlider(int index)
    {
        if (tabSlider == null) return;

        float v = pos != null && index >= 0 && index < pos.Length ? pos[index] : 0f;
        tabSlider.value = v;
    }
}
