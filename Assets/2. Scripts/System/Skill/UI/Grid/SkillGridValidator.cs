using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Skill.UI.Grid
{
    // 스킬 그리드 배치 유효성 검사 결과
    public struct ValidationResult
    {
        public bool isValid;
        public string failReason;
        public List<Vector2Int> validCells;
        public List<Vector2Int> invalidCells;

        public static ValidationResult Success(List<Vector2Int> validCells)
        {
            return new ValidationResult
            {
                isValid = true,
                failReason = string.Empty,
                validCells = validCells,
                invalidCells = new List<Vector2Int>()
            };
        }

        public static ValidationResult Fail(string reason, List<Vector2Int> invalidCells = null)
        {
            return new ValidationResult
            {
                isValid = false,
                failReason = reason,
                validCells = new List<Vector2Int>(),
                invalidCells = invalidCells ?? new List<Vector2Int>()
            };
        }
    }

    // 스킬 그리드 배치 유효성 검사기
    // 배치 가능 여부, 충돌 감지, 범위 체크 등을 담당
    public static class SkillGridValidator
    {
        // 배치 유효성 검사
        public static ValidationResult ValidatePlacement(
            SkillGridManager gridManager,
            string skillId,
            Vector2Int position,
            int rotation,
            SkillShapeType shapeType)
        {
            if (gridManager == null)
            {
                return ValidationResult.Fail("그리드 매니저가 없습니다.");
            }

            var shapeData = SkillShapeData.Create(shapeType);
            var occupiedCells = shapeData.GetOccupiedCells(position, rotation);

            // 1. 그리드 범위 체크
            var outOfBoundsCells = new List<Vector2Int>();
            foreach (var cell in occupiedCells)
            {
                if (!gridManager.IsWithinGrid(cell))
                {
                    outOfBoundsCells.Add(cell);
                }
            }

            if (outOfBoundsCells.Count > 0)
            {
                return new ValidationResult
                {
                    isValid = false,
                    failReason = "그리드 범위를 벗어납니다.",
                    validCells = new List<Vector2Int>(),
                    invalidCells = outOfBoundsCells
                };
            }

            // 2. 기존 스킬 중복 체크 (같은 ID가 이미 그리드에 있는지 전역 체크)
            var allSkills = gridManager.GetAllPlacedSkills();
            var existingSkillWithSameId = allSkills?.Find(s => s.skillId == skillId);
            if (existingSkillWithSameId != null)
            {
                return ValidationResult.Fail("이미 배치된 스킬입니다.", occupiedCells);
            }

            // 3. 다른 스킬과 충돌 체크
            var collisionCells = new List<Vector2Int>();
            foreach (var cell in occupiedCells)
            {
                var occupyingSkill = gridManager.GetSkillAtCell(cell);
                if (occupyingSkill != null && occupyingSkill.skillId != skillId)
                {
                    collisionCells.Add(cell);
                }
            }

            if (collisionCells.Count > 0)
            {
                return new ValidationResult
                {
                    isValid = false,
                    failReason = $"다른 스킬과 충돌합니다.",
                    validCells = new List<Vector2Int>(),
                    invalidCells = collisionCells
                };
            }

            // 모든 검사 통과
            return ValidationResult.Success(occupiedCells);
        }

        // 드래그 중 미리보기용 유효성 검사 (하이라이트 표시용)
        public static ValidationResult ValidateForPreview(
            SkillGridManager gridManager,
            Vector2Int position,
            int rotation,
            SkillShapeType shapeType,
            string excludeSkillId = null)
        {
            if (gridManager == null)
            {
                return ValidationResult.Fail("그리드 매니저가 없습니다.");
            }

            var shapeData = SkillShapeData.Create(shapeType);
            var occupiedCells = shapeData.GetOccupiedCells(position, rotation);

            var validCells = new List<Vector2Int>();
            var invalidCells = new List<Vector2Int>();
            bool hasAnyInvalid = false;

            foreach (var cell in occupiedCells)
            {
                // 그리드 범위 체크
                if (!gridManager.IsWithinGrid(cell))
                {
                    invalidCells.Add(cell);
                    hasAnyInvalid = true;
                    continue;
                }

                // 다른 스킬과 충돌 체크 (제외할 스킬 ID는 무시)
                var occupyingSkill = gridManager.GetSkillAtCell(cell);
                if (occupyingSkill != null && occupyingSkill.skillId != excludeSkillId)
                {
                    invalidCells.Add(cell);
                    hasAnyInvalid = true;
                    continue;
                }

                validCells.Add(cell);
            }

            return new ValidationResult
            {
                isValid = !hasAnyInvalid,
                failReason = hasAnyInvalid ? "배치할 수 없는 위치가 포함되어 있습니다." : string.Empty,
                validCells = validCells,
                invalidCells = invalidCells
            };
        }

        // 회전 가능 여부 검사
        public static bool CanRotate(
            SkillGridManager gridManager,
            string skillId,
            int newRotation)
        {
            var allSkills = gridManager.GetAllPlacedSkills();
            var skill = allSkills?.Find(s => s.skillId == skillId);

            if (skill == null) return false;

            var result = ValidateForPreview(
                gridManager,
                skill.GridPosition,
                newRotation,
                skill.shapeType,
                skillId
            );

            return result.isValid;
        }

        // 이동 가능 여부 검사
        public static bool CanMove(
            SkillGridManager gridManager,
            string skillId,
            Vector2Int newPosition,
            int newRotation)
        {
            var allSkills = gridManager.GetAllPlacedSkills();
            var skill = allSkills?.Find(s => s.skillId == skillId);

            if (skill == null) return false;

            var result = ValidateForPreview(
                gridManager,
                newPosition,
                newRotation,
                skill.shapeType,
                skillId
            );

            return result.isValid;
        }

        // 빈 셀 찾기 (자동 배치용)
        public static List<Vector2Int> FindEmptyCells(SkillGridManager gridManager)
        {
            var emptyCells = new List<Vector2Int>();

            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                for (int x = 0; x < gridManager.GridWidth; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (gridManager.GetSkillAtCell(pos) == null)
                    {
                        emptyCells.Add(pos);
                    }
                }
            }

            return emptyCells;
        }

        // 특정 모양을 배치할 수 있는 첫 번째 위치 찾기
        public static bool FindFirstValidPosition(
            SkillGridManager gridManager,
            SkillShapeType shapeType,
            out Vector2Int position,
            int rotation = 0)
        {
            var shapeData = SkillShapeData.Create(shapeType);

            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                for (int x = 0; x < gridManager.GridWidth; x++)
                {
                    var testPos = new Vector2Int(x, y);
                    var result = ValidateForPreview(gridManager, testPos, rotation, shapeType);

                    if (result.isValid)
                    {
                        position = testPos;
                        return true;
                    }
                }
            }

            position = Vector2Int.zero;
            return false;
        }

        // 그리드 사용률 계산 (0.0 ~ 1.0)
        public static float CalculateGridUsage(SkillGridManager gridManager)
        {
            int totalCells = gridManager.GridWidth * gridManager.GridHeight;
            int usedCells = 0;

            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                for (int x = 0; x < gridManager.GridWidth; x++)
                {
                    if (gridManager.GetSkillAtCell(new Vector2Int(x, y)) != null)
                    {
                        usedCells++;
                    }
                }
            }

            return (float)usedCells / totalCells;
        }
    }
}
