using UnityEngine;
using UnityEngine.UI;

/*
BottomPanelSlideToggle
-하단 UI 루트(bottomRoot)를 아래로 내려 숨기기/올리기
-숨김 조건:
  1) 바텀 영역(bottomRoot) 밖을 터치/클릭하면 숨김
  2) 현재 선택된 하단 탭 버튼을 한 번 더 누르면 숨김
-탭 전환은 NestedScrollManager.TabClick(n)로 라우팅
-버튼 OnClick은 이 스크립트의 OnTabButtonClick(n)만 연결
*/

public class BottomPanelSlideToggle : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform bottomRoot; //내려서 숨길 루트
    [SerializeField] private Canvas rootCanvas; //Overlay면 없어도 됨(권장: 연결)

    [Header("Managers")]
    [SerializeField] private NestedScrollManager nestedScrollManager; //탭 전환 담당

    [Header("Tuning")]
    [SerializeField] private float slideLerp = 0.2f; //0이면 즉시
    [SerializeField] private float hiddenExtra = 0f; //더 아래로 숨기고 싶으면 +값

    private bool isShown = true;
    private int currentTabIndex = -1;

    private float shownY;
    private float hiddenY;

    private void Awake()
    {
        if (bottomRoot == null)
        {
            bottomRoot = transform as RectTransform;
        }

        CachePositions();
    }

    private void Start()
    {
        ShowImmediate();

        //첫 탭은 여기서 결정(중복 방지)
        OnTabButtonClick(0);
    }

    private void CachePositions()
    {
        if (bottomRoot == null) return;

        shownY = bottomRoot.anchoredPosition.y;

        float h = bottomRoot.rect.height;
        hiddenY = shownY - h - hiddenExtra;
    }

    private void Update()
    {
        if (bottomRoot == null) return;

        //바텀 영역 밖 터치/클릭하면 숨김
        if (isShown && IsPointerDown())
        {
            Vector2 screenPos = GetPointerScreenPosition();

            if (!IsPointInsideBottom(screenPos))
            {
                Hide();
            }
        }

        SlideUpdate();
    }

    private void SlideUpdate()
    {
        float targetY = isShown ? shownY : hiddenY;

        if (slideLerp <= 0f)
        {
            SetY(targetY);
            return;
        }

        float y = Mathf.Lerp(bottomRoot.anchoredPosition.y, targetY, slideLerp);
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
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }

        return Input.mousePosition;
    }

    private bool IsPointInsideBottom(Vector2 screenPos)
    {
        Camera cam = null;

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = rootCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(bottomRoot, screenPos, cam);
    }

    //하단 탭 버튼에서 이 함수만 호출
    public void OnTabButtonClick(int n)
    {
        //같은 탭을 한번 더 누르면 숨김
        if (isShown && currentTabIndex == n)
        {
            Hide();
            return;
        }

        //다른 탭이면 올리고 전환
        Show();

        currentTabIndex = n;

        if (nestedScrollManager != null)
        {
            nestedScrollManager.TabClick(n);
        }
    }

    public void Show()
    {
        isShown = true;
    }

    public void Hide()
    {
        isShown = false;
    }

    public void Toggle()
    {
        isShown = !isShown;
    }

    private void ShowImmediate()
    {
        isShown = true;
        SetY(shownY);
    }

    public void HideImmediate()
    {
        isShown = false;
        SetY(hiddenY);
    }
}
