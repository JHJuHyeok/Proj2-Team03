using UnityEngine;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// 스킬 데이터 계산을 담당하는 정적 클래스
    /// 팀원의 JSON SkillData와 내 스킬 시스템을 연결
    /// </summary>
    public static class SkillCalculator
    {
        /// <summary>
        /// 레벨에 따른 데미지 계산
        /// baseDamage + (damagePerLevel * (level - 1))
        /// </summary>
        public static float GetDamage(SkillData data, int level)
        {
            // 팀원 데이터: initialRate를 기본값으로 사용
            float baseDamage = data.initialRate;
            float damagePerLevel = data.levelUpValue;
            return baseDamage + (damagePerLevel * (level - 1));
        }

        /// <summary>
        /// 레벨에 따른 쿨타임 계산
        /// 레벨당 2%씩 감소, 최대 50% 감소
        /// </summary>
        public static float GetCooldown(SkillData data, int level)
        {
            // 기본 쿨타임 5초 (JSON 데이터에 baseCooldown이 없으므로 기본값 사용)
            float baseCooldown = 5f;
            float reduction = Mathf.Min(0.5f, (level - 1) * 0.02f);
            return baseCooldown * (1f - reduction);
        }

        /// <summary>
        /// 레벨업 비용 계산
        /// baseGoldCost * (goldCostMultiplier ^ (currentLevel - 1))
        /// </summary>
        public static int GetLevelUpCost(SkillData data, int currentLevel)
        {
            if (currentLevel >= data.maxLevel) return 0;

            // 기본 비용 설정 (JSON 데이터에 없으므로 기본값 사용)
            int baseGoldCost = 100;
            float goldCostMultiplier = 1.5f;

            return Mathf.RoundToInt(baseGoldCost * Mathf.Pow(goldCostMultiplier, currentLevel - 1));
        }

        /// <summary>
        /// 패시브 스킬 버프 값 계산
        /// 레벨당 10%씩 증가
        /// </summary>
        public static float GetBuffValue(SkillData data, int level)
        {
            // 팀원 데이터에 buffValue가 없으므로 initialRate를 활용
            float baseValue = data.initialRate;
            float levelBonus = baseValue * 0.1f * (level - 1);
            return baseValue + levelBonus;
        }

        /// <summary>
        /// 마나 소모량 반환
        /// </summary>
        public static int GetManaCost(SkillData data)
        {
            return data.needMp;
        }

        /// <summary>
        /// 최대 레벨 반환
        /// </summary>
        public static int GetMaxLevel(SkillData data)
        {
            return data.maxLevel;
        }

        /// <summary>
        /// 액티브 스킬인지 확인
        /// </summary>
        public static bool IsActiveSkill(SkillData data)
        {
            return data.type == SkillType.Active;
        }

        /// <summary>
        /// 패시브 스킬인지 확인
        /// </summary>
        public static bool IsPassiveSkill(SkillData data)
        {
            return data.type == SkillType.Passive;
        }
    }
}
