using UnityEngine;

namespace SlayerLegend.Skill
{
    // 모든 스킬의 기반 클래스 (abstract)
    // - 스킬 데이터, 레벨 관리
    // - LevelUp(), Initialize()
    public abstract class SkillBase : MonoBehaviour
    {
        [Header("스킬 데이터")]
        [SerializeField] protected SkillData skillData;

        [Header("현재 상태")]
        [SerializeField] protected int currentLevel = 1;

        public int CurrentLevel
        {
            get => currentLevel;
            set => currentLevel = Mathf.Clamp(value, 1, SkillCalculator.GetMaxLevel(skillData));
        }

        public SkillData Data => skillData;
        public bool IsMaxLevel => currentLevel >= SkillCalculator.GetMaxLevel(skillData);

        // 레벨업
        public virtual bool LevelUp()
        {
            if (IsMaxLevel)
            {
                Debug.Log($"이미 최대 레벨입니다: {skillData.name}");
                return false;
            }

            currentLevel++;
            OnLevelUp();
            return true;
        }

        // 스킬 초기화
        public virtual void Initialize(SkillData data)
        {
            skillData = data;
            currentLevel = 1;
        }

        protected virtual void OnLevelUp()
        {
            Debug.Log($"{skillData.name} 레벨업! 현재 레벨: {currentLevel}");
        }

        public override string ToString()
            => $"{skillData.name} (Lv.{currentLevel}/{SkillCalculator.GetMaxLevel(skillData)})";
    }
}
