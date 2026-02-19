using UnityEngine;
using UnityEngine.UI;

namespace SlayerLegend.Skill.UI.Grid
{
    // 개별 그리드 셀
    // 6x6 그리드의 각 셀을 나타내며, 점유 상태와 시각적 표시 관리
    public class SkillGridCell : MonoBehaviour
    {
        [Header("셀 설정")]
        [SerializeField] private int gridX;
        [SerializeField] private int gridY;

        [Header("시각 요소")]
        [SerializeField] private Image background;
        [SerializeField] private Image highlightOverlay;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color darkColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        [SerializeField] private Color hoverColor = new Color(0.3f, 0.5f, 0.3f, 0.8f);
        [SerializeField] private Color validColor = new Color(0.2f, 0.8f, 0.2f, 0.6f);
        [SerializeField] private Color invalidColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);

        // 상태
        private bool isOccupied = false;
        private PlacedSkillData occupyingSkill = null;

        // 프로퍼티
        public Vector2Int GridPosition => new Vector2Int(gridX, gridY);
        public bool IsOccupied => isOccupied;
        public PlacedSkillData OccupyingSkill => occupyingSkill;

        // 초기화
        public void Initialize(int x, int y)
        {
            gridX = x;
            gridY = y;
            name = $"Cell_{x}_{y}";
            SetHighlight(CellHighlight.None);
        }

        // 점유 상태 설정
        public void SetOccupied(PlacedSkillData skill)
        {
            isOccupied = true;
            occupyingSkill = skill;
        }

        // 점유 해제
        public void ClearOccupied()
        {
            isOccupied = false;
            occupyingSkill = null;
        }

        // 하이라이트 상태 설정
        public void SetHighlight(CellHighlight highlight)
        {
            if (highlightOverlay == null) return;

            switch (highlight)
            {
                case CellHighlight.None:
                    highlightOverlay.gameObject.SetActive(false);
                    break;

                case CellHighlight.Hover:
                    highlightOverlay.gameObject.SetActive(true);
                    highlightOverlay.color = hoverColor;
                    break;

                case CellHighlight.Valid:
                    highlightOverlay.gameObject.SetActive(true);
                    highlightOverlay.color = validColor;
                    break;

                case CellHighlight.Invalid:
                    highlightOverlay.gameObject.SetActive(true);
                    highlightOverlay.color = invalidColor;
                    break;
            }
        }

        // 배경색 설정
        public void SetBackgroundColor(Color color)
        {
            if (background != null)
            {
                background.color = color;
            }
        }

        // 체스판 패턴 적용
        public void ApplyCheckerPattern()
        {
            bool isLight = (gridX + gridY) % 2 == 0;
            SetBackgroundColor(isLight ? normalColor : darkColor);
        }
    }

    // 셀 하이라이트 타입
    public enum CellHighlight
    {
        None,       // 하이라이트 없음
        Hover,      // 마우스 오버
        Valid,      // 배치 가능 (초록)
        Invalid     // 배치 불가 (빨강)
    }
}
