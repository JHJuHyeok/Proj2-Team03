using UnityEngine;

/*
[승문]
SkillGridPanelController
-스킬 배치용 그리드 패널 Show / Hide 전용
-메인메뉴 숨김/표시 상태에 따라 "지정한 자리(Transform)"로 이동/부모변경 가능

-규칙:
  1) 메인메뉴가 완전히 내려가 있으면(숨김) => 그리드 패널은 항상 보임
  2) 메인메뉴가 보이면 => 스킬탭일 때만 보임(그 외 탭이면 숨김)
*/
public class SkillGridPanelController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform panelRoot;

    [Header("Main Menu Ref")]
    [SerializeField] private BottomPanelSlideToggle bottomMenu;

    [Header("Position By MainMenu State (Use Anchors)")]
    [Tooltip("체크하면 메인메뉴 상태에 따라 아래 Anchor로 패널 위치/부모를 옮김")]
    [SerializeField] private bool useAnchorsByMainMenu = true;

    [Tooltip("메인메뉴가 '보일 때' 패널이 있어야 할 자리(빈 RectTransform)")]
    [SerializeField] private RectTransform anchorWhenMenuShown;

    [Tooltip("메인메뉴가 '완전히 내려갔을 때' 패널이 있어야 할 자리(빈 RectTransform)")]
    [SerializeField] private RectTransform anchorWhenMenuHidden;

    [Header("Behavior")]
    [Tooltip("메인메뉴가 내려가 있으면 그리드를 항상 보이게 함")]
    [SerializeField] private bool forceShowWhenMenuHidden = true;

    [Tooltip("Anchor로 이동할 때, Anchor의 부모로 panelRoot의 부모를 바꿀지 여부")]
    [SerializeField] private bool changeParentToAnchorParent = true;

    private bool isShown;
    public bool IsShown => isShown;

    // 현재 탭이 스킬탭인지(외부에서 알려줌)
    private bool isSkillTab;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = transform as RectTransform;
    }

    private void OnEnable()
    {
        if (bottomMenu != null)
        {
            bottomMenu.OnShownChanged += HandleMenuShownChanged;
            HandleMenuShownChanged(bottomMenu.IsShown);
        }

        EvaluateVisibility();
    }

    private void OnDisable()
    {
        if (bottomMenu != null)
            bottomMenu.OnShownChanged -= HandleMenuShownChanged;
    }

    // NestedScrollManager에서 탭 바뀔 때 호출해줄 함수
    public void SetIsSkillTab(bool value)
    {
        isSkillTab = value;
        EvaluateVisibility();
    }

    private void HandleMenuShownChanged(bool menuShown)
    {
        ApplyAnchor(menuShown);
        EvaluateVisibility();
    }

    private void ApplyAnchor(bool menuShown)
    {
        if (!useAnchorsByMainMenu) return;
        if (panelRoot == null) return;

        RectTransform targetAnchor = menuShown ? anchorWhenMenuShown : anchorWhenMenuHidden;
        if (targetAnchor == null) return;

        // 1) 부모를 Anchor의 부모로 옮길지
        if (changeParentToAnchorParent && targetAnchor.parent is RectTransform)
        {
            // worldPositionStays=false 로 UI 스케일/앵커 깨지는 것 최소화
            panelRoot.SetParent(targetAnchor.parent, false);
        }

        // 2) 자리(Anchor) 위치에 맞춰 이동
        // 같은 부모 기준이면 anchoredPosition 맞추기가 가장 안전함
        panelRoot.anchoredPosition = targetAnchor.anchoredPosition;
        panelRoot.anchorMin = targetAnchor.anchorMin;
        panelRoot.anchorMax = targetAnchor.anchorMax;
        panelRoot.pivot = targetAnchor.pivot;

        // 원하면 sizeDelta도 따라가게 할 수 있음(필요하면 주석 해제)
        // panelRoot.sizeDelta = targetAnchor.sizeDelta;
    }

    private void EvaluateVisibility()
    {
        bool menuShown = (bottomMenu == null) ? true : bottomMenu.IsShown;

        // 1) 메인메뉴가 내려가 있으면 => 항상 보임
        if (!menuShown && forceShowWhenMenuHidden)
        {
            Show();
            return;
        }

        // 2) 메인메뉴가 보이면 => 스킬탭일 때만
        if (isSkillTab) Show();
        else Hide();
    }

    public void Show()
    {
        if (panelRoot == null) return;
        if (isShown) return;

        isShown = true;
        panelRoot.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot == null) return;
        if (!isShown) return;

        isShown = false;
        panelRoot.gameObject.SetActive(false);
    }
    public void Toggle()
    {
        if (IsShown) Hide();
        else Show();
    }
}
