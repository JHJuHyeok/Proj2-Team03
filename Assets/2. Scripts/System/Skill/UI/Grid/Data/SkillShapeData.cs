using UnityEngine;
using System.Collections.Generic;

namespace SlayerLegend.Skill.UI.Grid
{
    // 스킬 모양 타입 (테트리스 형태)
    public enum SkillShapeType
    {
        OneByOne,   // ■ (1칸) - 기본
        TwoByOne,   // ■■ (2칸 가로)
        OneByTwo,   // ■ (2칸 세로)
                    // ■
        TwoByTwo,   // ■■ (4칸 정사각)
                    // ■■
        L_Shape,    // ■■ (L자 3칸)
                    // ■
        T_Shape     // ■■■ (T자 4칸)
                    //  ■
    }

    // 스킬 모양 데이터
    // 각 스킬의 형태와 회전 정보를 관리
    [System.Serializable]
    public class SkillShapeData
    {
        public SkillShapeType shapeType;
        public Vector2Int baseSize;         // 기본 크기 (회전 전)
        public int cellCount;               // 차지하는 셀 수

        // 회전 정규화 (0, 90, 180, 270도만 허용)
        public static int NormalizeRotation(int rotation)
        {
            int steps = Mathf.RoundToInt(rotation / 90f) % 4;
            if (steps < 0) steps += 4;
            return steps * 90;
        }

        // 회전된 셀 위치 반환 (0, 90, 180, 270도)
        public List<Vector2Int> GetOccupiedCells(Vector2Int origin, int rotation)
        {
            List<Vector2Int> baseCells = GetBaseCells();
            List<Vector2Int> rotatedCells = new List<Vector2Int>();

            int rotationSteps = NormalizeRotation(rotation) / 90;

            // 1단계: 모든 셀 회전
            int minX = 0, minY = 0;
            foreach (var cell in baseCells)
            {
                Vector2Int rotated = RotateCellRaw(cell, rotationSteps);
                rotatedCells.Add(rotated);
                if (rotated.x < minX) minX = rotated.x;
                if (rotated.y < minY) minY = rotated.y;
            }

            // 2단계: 전체 보정 및 origin 적용
            for (int i = 0; i < rotatedCells.Count; i++)
            {
                rotatedCells[i] = new Vector2Int(
                    rotatedCells[i].x - minX + origin.x,
                    rotatedCells[i].y - minY + origin.y
                );
            }

            return rotatedCells;
        }

        // 기본 셀 위치 (원점 기준)
        private List<Vector2Int> GetBaseCells()
        {
            return shapeType switch
            {
                SkillShapeType.OneByOne => new List<Vector2Int> { Vector2Int.zero },

                SkillShapeType.TwoByOne => new List<Vector2Int>
                {
                    Vector2Int.zero,
                    new Vector2Int(1, 0)
                },

                SkillShapeType.OneByTwo => new List<Vector2Int>
                {
                    Vector2Int.zero,
                    new Vector2Int(0, 1)
                },

                SkillShapeType.TwoByTwo => new List<Vector2Int>
                {
                    Vector2Int.zero,
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1)
                },

                SkillShapeType.L_Shape => new List<Vector2Int>
                {
                    Vector2Int.zero,
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1)
                },

                SkillShapeType.T_Shape => new List<Vector2Int>
                {
                    Vector2Int.zero,
                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0),
                    new Vector2Int(1, 1)
                },

                _ => new List<Vector2Int> { Vector2Int.zero }
            };
        }

        // 순수 회전만 수행 (보정 없음)
        private Vector2Int RotateCellRaw(Vector2Int cell, int steps)
        {
            Vector2Int result = cell;
            for (int i = 0; i < steps; i++)
            {
                // 90도 시계방향 회전: (x, y) -> (y, -x)
                result = new Vector2Int(result.y, -result.x);
            }
            return result;
        }

        // 회전 후 크기 반환
        public Vector2Int GetRotatedSize(int rotation)
        {
            int steps = NormalizeRotation(rotation) / 90;

            // 90도 또는 270도 회전 시 가로/세로 교환
            if (steps == 1 || steps == 3)
            {
                return new Vector2Int(baseSize.y, baseSize.x);
            }

            return baseSize;
        }

        // 팩토리 메서드: 모양 타입으로부터 데이터 생성
        public static SkillShapeData Create(SkillShapeType type)
        {
            return type switch
            {
                SkillShapeType.OneByOne => new SkillShapeData
                {
                    shapeType = type,
                    baseSize = new Vector2Int(1, 1),
                    cellCount = 1
                },

                SkillShapeType.TwoByOne => new SkillShapeData
                {
                    shapeType = type,
                    baseSize = new Vector2Int(2, 1),
                    cellCount = 2
                },

                SkillShapeType.OneByTwo => new SkillShapeData
                {
                    shapeType = type,
                    baseSize = new Vector2Int(1, 2),
                    cellCount = 2
                },

                SkillShapeType.TwoByTwo => new SkillShapeData
                {
                    shapeType = type,
                    baseSize = new Vector2Int(2, 2),
                    cellCount = 4
                },

                SkillShapeType.L_Shape => new SkillShapeData
                {
                    shapeType = type,
                    baseSize = new Vector2Int(2, 2),
                    cellCount = 3
                },

                SkillShapeType.T_Shape => new SkillShapeData
                {
                    shapeType = type,
                    baseSize = new Vector2Int(3, 2),
                    cellCount = 4
                },

                _ => new SkillShapeData
                {
                    shapeType = SkillShapeType.OneByOne,
                    baseSize = new Vector2Int(1, 1),
                    cellCount = 1
                }
            };
        }
    }
}
