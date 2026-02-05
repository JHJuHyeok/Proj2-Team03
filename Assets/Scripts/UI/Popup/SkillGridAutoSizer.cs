using UnityEngine;
using UnityEngine.UI;

/*
SkillGridAutoSizer
- 36칸(6x6) 정사각형 스킬 보드용 Grid Layout 자동 사이징
- 부모 RectTransform 크기에 맞춰 Cell Size를 자동 계산해서 "정사각" 유지
- Constraint: Fixed Column Count(6) 세팅을 전제로 동작
*/

[RequireComponent(typeof(GridLayoutGroup))]
public class SkillGridAutoSizer : MonoBehaviour
{
    [SerializeField] private int columnCount = 6;     // 6열 고정(36칸이면 6x6)
    [SerializeField] private float paddingLeft = 30f;
    [SerializeField] private float paddingRight = 30f;
    [SerializeField] private float paddingTop = 30f;
    [SerializeField] private float paddingBottom = 30f;
    [SerializeField] private float spacingX = 20f;
    [SerializeField] private float spacingY = 20f;

    private GridLayoutGroup grid;
    private RectTransform rt;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rt = GetComponent<RectTransform>();

        ApplyStaticSettings();
        RebuildCellSize();
    }

    private void OnRectTransformDimensionsChange()
    {
        // 해상도/레이아웃 변동 시 자동 재계산
        RebuildCellSize();
    }

    private void ApplyStaticSettings()
    {
        if (grid == null) return;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columnCount;

        grid.padding.left = Mathf.RoundToInt(paddingLeft);
        grid.padding.right = Mathf.RoundToInt(paddingRight);
        grid.padding.top = Mathf.RoundToInt(paddingTop);
        grid.padding.bottom = Mathf.RoundToInt(paddingBottom);

        grid.spacing = new Vector2(spacingX, spacingY);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
    }

    private void RebuildCellSize()
    {
        if (grid == null || rt == null) return;

        if (columnCount <= 0) return;

        // 보드(부모) 내부에서 실제로 사용할 수 있는 가로/세로 영역 계산
        float width = rt.rect.width - grid.padding.left - grid.padding.right;
        float height = rt.rect.height - grid.padding.top - grid.padding.bottom;

        // 6x6이므로 열/행 모두 columnCount 사용
        int rowCount = columnCount;

        // 스페이싱 총합을 제외한 남은 영역을 N등분
        float cellW = (width - grid.spacing.x * (columnCount - 1)) / columnCount;
        float cellH = (height - grid.spacing.y * (rowCount - 1)) / rowCount;

        // 정사각형 유지: 더 작은 쪽을 기준으로 맞춤
        float cell = Mathf.Floor(Mathf.Min(cellW, cellH));

        if (cell < 1f)
        {
            cell = 1f;
        }

        grid.cellSize = new Vector2(cell, cell);
    }
}
