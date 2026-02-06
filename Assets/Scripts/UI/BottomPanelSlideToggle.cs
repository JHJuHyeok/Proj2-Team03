using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*[승문]
BottomPanelSlideToggle
-하단 UI 루트(bottomRoot)를 아래로 내려 숨기기/올리기
-중요: "높이만큼 이동"이 아니라, 코너 계산으로 "부모 화면 밖"으로 완전히 빼는 방식
-숨김 조건:
  1) bottomRoot 밖을 터치/클릭하면 숨김(단, 예외 영역(ignoreRoots)은 제외)
  2) 현재 선택된 하단 탭 버튼을 한 번 더 누르면 숨김
-탭 전환은 nestedScrollManager.TabClick(n) 호출로 처리
-탭바(BTN Tab)는 별도 오브젝트로 남겨두고, ignoreRoots에 넣어서 자동 숨김 방지
*/
public class BottomPanelSlideToggle : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform bottomRoot;//내려서 숨길 루트(예: Bottom Screen 또는 PageContainer)
    [SerializeField] private Canvas rootCanvas;//Screen Space Overlay면 null이어도 동작

    [Header("Ignore Touch Roots")]
    [SerializeField] private RectTransform[] ignoreRoots;//예: BTN Tab 루트(탭바). 여길 누르면 바깥터치 숨김 금지

    [Header("Managers")]
    [SerializeField] private NestedScrollManager nestedScrollManager;//탭 전환 담당

    [Header("Tuning")]
    [SerializeField] private float slideLerp = 0.2f;//0이면 즉시
    [SerializeField] private float hiddenExtra = 0f;//완전히 아래로 더 내리고 싶으면 +값(픽셀)
    [SerializeField] private float arriveEpsilon = 0.5f;//목표값 근처면 스냅(영원히 덜덜거리는 Lerp 방지)

    [Header("Lock")]
    [SerializeField] private bool lockWhenPopupOpen = true;

    private bool isShown = true;
    private int currentTabIndex = -1;

    private float shownY;
    private float hiddenY;
    private bool positionsCached;

    private void Awake()
    {
        if (bottomRoot == null)
        {
            bottomRoot = transform as RectTransform;
        }
    }

    private void Start()
    {
        //레이아웃이 확정된 뒤(1프레임 뒤) 코너 기준으로 위치 캐싱
        RebuildAndCachePositions();
        ShowImmediate();
    }

    private void Update()
    {
        if (bottomRoot == null) return;

        //팝업이 떠있으면 바텀 자동 숨김 금지
        if (lockWhenPopupOpen && PopupManager.Instance != null && PopupManager.Instance.IsOpenAny())
        {
            SlideUpdate();
            return;
        }

        //바텀 영역 밖 터치/클릭하면 숨김(단, ignoreRoots는 제외)
        if (isShown && IsPointerDown())
        {
            Vector2 screenPos = GetPointerScreenPosition();

            //탭바/예외영역이면 자동 숨김 금지(버튼 클릭 이벤트가 정상 동작하게)
            if (!IsPointInsideAny(ignoreRoots, screenPos))
            {
                if (!IsPointInside(bottomRoot, screenPos))
                {
                    Hide();
                }
            }
        }

        SlideUpdate();
    }


    //레이아웃 확정 후 위치(Shown/Hidden) 재계산
    private void RebuildAndCachePositions()
    {
        if (bottomRoot == null) return;

        Canvas.ForceUpdateCanvases();

        shownY = bottomRoot.anchoredPosition.y;
        hiddenY = CalculateHiddenYByCorners();
        positionsCached = true;
    }

    //부모 기준 코너를 비교해서 "패널의 Top이 부모 Bottom 아래로" 내려가게 숨김 Y 계산
    private float CalculateHiddenYByCorners()
    {
        RectTransform parent = bottomRoot.parent as RectTransform;
        if (parent == null)
        {
            //부모가 없으면 일단 높이 기반으로 fallback
            return shownY - bottomRoot.rect.height - hiddenExtra;
        }

        Vector3[] panel = new Vector3[4];
        Vector3[] par = new Vector3[4];

        bottomRoot.GetWorldCorners(panel);
        parent.GetWorldCorners(par);

        //코너 인덱스: 0=BL, 1=TL, 2=TR, 3=BR
        float panelTopWorld = panel[1].y;
        float parentBottomWorld = par[0].y;

        //panelTop을 parentBottom 아래로 보내기 위한 월드 이동량(음수)
        float deltaWorldY = (parentBottomWorld - panelTopWorld) - hiddenExtra;

        //월드 이동량을 parent 로컬 이동량으로 변환
        float deltaLocalY = parent.InverseTransformVector(new Vector3(0f, deltaWorldY, 0f)).y;

        return shownY + deltaLocalY;
    }

    private void SlideUpdate()
    {
        if (!positionsCached)
        {
            RebuildAndCachePositions();
            if (!positionsCached) return;
        }

        float targetY = isShown ? shownY : hiddenY;

        if (slideLerp <= 0f)
        {
            SetY(targetY);
            return;
        }

        float curY = bottomRoot.anchoredPosition.y;
        float y = Mathf.Lerp(curY, targetY, slideLerp);

        //목표 근처면 스냅(“반만 오고 끝”처럼 보이는 체감 방지)
        if (Mathf.Abs(y - targetY) <= arriveEpsilon)
        {
            y = targetY;
        }

        SetY(y);
    }

    private void SetY(float y)
    {
        Vector2 p = bottomRoot.anchoredPosition;
        p.y = y;
        bottomRoot.anchoredPosition = p;
    }

    private bool IsPointerDown()
    {
        //PC
        if (Input.GetMouseButtonDown(0)) return true;

        //Mobile
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;

        return false;
    }

    private Vector2 GetPointerScreenPosition()
    {
        if (Input.touchCount > 0) return Input.GetTouch(0).position;
        return Input.mousePosition;
    }

    private bool IsPointInside(RectTransform rt, Vector2 screenPos)
    {
        if (rt == null) return false;

        Camera cam = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = rootCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam);
    }

    private bool IsPointInsideAny(RectTransform[] roots, Vector2 screenPos)
    {
        if (roots == null || roots.Length == 0) return false;

        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && IsPointInside(roots[i], screenPos))
            {
                return true;
            }
        }

        return false;
    }

    //하단 탭 버튼에서 이 함수만 호출하면 됨
    public void OnTabButtonClick(int n)
    {
        //같은 탭을 한번 더 누르면 숨김
        if (isShown && currentTabIndex == n)
        {
            Hide();
            return;
        }

        //다른 탭을 누르면 올리고 탭 전환
        Show();

        currentTabIndex = n;

        if (nestedScrollManager != null)
        {
            nestedScrollManager.TabClick(n);
        }
    }

    public void Show()
    {
        if (!positionsCached) RebuildAndCachePositions();
        isShown = true;
    }

    public void Hide()
    {
        if (!positionsCached) RebuildAndCachePositions();
        isShown = false;
    }

    public void Toggle()
    {
        if (!positionsCached) RebuildAndCachePositions();
        isShown = !isShown;
    }

    private void ShowImmediate()
    {
        if (!positionsCached) RebuildAndCachePositions();
        isShown = true;
        SetY(shownY);
    }

    public void HideImmediate()
    {
        if (!positionsCached) RebuildAndCachePositions();
        isShown = false;
        SetY(hiddenY);
    }

    //해상도/안전영역/캔버스 크기 바뀌면 코너 기준 재계산 필요
    private void OnRectTransformDimensionsChange()
    {
        //플레이 중에만(에디터 선택/리사이즈 등으로 계속 호출될 수 있음)
        if (!Application.isPlaying) return;

        //다음 Update에서 자연스럽게 다시 캐싱하도록
        positionsCached = false;
    }
}
