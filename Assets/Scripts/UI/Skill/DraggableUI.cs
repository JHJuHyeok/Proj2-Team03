using UnityEngine;
using UnityEngine.EventSystems;

/*
DraggableUI
- 오른쪽 큰 프리뷰 블록(드래그 소스)
- 드래그 중: overlay.ShowPreview로 놓을 위치 표시
- 드롭 시: overlay.Place로 확정(실패하면 원위치)
- 회전/스폰은 TestController가 담당
*/

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform selfRect;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private SkillGridBlockOverlay overlay;
    [SerializeField] private SkillGridBlockTestController controller;

    private RectTransform parentRect;
    private Vector2 startAnchoredPos;

    private void Awake()
    {
        if (selfRect == null)
        {
            selfRect = transform as RectTransform;
        }

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        if (selfRect != null)
        {
            parentRect = selfRect.parent as RectTransform;
        }
    }

    public void SetRefs(SkillGridBlockOverlay ov, SkillGridBlockTestController tc)
    {
        overlay = ov;
        controller = tc;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (selfRect == null)
        {
            return;
        }

        startAnchoredPos = selfRect.anchoredPosition;
        selfRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (selfRect == null || parentRect == null)
        {
            return;
        }

        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            selfRect.anchoredPosition = localPos;
        }

        if (overlay == null || controller == null)
        {
            return;
        }

        SkillBlockData data = controller.GetCurrentBlockData();
        if (data == null)
        {
            return;
        }

        Vector2Int cell;
        Camera cam = rootCanvas != null ? rootCanvas.worldCamera : null;

        bool ok = overlay.TryGetCellFromScreen(eventData.position, cam, out cell);
        if (!ok)
        {
            overlay.HidePreview();
            return;
        }

        overlay.ShowPreview(cell, data);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (overlay != null)
        {
            overlay.HidePreview();
        }

        if (overlay == null || controller == null)
        {
            ReturnToStart();
            return;
        }

        SkillBlockData data = controller.GetCurrentBlockData();
        if (data == null)
        {
            ReturnToStart();
            return;
        }

        Vector2Int cell;
        Camera cam = rootCanvas != null ? rootCanvas.worldCamera : null;

        bool ok = overlay.TryGetCellFromScreen(eventData.position, cam, out cell);
        if (!ok)
        {
            ReturnToStart();
            return;
        }

        bool placed = overlay.Place(cell, data);
        if (!placed)
        {
            ReturnToStart();
            return;
        }

        controller.OnPlacedSuccessfully();
        ReturnToStart();
    }

    private void ReturnToStart()
    {
        if (selfRect == null)
        {
            return;
        }

        selfRect.anchoredPosition = startAnchoredPos;
    }
}
