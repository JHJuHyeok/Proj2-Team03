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

            // [조민희] 저장된 스킬 레벨 불러오기
            currentLevel = GetSavedLevel(data.id);
        }

        /// <summary>
        /// [조민희] 저장 데이터에서 스킬 레벨 조회
        /// </summary>
        protected int GetSavedLevel(string skillId)
        {
            if (DataManager.CurrentSaveData?.skillInfo == null)
            {
                return 1; // 기본 레벨
            }

            if (DataManager.CurrentSaveData.skillInfo.TryGetValue(skillId, out var info))
            {
                return info.level > 0 ? info.level : 1;
            }

            return 1; // 기본 레벨
        }

        protected virtual void OnLevelUp()
        {
            Debug.Log($"{skillData.name} 레벨업! 현재 레벨: {currentLevel}");
        }

        /// <summary>
        /// [조민희] 저장 데이터에서 레벨을 다시 불러와 갱신
        /// 외부(UI)에서 스킬 레벨을 변경했을 때 호출
        /// </summary>
        public void RefreshLevel()
        {
            if (skillData == null) return;

            int savedLevel = GetSavedLevel(skillData.id);
            if (savedLevel != currentLevel)
            {
                int oldLevel = currentLevel;
                currentLevel = savedLevel;
                OnLevelUp(); // 레벨 변경 알림
                Debug.Log($"[SkillBase] {skillData.name} 레벨 갱신: {oldLevel} → {currentLevel}");
            }
        }

        public override string ToString()
            => $"{skillData.name} (Lv.{currentLevel}/{SkillCalculator.GetMaxLevel(skillData)})";
    }
}
