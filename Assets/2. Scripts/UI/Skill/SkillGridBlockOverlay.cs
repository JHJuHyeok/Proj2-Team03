using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
SkillGridBlockOverlay
- 그리드 좌표 계산(스크린 좌표 -> 셀 좌표)
- 블록 배치 가능 여부 검사(범위/잠금/점유)
- 배치 확정 시 점유 처리
- (옵션) 드래그 중 프리뷰 하이라이트(초록/빨강)
*/

public class SkillGridBlockOverlay : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private RectTransform gridRect;       // Skill Grid Board RectTransform
    [SerializeField] private GridLayoutGroup gridLayout;   // Skill Grid Board GridLayoutGroup
    [SerializeField] private int columnCount = 6;          // 6 고정

    [Header("Cells")]
    [SerializeField] private SkillGridCell[] cells;        // 36개(그리드 순서대로)

    [Header("Preview Overlay (Optional)")]
    [SerializeField] private RectTransform overlayRoot;    // 프리뷰 이미지 생성할 부모(Overlay)
    [SerializeField] private Image overlayCellPrefab;      // 반투명 셀 하이라이트 프리팹
    [SerializeField] private Color canPlaceColor = new Color(0f, 1f, 0f, 0.25f);
    [SerializeField] private Color cantPlaceColor = new Color(1f, 0f, 0f, 0.25f);

    private readonly List<Image> previewCells = new List<Image>(8);

    public bool IsReady()
    {
        if (gridRect == null) return false;
        if (gridLayout == null) return false;
        if (cells == null || cells.Length == 0) return false;

        return true;
    }

    // 스크린 좌표 -> 셀 좌표 (0,0 = 좌상단)
    public bool TryGetCellFromScreen(Vector2 screenPos, Camera uiCamera, out Vector2Int cell)
    {
        cell = Vector2Int.zero;

        if (!IsReady())
        {
            return false;
        }

        Vector2 local;
        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, screenPos, uiCamera, out local);
        if (!ok)
        {
            return false;
        }

        Rect r = gridRect.rect;

        // local(중앙 기준) -> 좌상단 기준
        float x = local.x + r.width * 0.5f;
        float y = r.height * 0.5f - local.y;

        x -= gridLayout.padding.left;
        y -= gridLayout.padding.top;

        float stepX = gridLayout.cellSize.x + gridLayout.spacing.x;
        float stepY = gridLayout.cellSize.y + gridLayout.spacing.y;

        if (stepX <= 0f || stepY <= 0f)
        {
            return false;
        }

        int cx = Mathf.FloorToInt(x / stepX);
        int cy = Mathf.FloorToInt(y / stepY);

        int rowCount = Mathf.CeilToInt((float)cells.Length / columnCount);

        if (cx < 0 || cy < 0)
        {
            return false;
        }

        if (cx >= columnCount || cy >= rowCount)
        {
            return false;
        }

        cell = new Vector2Int(cx, cy);
        return true;
    }

    // 배치 가능 여부(범위/잠금/점유)
    public bool CanPlace(Vector2Int startCell, SkillBlockData data)
    {
        if (!IsReady())
        {
            return false;
        }

        if (data == null || data.cells == null || data.cells.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < data.cells.Count; i++)
        {
            Vector2Int p = startCell + data.cells[i];

            if (!IsInBounds(p))
            {
                return false;
            }

            SkillGridCell c = GetCell(p);
            if (c == null)
            {
                return false;
            }

            if (!c.IsUnlocked)
            {
                return false;
            }

            if (c.IsOccupied)
            {
                return false;
            }
        }

        return true;
    }

    // 드롭 확정(점유 처리)
    public bool Place(Vector2Int startCell, SkillBlockData data)
    {
        if (!CanPlace(startCell, data))
        {
            return false;
        }

        for (int i = 0; i < data.cells.Count; i++)
        {
            Vector2Int p = startCell + data.cells[i];
            SkillGridCell c = GetCell(p);
            c.SetOccupied(true);
        }

        return true;
    }

    // 프리뷰 표시(옵션)
    public void ShowPreview(Vector2Int startCell, SkillBlockData data)
    {
        if (overlayRoot == null || overlayCellPrefab == null)
        {
            return;
        }

        if (!IsReady())
        {
            return;
        }

        if (data == null || data.cells == null || data.cells.Count == 0)
        {
            HidePreview();
            return;
        }

        EnsurePreviewCount(data.cells.Count);

        bool can = CanPlace(startCell, data);

        for (int i = 0; i < previewCells.Count; i++)
        {
            previewCells[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < data.cells.Count; i++)
        {
            Vector2Int p = startCell + data.cells[i];

            Image img = previewCells[i];
            img.gameObject.SetActive(true);
            img.color = can ? canPlaceColor : cantPlaceColor;

            RectTransform cellRt = GetCellRect(p);
            if (cellRt != null)
            {
                img.rectTransform.position = cellRt.position;
                img.rectTransform.sizeDelta = cellRt.sizeDelta;
            }
        }
    }

    public void HidePreview()
    {
        for (int i = 0; i < previewCells.Count; i++)
        {
            previewCells[i].gameObject.SetActive(false);
        }
    }

    private void EnsurePreviewCount(int need)
    {
        while (previewCells.Count < need)
        {
            Image img = Instantiate(overlayCellPrefab, overlayRoot);
            img.gameObject.SetActive(false);
            previewCells.Add(img);
        }
    }

    private bool IsInBounds(Vector2Int p)
    {
        int rowCount = Mathf.CeilToInt((float)cells.Length / columnCount);

        if (p.x < 0 || p.y < 0)
        {
            return false;
        }

        if (p.x >= columnCount || p.y >= rowCount)
        {
            return false;
        }

        int idx = p.y * columnCount + p.x;
        if (idx < 0 || idx >= cells.Length)
        {
            return false;
        }

        return true;
    }

    private SkillGridCell GetCell(Vector2Int p)
    {
        int idx = p.y * columnCount + p.x;
        if (idx < 0 || idx >= cells.Length)
        {
            return null;
        }

        return cells[idx];
    }

    private RectTransform GetCellRect(Vector2Int p)
    {
        SkillGridCell c = GetCell(p);
        if (c == null)
        {
            return null;
        }

        return c.transform as RectTransform;
    }
}
