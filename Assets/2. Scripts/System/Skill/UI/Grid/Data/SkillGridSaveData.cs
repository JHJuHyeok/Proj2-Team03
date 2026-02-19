using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Skill.UI.Grid
{
    // 그리드에 배치된 스킬 정보
    [Serializable]
    public class PlacedSkillData
    {
        public string skillId;              // 스킬 ID
        public int gridX;                   // 그리드 X 좌표
        public int gridY;                   // 그리드 Y 좌표
        public int rotation;                // 회전 (0, 90, 180, 270)
        public SkillShapeType shapeType;    // 스킬 모양 타입

        // 편의 프로퍼티
        public Vector2Int GridPosition => new Vector2Int(gridX, gridY);

        // 생성자
        public PlacedSkillData() { }

        public PlacedSkillData(string id, Vector2Int pos, int rot, SkillShapeType shape = SkillShapeType.OneByOne)
        {
            skillId = id;
            gridX = pos.x;
            gridY = pos.y;
            rotation = SkillShapeData.NormalizeRotation(rot);
            shapeType = shape;
        }

        public PlacedSkillData(string id, int x, int y, int rot, SkillShapeType shape = SkillShapeType.OneByOne)
        {
            skillId = id;
            gridX = x;
            gridY = y;
            rotation = SkillShapeData.NormalizeRotation(rot);
            shapeType = shape;
        }

        // 스킬이 차지하는 모든 셀 위치 반환
        public List<Vector2Int> GetOccupiedCells()
        {
            var shapeData = SkillShapeData.Create(shapeType);
            return shapeData.GetOccupiedCells(GridPosition, rotation);
        }

        // 특정 셀을 점유하고 있는지 확인
        public bool OccupiesCell(Vector2Int cellPos)
        {
            var occupiedCells = GetOccupiedCells();
            return occupiedCells.Contains(cellPos);
        }
    }

    // 그리드 저장 데이터 (JSON 직렬화용)
    [Serializable]
    public class SkillGridSaveData
    {
        public int gridWidth = 6;           // 그리드 가로 크기
        public int gridHeight = 6;          // 그리드 세로 크기
        public List<PlacedSkillData> placedSkills = new List<PlacedSkillData>();

        // 그리드 크기
        public Vector2Int GridSize => new Vector2Int(gridWidth, gridHeight);

        // 기본 생성자 (6x6 그리드)
        public SkillGridSaveData()
        {
            gridWidth = 6;
            gridHeight = 6;
        }

        // 커스텀 크기 생성자
        public SkillGridSaveData(int width, int height)
        {
            gridWidth = width;
            gridHeight = height;
        }

        // 스킬 배치 추가
        public void AddPlacedSkill(PlacedSkillData skill)
        {
            // 동일 ID가 있으면 제거 후 추가 (갱신)
            placedSkills.RemoveAll(s => s.skillId == skill.skillId);
            placedSkills.Add(skill);
        }

        // 스킬 배치 제거
        public void RemovePlacedSkill(string skillId)
        {
            placedSkills.RemoveAll(s => s.skillId == skillId);
        }

        // 스킬 배치 조회
        public PlacedSkillData GetPlacedSkill(string skillId)
        {
            return placedSkills.Find(s => s.skillId == skillId);
        }

        // 특정 위치의 스킬 조회 (origin 위치만 확인)
        public PlacedSkillData GetSkillWithOriginAtPosition(Vector2Int pos)
        {
            return placedSkills.Find(s => s.GridPosition == pos);
        }

        // 특정 셀을 점유하는 스킬 조회 (실제 차지하는 모든 셀 확인)
        public PlacedSkillData GetSkillOccupyingCell(Vector2Int cellPos)
        {
            return placedSkills.Find(s => s.OccupiesCell(cellPos));
        }

        // 배치된 스킬 ID 목록
        public List<string> GetPlacedSkillIds()
        {
            List<string> ids = new List<string>();
            foreach (var skill in placedSkills)
            {
                ids.Add(skill.skillId);
            }
            return ids;
        }

        // 전체 초기화
        public void Clear()
        {
            placedSkills.Clear();
        }

        // JSON으로 변환
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        // JSON에서 로드
        public static SkillGridSaveData FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<SkillGridSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillGridSaveData] JSON 파싱 실패: {e.Message}");
                return new SkillGridSaveData();
            }
        }

        // 복사본 생성
        public SkillGridSaveData Clone()
        {
            string json = ToJson();
            return FromJson(json);
        }
    }

    // 그리드 확장 정보 (진행에 따른 그리드 확장용)
    [Serializable]
    public class GridExpansionData
    {
        public int currentWidth = 6;
        public int currentHeight = 6;
        public int maxWidth = 10;
        public int maxHeight = 10;

        // 확장 가능 여부
        public bool CanExpandWidth => currentWidth < maxWidth;
        public bool CanExpandHeight => currentHeight < maxHeight;

        // 가로 확장
        public bool ExpandWidth()
        {
            if (!CanExpandWidth) return false;
            currentWidth++;
            return true;
        }

        // 세로 확장
        public bool ExpandHeight()
        {
            if (!CanExpandHeight) return false;
            currentHeight++;
            return true;
        }
    }
}
