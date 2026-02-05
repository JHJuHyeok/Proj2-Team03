using UnityEngine;
using UnityEngine.UI;

/*
SkillGridBlockOverlay
- GridLayoutGroup으로 배치된 "칸" 위에 블럭(빨간 박스)을 겹쳐서 표시
- 블럭은 "시작 칸(col,row)" + "덮을 칸 수(width,height)"로 배치
- GridLayoutGroup의 padding/spacing/cellSize를 읽어서 위치/크기를 자동 계산
- Overlay는 LayoutGroup 영향이 없도록 별도 레이어로 분리해서 사용
*/

public class SkillGridBlockOverlay : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private GridLayoutGroup grid;         // Skill Grid Board의 GridLayoutGroup
    [SerializeField] private RectTransform gridRect;       // Skill Grid Board의 RectTransform

    [Header("Overlay")]
    [SerializeField] private RectTransform overlayRoot;    // Overlay(Stretch로 gridRect 위에 덮기)
    [SerializeField] private RectTransform blockPrefab;    // 빨간 박스 프리팹(Image)

    [Header("Grid Info")]
    [SerializeField] private int columnCount = 6;          // 6x6 고정이면 6

    private void Awake()
    {
        if (gridRect == null)
        {
            gridRect = GetComponentInParent<RectTransform>();
        }

        if (overlayRoot == null)
        {
            overlayRoot = GetComponent<RectTransform>();
        }
    }

    // 예시: (0,0)에서 시작해서 가로 2칸, 세로 1칸 덮는 블럭
    [ContextMenu("Test Spawn")]
    private void TestSpawn()
    {
        ClearBlocks();
        SpawnBlock(new Vector2Int(0, 0), new Vector2Int(2, 1));
        SpawnBlock(new Vector2Int(2, 1), new Vector2Int(1, 3));
        SpawnBlock(new Vector2Int(4, 4), new Vector2Int(2, 2));
    }

    // 블럭 전체 삭제
    public void ClearBlocks()
    {
        if (overlayRoot == null)
        {
            return;
        }

        for (int i = overlayRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(overlayRoot.GetChild(i).gameObject);
        }
    }

    /*
    startCell: (col,row) 0부터 시작, (0,0)=좌상단
    span: (width,height) 덮을 칸 수
    */
    public RectTransform SpawnBlock(Vector2Int startCell, Vector2Int span)
    {
        if (grid == null || gridRect == null || overlayRoot == null || blockPrefab == null)
        {
            Debug.LogError("[SkillGridBlockOverlay] Reference missing");
            return null;
        }

        if (columnCount <= 0)
        {
            Debug.LogError("[SkillGridBlockOverlay] columnCount invalid");
            return null;
        }

        if (span.x <= 0 || span.y <= 0)
        {
            Debug.LogError("[SkillGridBlockOverlay] span invalid");
            return null;
        }

        // grid 기준 값들
        Vector2 cell = grid.cellSize;
        Vector2 spacing = grid.spacing;
        RectOffset pad = grid.padding;

        // (0,0)=좌상단 기준으로 "칸의 좌상단" 위치를 계산
        // overlayRoot는 gridRect와 동일 좌표계(Stretch)라고 가정
        float x0 = pad.left + startCell.x * (cell.x + spacing.x);
        float y0 = pad.top + startCell.y * (cell.y + spacing.y);

        // 블럭 크기(덮는 칸 수만큼)
        float w = span.x * cell.x + (span.x - 1) * spacing.x;
        float h = span.y * cell.y + (span.y - 1) * spacing.y;

        // RectTransform 좌표는 pivot/anchor에 따라 달라서,
        // 여기서는 overlayRoot를 Stretch(0~1) + pivot(0,1)로 두면 계산이 가장 깔끔함.
        // (Overlay 세팅 팁: Pivot을 (0,1)로 맞추면 아래 anchoredPosition 계산이 직관적)
        RectTransform block = Instantiate(blockPrefab, overlayRoot);
        block.name = $"Block_{startCell.x}_{startCell.y}_{span.x}x{span.y}";

        // overlayRoot Pivot이 (0,1)일 때:
        // anchoredPosition은 좌상단 기준으로 (x, -y)
        block.anchorMin = new Vector2(0f, 1f);
        block.anchorMax = new Vector2(0f, 1f);
        block.pivot = new Vector2(0f, 1f);

        block.anchoredPosition = new Vector2(x0, -y0);
        block.sizeDelta = new Vector2(w, h);

        return block;
    }
}
