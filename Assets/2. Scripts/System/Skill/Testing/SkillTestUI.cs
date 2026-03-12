using UnityEngine;
using UnityEngine.UI;

// [주혁] - DataManager 정적 클래스 전환에 의해 코드 수정(40, 58)

namespace SlayerLegend.Skill.Testing
{
    // 스킬 시스템 테스트용 UI
    public class SkillTestUI : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private DummyCharacter dummyCharacter;
        [SerializeField] private SkillController skillController;

        [Header("액티브 스킬 버튼")]
        [SerializeField] private Button addFireballBtn;
        [SerializeField] private Button addIceSpearBtn;

        [Header("패시브 스킬 버튼")]
        [SerializeField] private Button addAttackBoostBtn;
        [SerializeField] private Button addCritBoostBtn;

        [Header("테스트 버튼")]
        [SerializeField] private Button addGoldBtn;
        [SerializeField] private Button healBtn;

        private void Start()
        {
            // 버튼 이벤트 연결
            addFireballBtn.onClick.AddListener(() => AddActiveSkill("fireball", "파이어볼"));
            addIceSpearBtn.onClick.AddListener(() => AddActiveSkill("ice_spear", "얼음 창"));
            addAttackBoostBtn.onClick.AddListener(() => AddPassiveSkill("attack_boost", "공격력 강화"));
            addCritBoostBtn.onClick.AddListener(() => AddPassiveSkill("crit_boost", "치명타 강화"));
            addGoldBtn.onClick.AddListener(() => dummyCharacter.TestAddGold(1000));
            healBtn.onClick.AddListener(() => dummyCharacter.TestHeal());
        }

        private void AddActiveSkill(string id, string name)
        {
            var data = DataManager.skills.Get(id);
            if (data == null)
            {
                Debug.LogError($"스킬 데이터를 찾을 수 없습니다: {id}");
                return;
            }

            var skill = skillController.CreateActiveSkill(data);
            if (skill != null)
            {
                skill.transform.SetParent(dummyCharacter.transform);
                skillController.AddActiveSkill(skill);
            }
        }

        private void AddPassiveSkill(string id, string name)
        {
            var data = DataManager.skills.Get(id);
            if (data == null)
            {
                Debug.LogError($"스킬 데이터를 찾을 수 없습니다: {id}");
                return;
            }

            var skill = skillController.CreatePassiveSkill(data);
            if (skill != null)
            {
                skill.transform.SetParent(dummyCharacter.transform);
                skillController.AddPassiveSkill(skill);
            }
        }
    }
}
