using System;
using System.Collections.Generic;
using SlayerLegend.Skill.UI.Grid;

namespace SlayerLegend.Skill.Data
{
    // SkillData의 그리드 관련 확장 메서드
    // 팀원 파일(SkillData.cs)을 직접 수정하지 않고 그리드 시스템과 연동
    public static class SkillDataGridExtensions
    {
        // 스킬 ID에 따른 모양 반환 (기본 규칙 기반)
        // 실제로는 JSON에서 shapeType을 읽어야 하지만,
        // SkillData.cs를 수정하지 않기 위해 규칙 기반으로 구현
        public static SkillShapeType GetShapeType(this SkillData skillData)
        {
            // null 체크
            if (skillData == null)
            {
                return SkillShapeType.OneByOne;
            }

            // 스킬 ID 또는 등급에 따른 모양 결정 규칙
            return skillData.id switch
            {
                // 특수 스킬들
                "meteor" => SkillShapeType.TwoByTwo,       // 메테오: 2x2
                "ice_spear" => SkillShapeType.OneByTwo,    // 얼음 창: 1x2 (세로)
                "burn" => SkillShapeType.TwoByOne,         // 화상: 2x1 (가로)
                "freeze_blast" => SkillShapeType.L_Shape,  // 빙결 폭발: L자

                // 기본 스킬들
                _ => GetShapeByGrade(skillData.grade)
            };
        }

        // 등급에 따른 기본 모양
        private static SkillShapeType GetShapeByGrade(SkillGrade grade)
        {
            return grade switch
            {
                SkillGrade.Common => SkillShapeType.OneByOne,      // 일반: 1x1
                SkillGrade.Uncommon => SkillShapeType.OneByOne,    // 고급: 1x1
                SkillGrade.Rare => SkillShapeType.TwoByOne,        // 희귀: 2x1
                SkillGrade.Hero => SkillShapeType.OneByTwo,        // 영웅: 1x2
                SkillGrade.Legend => SkillShapeType.L_Shape,       // 전설: L자
                SkillGrade.Myth => SkillShapeType.T_Shape,         // 신화: T자
                _ => SkillShapeType.OneByOne
            };
        }

        // 스킬이 그리드에 배치 가능한지 확인
        public static bool CanPlaceOnGrid(this SkillData skillData)
        {
            // 모든 스킬은 기본적으로 그리드에 배치 가능
            // 필요시 조건 추가 (예: 특정 타입만 배치 가능 등)
            return true;
        }

        // 스킬의 그리드 크기 반환
        public static string GetGridSizeDescription(this SkillData skillData)
        {
            if (skillData == null) return "알 수 없음";

            var shapeType = skillData.GetShapeType();

            return shapeType switch
            {
                SkillShapeType.OneByOne => "1칸",
                SkillShapeType.TwoByOne => "2칸 (가로)",
                SkillShapeType.OneByTwo => "2칸 (세로)",
                SkillShapeType.TwoByTwo => "4칸 (정사각)",
                SkillShapeType.L_Shape => "3칸 (L자)",
                SkillShapeType.T_Shape => "4칸 (T자)",
                _ => $"{SkillShapeData.Create(shapeType).cellCount}칸"
            };
        }
    }

    // 그리드용 스킬 메타데이터
    // SkillData와 그리드 시스템 간의 브릿지
    [Serializable]
    public class SkillGridMetaData
    {
        public string skillId;
        public SkillShapeType shapeType;
        public bool isUnlocked;
        public bool isInInventory;

        public SkillGridMetaData()
        {
            shapeType = SkillShapeType.OneByOne;
            isUnlocked = true;
            isInInventory = false;
        }

        public SkillGridMetaData(string id, SkillShapeType shape)
        {
            skillId = id;
            shapeType = shape;
            isUnlocked = true;
            isInInventory = false;
        }

        // SkillData로부터 생성
        public static SkillGridMetaData FromSkillData(SkillData skillData)
        {
            if (skillData == null)
            {
                return new SkillGridMetaData();
            }

            return new SkillGridMetaData
            {
                skillId = skillData.id,
                shapeType = skillData.GetShapeType(),
                isUnlocked = true,
                isInInventory = false
            };
        }
    }

    // 그리드 메타데이터 리스트 (JSON 직렬화용)
    [Serializable]
    public class SkillGridMetaDataList
    {
        public List<SkillGridMetaData> metaList = new List<SkillGridMetaData>();
    }
}
