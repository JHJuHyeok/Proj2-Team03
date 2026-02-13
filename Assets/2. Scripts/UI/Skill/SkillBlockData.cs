using System.Collections.Generic;
using UnityEngine;

/*
SkillBlockData
- 테트리스 블록처럼 여러 칸을 차지하는 모양 데이터
- cells: 기준점(0,0) 대비 상대좌표 리스트
- 회전 시 좌표가 음수로 내려갈 수 있으므로 Normalize로 0 이상으로 정리
*/

[System.Serializable]
public class SkillBlockData
{
    public List<Vector2Int> cells = new List<Vector2Int>();

    public SkillBlockData Clone()
    {
        SkillBlockData d = new SkillBlockData();
        d.cells = new List<Vector2Int>(cells);
        return d;
    }

    // 시계방향 90도: (x,y) -> (y,-x)
    public void RotateCW()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int p = cells[i];
            cells[i] = new Vector2Int(p.y, -p.x);
        }

        NormalizeToZero();
    }

    // 반시계 90도: (x,y) -> (-y,x)
    public void RotateCCW()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int p = cells[i];
            cells[i] = new Vector2Int(-p.y, p.x);
        }

        NormalizeToZero();
    }

    // 최소 x,y가 0이 되도록 평행이동
    private void NormalizeToZero()
    {
        if (cells == null || cells.Count == 0)
        {
            return;
        }

        int minX = int.MaxValue;
        int minY = int.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int p = cells[i];
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
        }

        if (minX == 0 && minY == 0)
        {
            return;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int p = cells[i];
            cells[i] = new Vector2Int(p.x - minX, p.y - minY);
        }
    }
}
