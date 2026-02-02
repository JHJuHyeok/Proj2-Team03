using UnityEngine;

namespace SlayerLegend.Data
{
    // 스킬 타입
    public enum SkillType
    {
        Active,   // 쿨타임마다 자동 발동
        Passive   // 항상 활성화
    }

    // 개별 스킬 데이터 (ScriptableObject)
    [CreateAssetMenu(fileName = "NewSkill", menuName = "SlayerLegend/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("기본 정보")]
        public string skillId;
        public string skillName;
        public Sprite icon;
        [TextArea(3, 5)]
        public string description;
        public SkillType skillType;

        [Header("액티브 스킬 전용")]
        public float baseCooldown = 5f;
        public int baseManaCost = 10;
        public GameObject effectPrefab;

        [Header("데미지/성장")]
        public float baseDamage = 100f;
        public float damagePerLevel = 10f;
        public int maxLevel = 10;
        public int baseGoldCost = 100;
        public float goldCostMultiplier = 1.5f;

        [Header("패시브 스킬 전용")]
        public PassiveBuffType buffType;
        public float buffValue = 10f;

        // 레벨에 따른 데미지, 쿨타임, 레벨업 비용 계산
        public float GetDamage(int level)
            => baseDamage + (damagePerLevel * (level - 1));

        public int GetLevelUpCost(int currentLevel)
        {
            if (currentLevel >= maxLevel) return 0;
            return Mathf.RoundToInt(baseGoldCost * Mathf.Pow(goldCostMultiplier, currentLevel - 1));
        }

        public float GetCooldown(int level)
        {
            float reduction = Mathf.Min(0.5f, (level - 1) * 0.02f); // 최대 50% 감소
            return baseCooldown * (1f - reduction);
        }
    }

    // 패시브 버프 타입
    public enum PassiveBuffType
    {
        None,
        AttackDamagePercent,
        AttackDamageFixed,
        MaxHealthPercent,
        MaxHealthFixed,
        CriticalRate,
        CriticalDamage,
        GoldGainPercent
    }
}
