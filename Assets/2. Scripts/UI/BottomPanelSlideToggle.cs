using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*[승문]
BottomPanelSlideToggle
-하단 UI 루트(bottomRoot)를 아래로 내려 숨기기/올리기
-중요: "높이만큼 이동"이 아니라, 코너 계산으로 "부모 화면 밖"으로 완전히 빼는 방식
-숨김 조건:
  1) bottomRoot 밖을 터치/클릭하면 숨김(단, 예외 영역(ignoreRoots)은 제외)
  2) 현재 선택된 하단 탭 버튼을 한 번 더 누르면 숨김
-탭 전환은 nestedScrollManager.TabClick(n) 호출로 처리
-탭바(BTN Tab)는 별도 오브젝트로 남겨두고, ignoreRoots에 넣어서 자동 숨김 방지

★추가:
- OnShownChanged 이벤트(외부에서 “보임/숨김” 상태를 구독 가능)
- "UI를 눌렀을 때는 바깥터치 숨김을 무시" 옵션(추천)
  -> 스킬그리드 패널/다른 버튼들을 누를 때 메인메뉴가 내려가버리는 문제 해결용
*/
public class BottomPanelSlideToggle : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform bottomRoot;//내려서 숨길 루트(예: PageContainer)
    [SerializeField] private Canvas rootCanvas;//Screen Space Overlay면 null이어도 동작

    [Header("Ignore Touch Roots")]
    [SerializeField] private RectTransform[] ignoreRoots;//예: BTN Tab 루트(탭바), TopBar, SkillSetPanel 등

    [Header("Managers")]
    [SerializeField] private NestedScrollManager nestedScrollManager;//탭 전환 담당

    [Header("Tuning")]
    [SerializeField] private float slideLerp = 0.2f;//0이면 즉시
    [SerializeField] private float hiddenExtra = 0f;//완전히 아래로 더 내리고 싶으면 +값(픽셀)
    [SerializeField] private float arriveEpsilon = 0.5f;//목표값 근처면 스냅

    [Header("Lock")]
    [SerializeField] private bool lockWhenPopupOpen = true;

    [Header("Touch Exception (Recommended)")]
    [Tooltip("체크 시, UI(버튼/패널 등)를 누르면 '바깥터치 숨김'이 동작하지 않음")]
    [SerializeField] private bool ignoreWhenTouchingUI = true;

    [Tooltip("UI Raycast에 사용할 GraphicRaycaster (비우면 RootCanvas에서 자동 탐색)")]
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    // ★ 추가: 보여짐 상태 변경 이벤트
    public event Action<bool> OnShownChanged;

    // ★ 추가: 현재 상태 외부 조회
    public bool IsShown => isShown;

    private bool isShown = true;
    private int currentTabIndex = -1;

    private float shownY;
    private float hiddenY;
    private bool positionsCached;

    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>(16);
    private PointerEventData pointerEventData;

    private void Awake()
    {
        if (bottomRoot == null)
        {
            bottomRoot = transform as RectTransform;
        }
    }

    private void Start()
    {
        if (graphicRaycaster == null && rootCanvas != null)
        {
            graphicRaycaster = rootCanvas.GetComponent<GraphicRaycaster>();
        }

        //레이아웃이 확정된 뒤 코너 기준으로 위치 캐싱
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

            // 0) UI를 누른 경우는 바깥터치 숨김 무시(추천)
            if (ignoreWhenTouchingUI && IsPointerOverAnyUI(screenPos))
            {
                SlideUpdate();
                return;
            }

            // 1) 탭바/예외영역이면 자동 숨김 금지
            if (IsPointInsideAny(ignoreRoots, screenPos))
            {
                SlideUpdate();
                return;
            }

            // 2) bottomRoot 밖이면 숨김
            if (!IsPointInside(bottomRoot, screenPos))
            {
                Hide();
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
            //부모가 없으면 높이 기반 fallback
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
        if (Input.GetMouseButtonDown(0)) return true;
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

    // ★ 추가: "UI 위를 눌렀는지" 체크 (버튼/패널 누르면 바깥터치 숨김 무시)
    private bool IsPointerOverAnyUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        if (graphicRaycaster == null)
        {
            // rootCanvas가 비었으면 "이 컴포넌트가 붙은 캔버스"라도 찾아봄
            Canvas c = GetComponentInParent<Canvas>();
            if (c != null) graphicRaycaster = c.GetComponent<GraphicRaycaster>();
        }

        if (graphicRaycaster == null) return false;

        if (pointerEventData == null)
        {
            pointerEventData = new PointerEventData(EventSystem.current);
        }

        pointerEventData.position = screenPos;
        raycastResults.Clear();

        graphicRaycaster.Raycast(pointerEventData, raycastResults);

        // 아무 UI도 안 맞았으면 false
        return raycastResults.Count > 0;
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
        if (isShown) return;

        isShown = true;
        OnShownChanged?.Invoke(true);
    }

    public void Hide()
    {
        if (!positionsCached) RebuildAndCachePositions();
        if (!isShown) return;

        isShown = false;
        OnShownChanged?.Invoke(false);
    }

    public void Toggle()
    {
        if (!positionsCached) RebuildAndCachePositions();

        isShown = !isShown;
        OnShownChanged?.Invoke(isShown);
    }

    private void ShowImmediate()
    {
        if (!positionsCached) RebuildAndCachePositions();
        isShown = true;
        SetY(shownY);
        OnShownChanged?.Invoke(true);
    }

    public void HideImmediate()
    {
        if (!positionsCached) RebuildAndCachePositions();
        isShown = false;
        SetY(hiddenY);
        OnShownChanged?.Invoke(false);
    }

    public void RefreshLayoutAndPositions()
    {
        positionsCached = false;
        RebuildAndCachePositions();

        float targetY = isShown ? shownY : hiddenY;
        SetY(targetY);
    }

    //해상도/안전영역/캔버스 크기 바뀌면 코너 기준 재계산 필요
    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying) return;
        positionsCached = false;
    }
}
