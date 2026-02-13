using UnityEngine;
using SlayerLegend.Skill;
using System.Linq;

// [주혁] - DataManager 정적 클래스 전환으로 인한 코드 수정(134, 143, 231, 278)

public class SkillTestInitializer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SkillController skillController;

    [Header("발사체 프리팹")]
    [SerializeField] private SkillProjectile2D fireballProjectile;
    [SerializeField] private IceSpearProjectile iceSpearProjectile;  // 조민희 추가: 관통형 얼음 창
    [SerializeField] private SkillProjectile2D meteorProjectile;
    [SerializeField] private DoTProjectile poisonProjectile;  // 조민희 추가: DoT형 독 발사체
    // 참고: 블래스트 번은 폭발 스킬이므로 발사체 프리팹이 필요 없음

    [Header("폭발 이펙트 프리팹")]
    [SerializeField] private GameObject blastBurnEffect;  // 블래스트 번 폭발 이펙트

    // 조민희 추가: 스킬 데이터 캐싱 (재생성용)
    private struct SkillConfig
    {
        public string skillId;
        public SkillProjectile2D projectile;
        public Vector3 direction;
        public string name;
        public Vector3 offset;
        public bool hasRandomX;
        public Vector2 randomXRange;
        public GameObject explosionEffect;  // 폭발 이펙트 (폭발 스킬용)
    }

    private SkillConfig[] skillConfigs;

    // 조민희 추가: 패시브 스킬 설정
    private struct PassiveSkillConfig
    {
        public string skillId;
        public string name;
    }

    private PassiveSkillConfig[] passiveSkillConfigs;

    // 조민희 추가: 스킬 초기화 헬퍼 메서드
    private void InitializeSkill(SkillData skillData, SkillProjectile2D projectile, Vector3 direction, string skillName, Vector3 offset = default, object randomX = null, GameObject explosionEffect = null)
    {
        if (skillData != null)
        {
            var skill = skillController.CreateActiveSkill(skillData);
            skill.transform.SetParent(transform);

            // 발사체 설정 (폭발 스킬이 아닌 경우만)
            if (projectile != null)
            {
                bool isBlast = skillData.effectData?.isBlastSkill == true;
                if (!isBlast)
                {
                    skill.SetProjectilePrefab(projectile);
                }
            }

            // 폭발 이펙트 설정 (폭발 스킬인 경우)
            if (skillData.effectData?.isBlastSkill == true && explosionEffect != null)
            {
                skill.SetExplosionEffectPrefab(explosionEffect);
            }

            skill.SetFireDirection(direction);

            // 발사 위치 설정
            skill.SetSpawnOffset(offset);

            // 랜덤 X범위가 있으면 설정 (메테오만 사용)
            if (randomX != null)
            {
                // Vector2를 float로 변환 필요
                Vector2 rx = (Vector2)randomX;
                skill.SetRandomXRange(rx.x, rx.y);
            }

            skill.SetTestCooldown(0.5f);
            skill.SetTestNoManaCost(true);
            skillController.AddActiveSkill(skill);
            skill.SetActive(true);
            Debug.Log($"[Test] {skillName} 스킬 추가 완료 (폭발: {(skillData.effectData?.isBlastSkill == true ? "예" : "아니오")})");
        }
    }

    private void Start()
    {
        // 조민희 추가: 스킬 설정 저장
        skillConfigs = new SkillConfig[]
        {
            new SkillConfig { skillId = "fireball", projectile = fireballProjectile, direction = Vector3.right, name = "파이어볼" },
            new SkillConfig { skillId = "ice_spear", projectile = iceSpearProjectile, direction = Vector3.right, name = "얼음 창" },
            new SkillConfig { skillId = "meteor", projectile = meteorProjectile, direction = Vector3.down, name = "메테오", offset = new Vector3(0, 10, 0), hasRandomX = true, randomXRange = new Vector2(1f, 3f) },
            new SkillConfig { skillId = "poison", projectile = poisonProjectile, direction = Vector3.right, name = "포이즌볼" },
            new SkillConfig { skillId = "burn", projectile = null, direction = Vector3.right, name = "블래스트 번", hasRandomX = true, randomXRange = new Vector2(1f, 3f), explosionEffect = blastBurnEffect }  // 폭발 스킬
        };

        InitializeAllSkills();

        // 조민희 추가: 패시브 스킬 설정
        passiveSkillConfigs = new PassiveSkillConfig[]
        {
            new PassiveSkillConfig { skillId = "attack_boost", name = "공격력 강화" }
        };

        InitializeAllPassiveSkills();
    }

    // 조민희 추가: 패시브 스킬 초기화 헬퍼 메서드
    private void InitializePassiveSkill(SkillData skillData, string skillName)
    {
        if (skillData != null)
        {
            var skill = skillController.CreatePassiveSkill(skillData);
            if (skill != null)
            {
                skill.transform.SetParent(transform);
                skillController.AddPassiveSkill(skill);
                Debug.Log($"[Test] {skillName} 패시브 스킬 추가 완료");
            }
        }
    }

    // 조민희 추가: 모든 패시브 스킬 초기화
    private void InitializeAllPassiveSkills()
    {
        foreach (var config in passiveSkillConfigs)
        {
            InitializePassiveSkill(DataManager.skills.Get(config.skillId), config.name);
        }
    }

    // 조민희 추가: 모든 스킬 초기화
    private void InitializeAllSkills()
    {
        foreach (var config in skillConfigs)
        {
            InitializeSkill(DataManager.skills.Get(config.skillId), config.projectile, config.direction, config.name,
                config.offset, config.hasRandomX ? (object)config.randomXRange : null, config.explosionEffect);
        }
    }

    // 조민희 추가: 인스펙터 버튼용 - 모든 스킬 재생성
    [ContextMenu("모든 스킬 재생성")]
    public void RecreateAllSkills()
    {
        RemoveAllSkills();
        InitializeAllSkills();
        Debug.Log("[SkillTestInitializer] 모든 스킬 재생성 완료");
    }

    // 조민희 추가: 인스펙터 버튼용 - 모든 스킬 비활성화
    [ContextMenu("모든 스킬 비활성화")]
    public void DeactivateAllSkills()
    {
        var activeSkills = skillController.ActiveSkills;
        foreach (var skill in activeSkills)
        {
            skill.SetActive(false);
        }
        Debug.Log($"[SkillTestInitializer] {activeSkills.Count}개 스킬 비활성화됨");
    }

    // 조민희 추가: 인스펙터 버튼용 - 모든 스킬 활성화
    [ContextMenu("모든 스킬 활성화")]
    public void ActivateAllSkills()
    {
        var activeSkills = skillController.ActiveSkills;
        foreach (var skill in activeSkills)
        {
            skill.SetActive(true);
        }
        Debug.Log($"[SkillTestInitializer] {activeSkills.Count}개 스킬 활성화됨");
    }

    // 조민희 추가: 모든 자식 스킬 제거
    private void RemoveAllSkills()
    {
        // 자식 오브젝트 중 ActiveSkill 컴포넌트를 가진 오브젝트 제거
        var activeSkills = GetComponentsInChildren<ActiveSkill>(true);
        foreach (var skill in activeSkills)
        {
            if (Application.isPlaying)
                Destroy(skill.gameObject);
            else
                DestroyImmediate(skill.gameObject);
        }

        // SkillController의 스킬 목록도 비우기 (해당 기능이 있다면)
        Debug.Log($"[SkillTestInitializer] {activeSkills.Length}개 스킬 제거됨");
    }

    // 조민희 추가: 인스펙터 버튼용 - 특정 스킬만 재생성
    [ContextMenu("파이어볼 재생성")]
    public void RecreateFireball() => RecreateSkill("fireball", fireballProjectile, Vector3.right, "파이어볼");

    [ContextMenu("얼음 창 재생성")]
    public void RecreateIceSpear() => RecreateSkill("ice_spear", iceSpearProjectile, Vector3.right, "얼음 창");

    [ContextMenu("메테오 재생성")]
    public void RecreateMeteor() => RecreateSkill("meteor", meteorProjectile, Vector3.down, "메테오", new Vector3(0, 10, 0), randomX: new Vector2(1f, 3f));

    [ContextMenu("포이즌볼 재생성")]
    public void RecreatePoison() => RecreateSkill("poison", poisonProjectile, Vector3.right, "포이즌볼");

    [ContextMenu("블래스트 번 재생성")]
    public void RecreateBurn() => RecreateSkill("burn", null, Vector3.right, "블래스트 번", randomX: new Vector2(1f, 3f), explosionEffect: blastBurnEffect);

    // 조민희 추가: 특정 스킬만 재생성
    private void RecreateSkill(string skillId, SkillProjectile2D projectile, Vector3 direction, string skillName, Vector3 offset = default, object randomX = null, GameObject explosionEffect = null)
    {
        // 같은 ID의 스킬 찾아서 제거 (SkillController의 리스트에서 검색)
        var existingSkills = skillController.ActiveSkills
            .Where(s => s.Data != null && s.Data.id == skillId)
            .ToArray();

        foreach (var skill in existingSkills)
        {
            if (Application.isPlaying)
                Destroy(skill.gameObject);
            else
                DestroyImmediate(skill.gameObject);
        }

        // 새로 생성
        InitializeSkill(DataManager.skills.Get(skillId), projectile, direction, skillName, offset, randomX, explosionEffect);
        Debug.Log($"[SkillTestInitializer] {skillName} 스킬 재생성 완료");
    }

    #region 패시브 스킬 테스트용 ContextMenu

    // 조민희 추가: 인스펙터 버튼용 - 모든 패시브 스킬 재생성
    [ContextMenu("모든 패시브 스킬 재생성")]
    public void RecreateAllPassiveSkills()
    {
        RemoveAllPassiveSkills();
        InitializeAllPassiveSkills();
        Debug.Log("[SkillTestInitializer] 모든 패시브 스킬 재생성 완료");
    }

    // 조민희 추가: 모든 패시브 스킬 제거
    private void RemoveAllPassiveSkills()
    {
        var passiveSkills = skillController.PassiveSkills;
        foreach (var skill in passiveSkills.ToArray())
        {
            if (Application.isPlaying)
                Destroy(skill.gameObject);
            else
                DestroyImmediate(skill.gameObject);
        }
        Debug.Log($"[SkillTestInitializer] {passiveSkills.Count}개 패시브 스킬 제거됨");
    }

    // 조민희 추가: 인스펙터 버튼용 - 공격력 강화 재생성
    [ContextMenu("공격력 강화 재생성")]
    public void RecreateAttackBoost()
    {
        // 기존 패시브 스킬 제거
        var existingSkills = skillController.PassiveSkills
            .Where(s => s.Data != null && s.Data.id == "attack_boost")
            .ToArray();

        foreach (var skill in existingSkills)
        {
            if (Application.isPlaying)
                Destroy(skill.gameObject);
            else
                DestroyImmediate(skill.gameObject);
        }

        // 새로 생성
        InitializePassiveSkill(DataManager.skills.Get("attack_boost"), "공격력 강화");
        Debug.Log("[SkillTestInitializer] 공격력 강화 재생성 완료");
    }

    // 조민희 추가: 인스펙터 버튼용 - 패시브 스킬 버프 효과 로그
    [ContextMenu("패시브 스킬 버프 효과 확인")]
    public void LogPassiveBuffEffects()
    {
        var passiveSkills = skillController.PassiveSkills;
        foreach (var skill in passiveSkills)
        {
            if (skill != null && skill.Data != null)
            {
                Debug.Log($"[Passive] {skill.Data.name}: 활성={skill.IsActive}, 버프량={skill.GetBuffValue():F1}");
            }
        }
    }

    #endregion
}