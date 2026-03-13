using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*[승문]
BottomPanelSlideToggle
-하단 UI 루트(bottomRoot)를 아래로 내려 숨기기/올리기
-탭을 바꾸거나 레이아웃이 바뀌어도 원래 shownY는 유지
-현재 위치 기준이 아니라 "보이는 위치" 기준으로 hiddenY를 다시 계산
-탭별로 추가 숨김값(runtimeHiddenExtra)도 적용 가능
*/
public class BottomPanelSlideToggle : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform bottomRoot;
    [SerializeField] private Canvas rootCanvas;

    [Header("Ignore Touch Roots")]
    [SerializeField] private RectTransform[] ignoreRoots;

    [Header("Managers")]
    [SerializeField] private NestedScrollManager nestedScrollManager;

    [Header("Tuning")]
    [SerializeField] private float slideLerp = 0.2f;
    [SerializeField] private float hiddenExtra = 0f;
    [SerializeField] private float arriveEpsilon = 0.5f;

    [Header("Lock")]
    [SerializeField] private bool lockWhenPopupOpen = true;

    [Header("Touch Exception (Recommended)")]
    [SerializeField] private bool ignoreWhenTouchingUI = false;

    [Tooltip("UI Raycast에 사용할 GraphicRaycaster (비우면 RootCanvas에서 자동 탐색)")]
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    public event Action<bool> OnShownChanged;
    public bool IsShown => isShown;

    private bool isShown = true;
    private int currentTabIndex = -1;

    private float shownY;
    private float hiddenY;
    private bool positionsCached;
    private bool shownYInitialized;

    // 탭별로 추가로 더 숨길 양
    private float runtimeHiddenExtra = 0f;

    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>(16);
    private PointerEventData pointerEventData;

    private void OnEnable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStateChanged += OnCombatStateChanged;
    }

    private void OnDisable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStateChanged -= OnCombatStateChanged;
    }

    private void OnCombatStateChanged(CombatState state)
    {
        if (state == CombatState.Adventure)
            Hide();
    }

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

        RebuildAndCachePositions();
        ShowImmediate();
    }

    private void Update()
    {
        if (bottomRoot == null) return;

        if (lockWhenPopupOpen && PopupManager.Instance != null && PopupManager.Instance.IsOpenAny())
        {
            SlideUpdate();
            return;
        }

        if (isShown && IsPointerDown())
        {
            Vector2 screenPos = GetPointerScreenPosition();

            if (ignoreWhenTouchingUI && IsPointerOverAnyUI(screenPos))
            {
                SlideUpdate();
                return;
            }

            if (IsPointInsideAny(ignoreRoots, screenPos))
            {
                SlideUpdate();
                return;
            }

            if (!IsPointInside(bottomRoot, screenPos))
            {
                Hide();
            }
        }

        SlideUpdate();
    }

    private void RebuildAndCachePositions()
    {
        if (bottomRoot == null) return;

        Canvas.ForceUpdateCanvases();

        // shownY는 처음 한 번만 저장
        if (!shownYInitialized)
        {
            shownY = bottomRoot.anchoredPosition.y;
            shownYInitialized = true;
        }

        hiddenY = CalculateHiddenYFromShownState();
        positionsCached = true;
    }

    /// <summary>
    /// 현재 위치와 상관없이, "보이는 위치(shownY)"를 기준으로 hiddenY를 계산
    /// </summary>
    private float CalculateHiddenYFromShownState()
    {
        if (bottomRoot == null) return 0f;

        Vector2 originalPos = bottomRoot.anchoredPosition;

        // 계산용으로 잠깐 shown 위치로 이동
        SetY(shownY);
        Canvas.ForceUpdateCanvases();

        float result = CalculateHiddenYByCorners();

        // 원래 위치 복원
        bottomRoot.anchoredPosition = originalPos;
        Canvas.ForceUpdateCanvases();

        return result;
    }

    private float CalculateHiddenYByCorners()
    {
        RectTransform parent = bottomRoot.parent as RectTransform;
        if (parent == null)
        {
            return shownY - bottomRoot.rect.height - (hiddenExtra + runtimeHiddenExtra);
        }

        Vector3[] panel = new Vector3[4];
        Vector3[] par = new Vector3[4];

        bottomRoot.GetWorldCorners(panel);
        parent.GetWorldCorners(par);

        float panelTopWorld = panel[1].y;
        float parentBottomWorld = par[0].y;

        float deltaWorldY = (parentBottomWorld - panelTopWorld) - (hiddenExtra + runtimeHiddenExtra);
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

    private bool IsPointerOverAnyUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        if (graphicRaycaster == null)
        {
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

        return raycastResults.Count > 0;
    }

    public void OnTabButtonClick(int n)
    {
        if (isShown && currentTabIndex == n)
        {
            Hide();
            return;
        }

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

    /// <summary>
    /// 탭별로 추가로 더 숨길 양 설정
    /// 예: 모험탭일 때만 120
    /// </summary>
    public void SetRuntimeHiddenExtra(float extra)
    {
        runtimeHiddenExtra = extra;
        RefreshLayoutAndPositions();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying) return;
        positionsCached = false;
    }
}