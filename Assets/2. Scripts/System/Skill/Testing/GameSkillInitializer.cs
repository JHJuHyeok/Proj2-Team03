using UnityEngine;
using System.Collections;
using SlayerLegend.Skill;
using System.Collections.Generic;

namespace SlayerLegend.Skill.Testing
{
    /// <summary>
    /// 실제 게임 스킬 초기화기
    /// SkillList.json에 정의된 실제 스킬 ID를 사용하여 스킬을 초기화합니다.
    /// 작성자: 조민희
    /// </summary>
    public class GameSkillInitializer : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private SkillController skillController;

        [Header("공용 발사체 프리팹")]
        [SerializeField] private SkillProjectile2D fireProjectile;      // 불 속성 발사체
        [SerializeField] private SkillProjectile2D waterProjectile;     // 물 속성 발사체
        [SerializeField] private SkillProjectile2D windProjectile;      // 바람 속성 발사체
        [SerializeField] private SkillProjectile2D earthProjectile;     // 땅 속성 발사체

        [Header("공용 폭발 이펙트")]
        [SerializeField] private GameObject fireExplosionEffect;        // 불 폭발
        [SerializeField] private GameObject waterExplosionEffect;       // 물 폭발 (얼음)
        [SerializeField] private GameObject windExplosionEffect;        // 바람 폭발 (번개)
        [SerializeField] private GameObject earthExplosionEffect;       // 땅 폭발

        [Header("특수 발사체")]
        [SerializeField] private SkillProjectile2D iceSpearProjectile;  // 얼음 창 (관통)
        [SerializeField] private SkillProjectile2D meteorProjectile;    // 메테오 (위에서 떨어짐)
        [SerializeField] private DiagonalProjectile diagonalProjectile; // 대각선 발사체 (Water_04용)

        [Header("초기화 설정")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool useTestMode = true;               // 테스트 모드 (쿨타임 짧게, 마나 무제한)

        // 활성화된 스킬 목록
        private List<ActiveSkill> activeSkills = new List<ActiveSkill>();
        private List<PassiveSkill> passiveSkills = new List<PassiveSkill>();

        // 속성별 발사체 매핑
        private Dictionary<SkillElement, SkillProjectile2D> projectileByElement;
        private Dictionary<SkillElement, GameObject> explosionByElement;

        private void Awake()
        {
            // 매핑 초기화
            projectileByElement = new Dictionary<SkillElement, SkillProjectile2D>
            {
                { SkillElement.Fire, fireProjectile },
                { SkillElement.Water, waterProjectile },
                { SkillElement.Wind, windProjectile },
                { SkillElement.Earth, earthProjectile }
            };

            explosionByElement = new Dictionary<SkillElement, GameObject>
            {
                { SkillElement.Fire, fireExplosionEffect },
                { SkillElement.Water, waterExplosionEffect },
                { SkillElement.Wind, windExplosionEffect },
                { SkillElement.Earth, earthExplosionEffect }
            };
        }

        private IEnumerator Start()
        {
            // DataManager가 스킬 데이터를 로드할 때까지 대기
            // (CombatManager에서 이미 로드 중이므로 완료될 때까지 대기)
            int waitCount = 0;
            while (DataManager.skills.Get("Fire_01") == null && waitCount < 100)
            {
                yield return new WaitForSeconds(0.1f);
                waitCount++;
            }

            if (DataManager.skills.Get("Fire_01") == null)
            {
                Debug.LogWarning("[GameSkillInitializer] 스킬 데이터 로드 실패");
                yield break;
            }

            if (initializeOnStart)
            {
                InitializeAllSkills();
            }
        }

        /// <summary>
        /// 모든 스킬 초기화
        /// </summary>
        [ContextMenu("모든 스킬 초기화")]
        public void InitializeAllSkills()
        {
            if (skillController == null)
            {
                Debug.LogError("[GameSkillInitializer] SkillController가 할당되지 않았습니다.");
                return;
            }

            // 기존 스킬 제거
            ClearAllSkills();

            // Phase 2: 기본 AOE 스킬 (범위 공격)
            InitializeAOESkills();

            // Phase 3: 발사체/특수 스킬 (투사체, 다회 타격)
            InitializeProjectileSkills();

            // Phase 4: CC/특수 스킬 (상태이상)
            InitializeCCSkills();

            // Phase 5: 랜덤/소환 스킬 (랜덤 타겟)
            InitializeRandomSkills();

            // Phase 6: 기본 Passive 스킬 (버프)
            InitializePassiveSkills();

            // Phase 7: 누적형 Passive 스킬 (시간/공격 기반 누적)
            InitializeAccumulatingSkills();

            // Phase 8: 특수 Passive 스킬 (체력소모, 지연발동, 회복)
            InitializeSpecialPassiveSkills();
        }

        #region Phase 2: 기본 AOE 스킬

        /// <summary>
        /// 범위 공격 스킬 초기화
        /// </summary>
        private void InitializeAOESkills()
        {
            // Fire 속성 AOE (전체 범위 공격)
            CreateAOESkill("Fire_01", "불꽃 베기", 3f);       // 범위 3
            CreateAOESkill("Fire_03", "열풍", 3f);           // 범위 3
            CreateAOESkill("Fire_04", "화염 베기", 4f);      // 범위 4
            CreateAOESkill("Fire_07", "연옥 화염 베기", 6f); // 범위 6
            CreateAOESkill("Fire_08", "진 열풍", 3f);        // 범위 3

            // Earth 속성 AOE (다회 타격)
            CreateAOESkill("Earth_01", "스톤 스트라이크", 1f, hitCount: 1);  // 범위 1, 1회
            CreateAOESkill("Earth_03", "파워 스트라이크", 1f, hitCount: 2);  // 범위 1, 2회
            CreateAOESkill("Earth_08", "기가 스트라이크", 1f, hitCount: 3);  // 범위 1, 3회

            // Wind 속성 다중 타겟 (타겟 수 제한)
            CreateMultiTargetSkill("Wind_01", "번개 베기", maxTargets: 5, range: 5f);    // 5적, 범위 5
            CreateMultiTargetSkill("Wind_04", "천둥 베기", maxTargets: 7, range: 7f);    // 7적, 범위 7
            CreateMultiTargetSkill("Wind_08", "수라 번개 베기", maxTargets: 10, range: 10f); // 10적, 범위 10
            CreateMultiTargetSkill("Wind_09", "적뢰", maxTargets: 10, range: 20f);       // 10적, 범위 20
        }

        /// <summary>
        /// 범위 AOE 스킬 생성
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <param name="skillName">스킬 이름</param>
        /// <param name="radius">폭발 범위</param>
        /// <param name="maxTargets">최대 타겟 수 (-1 = 무제한)</param>
        /// <param name="hitCount">타격 횟수</param>
        private void CreateAOESkill(string skillId, string skillName, float radius, int maxTargets = -1, int hitCount = 1)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreateActiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);

            // 폭발 이펙트 설정
            if (explosionByElement.TryGetValue(skillData.element, out var effect) && effect != null)
            {
                skill.SetExplosionEffectPrefab(effect);
            }

            // effectData가 없으면 기본값 생성
            if (skillData.effectData == null)
            {
                skillData.effectData = new Data.SkillEffectData();
            }

            // AOE 설정
            skillData.effectData.isBlastSkill = true;
            skillData.effectData.explosionRadius = radius;

            // 조민희 추가: 타겟 제한 및 다회 타격 설정 (2026-02-24)
            skillData.effectData.maxTargets = maxTargets;
            skillData.effectData.hitCount = hitCount;

            // 테스트 모드 설정
            if (useTestMode)
            {
                skill.SetTestCooldown(0.5f);
                skill.SetTestNoManaCost(true);
            }

            skill.SetActive(true);
            activeSkills.Add(skill);
        }

        /// <summary>
        /// 다중 타겟 스킬 생성 (타겟 수 제한)
        /// </summary>
        private void CreateMultiTargetSkill(string skillId, string skillName, int maxTargets, float range)
        {
            // CreateAOESkill에 타겟 수 전달
            CreateAOESkill(skillId, skillName, range, maxTargets: maxTargets, hitCount: 1);
        }

        #endregion

        #region Phase 3: 발사체/특수 스킬

        /// <summary>
        /// 발사체 및 특수 스킬 초기화
        /// </summary>
        private void InitializeProjectileSkills()
        {
            // Fire_06: 화염 파동 - 발사체 + 스플래시
            CreateProjectileSkill("Fire_06", "화염 파동",
                splashRadius: 2f,       // 스플래시 범위
                useSplash: true);

            // Water_04: 파도베기 - 대각선 7연발
            CreateDiagonalProjectileSkill("Water_04", "파도베기", hitCount: 7);

            // Wind_02: 뇌격 - 15범위 랜덤 30회
            CreateRandomStrikeSkill("Wind_02", "뇌격",
                range: 15f,
                strikeCount: 30);

            // Wind_06: 전광석화 - 범위 3 AOE (돌진은 복잡하므로 일단 AOE)
            CreateAOESkill("Wind_06", "전광석화", 3f);
        }

        /// <summary>
        /// 발사체 스킬 생성 (스플래시 포함)
        /// </summary>
        private void CreateProjectileSkill(string skillId, string skillName, float splashRadius = 0f, bool useSplash = false)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 발사체 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreateActiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);

            // 발사체 프리팹 설정
            if (projectileByElement.TryGetValue(skillData.element, out var projectile) && projectile != null)
            {
                skill.SetProjectilePrefab(projectile);
            }

            // 폭발 이펙트 설정
            if (explosionByElement.TryGetValue(skillData.element, out var effect) && effect != null)
            {
                skill.SetExplosionEffectPrefab(effect);
            }

            // effectData 설정
            if (skillData.effectData == null)
            {
                skillData.effectData = new Data.SkillEffectData();
            }

            // 스플래시 설정
            if (useSplash && splashRadius > 0)
            {
                skillData.effectData.isBlastSkill = true;
                skillData.effectData.explosionRadius = splashRadius;
            }

            // 테스트 모드 설정
            if (useTestMode)
            {
                skill.SetTestCooldown(0.5f);
                skill.SetTestNoManaCost(true);
            }

            skill.SetActive(true);
            activeSkills.Add(skill);
        }

        /// <summary>
        /// 대각선 발사체 스킬 생성 (Water_04 파도베기용)
        /// - 위에서 오른쪽 아래로 대각선 이동
        /// </summary>
        private void CreateDiagonalProjectileSkill(string skillId, string skillName, int hitCount = 7)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 대각선 발사체 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreateActiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);

            // 대각선 발사체 프리팹 설정
            if (diagonalProjectile != null)
            {
                skill.SetProjectilePrefab(diagonalProjectile);
            }
            else
            {
                Debug.LogWarning($"[GameSkillInitializer] diagonalProjectile이 설정되지 않음");
            }

            // 폭발 이펙트 설정
            if (explosionByElement.TryGetValue(skillData.element, out var effect) && effect != null)
            {
                skill.SetExplosionEffectPrefab(effect);
            }

            // effectData 설정
            if (skillData.effectData == null)
            {
                skillData.effectData = new Data.SkillEffectData();
            }

            // 다회 타격 설정
            skillData.effectData.hitCount = hitCount;

            // 테스트 모드 설정
            if (useTestMode)
            {
                skill.SetTestCooldown(0.5f);
                skill.SetTestNoManaCost(true);
            }

            skill.SetActive(true);
            activeSkills.Add(skill);
        }

        /// <summary>
        /// 랜덤 타격 스킬 생성 (뇌격류)
        /// </summary>
        private void CreateRandomStrikeSkill(string skillId, string skillName, float range, int strikeCount)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 랜덤 타격 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreateActiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);

            // 폭발 이펙트 설정
            if (explosionByElement.TryGetValue(skillData.element, out var effect) && effect != null)
            {
                skill.SetExplosionEffectPrefab(effect);
            }

            // effectData 설정
            if (skillData.effectData == null)
            {
                skillData.effectData = new Data.SkillEffectData();
            }

            // 랜덤 타격 설정: 넓은 범위 + 다회 타격
            skillData.effectData.isBlastSkill = true;
            skillData.effectData.explosionRadius = range;     // 전체 탐색 범위
            skillData.effectData.maxTargets = -1;             // 무제한
            skillData.effectData.hitCount = strikeCount;      // 타격 횟수
            skillData.effectData.isRandomHit = true;          // 랜덤 타격 모드 활성화

            // 테스트 모드 설정
            if (useTestMode)
            {
                skill.SetTestCooldown(0.5f);
                skill.SetTestNoManaCost(true);
            }

            skill.SetActive(true);
            activeSkills.Add(skill);
        }

        #endregion

        #region Phase 4: CC/특수 스킬

        /// <summary>
        /// CC 및 특수 스킬 초기화 (Phase 4)
        /// </summary>
        private void InitializeCCSkills()
        {
            // Earth_04: 파워 임팩트 - 범위 5, 스턴 50%
            CreateCCSkill("Earth_04", "파워 임팩트",
                radius: 5f,
                isStun: true, stunDuration: 1f, stunChance: 50f);

            // Earth_09: 기가 임팩트 - 범위 5, 스턴 100%
            CreateCCSkill("Earth_09", "기가 임팩트",
                radius: 5f,
                isStun: true, stunDuration: 1f, stunChance: 100f);

            // Water_10: 블리자드 - 전방 모든 적, 빙결 100%
            CreateCCSkill("Water_10", "블리자드",
                radius: 10f,
                isFreeze: true, freezeDuration: 2f, freezeChance: 100f);

            // Fire_10: 불기둥 - 연속 폭발, 점차 강화 (7회, 점진적 데미지)
            CreateProgressiveSkill("Fire_10", "불기둥",
                radius: 4f,
                hitCount: 7,
                hitInterval: 0.3f);

            // Earth_10: 마수 사냥 - 7회 연속, 마지막 강화
            CreateProgressiveSkill("Earth_10", "마수 사냥",
                radius: 4f,
                hitCount: 7,
                hitInterval: 0.2f,
                lastHitMultiplier: 3f);
        }

        /// <summary>
        /// CC 스킬 생성 (스턴/빙결)
        /// </summary>
        private void CreateCCSkill(string skillId, string skillName, float radius,
            bool isStun = false, float stunDuration = 1f, float stunChance = 100f,
            bool isFreeze = false, float freezeDuration = 2f, float freezeChance = 100f)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] CC 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreateActiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);

            // 폭발 이펙트 설정
            if (explosionByElement.TryGetValue(skillData.element, out var effect) && effect != null)
            {
                skill.SetExplosionEffectPrefab(effect);
            }

            // effectData 설정
            if (skillData.effectData == null)
            {
                skillData.effectData = new Data.SkillEffectData();
            }

            // 기본 AOE 설정
            skillData.effectData.isBlastSkill = true;
            skillData.effectData.explosionRadius = radius;

            // CC 설정
            skillData.effectData.isStun = isStun;
            skillData.effectData.stunDuration = stunDuration;
            skillData.effectData.stunChance = stunChance;
            skillData.effectData.isFreeze = isFreeze;
            skillData.effectData.freezeDuration = freezeDuration;
            skillData.effectData.freezeChance = freezeChance;

            // 테스트 모드 설정
            if (useTestMode)
            {
                skill.SetTestCooldown(0.5f);
                skill.SetTestNoManaCost(true);
            }

            skill.SetActive(true);
            activeSkills.Add(skill);
        }

        /// <summary>
        /// 점진적 강화 스킬 생성 (연속 폭발)
        /// </summary>
        private void CreateProgressiveSkill(string skillId, string skillName, float radius,
            int hitCount, float hitInterval, float lastHitMultiplier = 2f)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 점진적 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreateActiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);

            // 폭발 이펙트 설정
            if (explosionByElement.TryGetValue(skillData.element, out var effect) && effect != null)
            {
                skill.SetExplosionEffectPrefab(effect);
            }

            // effectData 설정
            if (skillData.effectData == null)
            {
                skillData.effectData = new Data.SkillEffectData();
            }

            // 다회 타격 설정
            skillData.effectData.isBlastSkill = true;
            skillData.effectData.explosionRadius = radius;
            skillData.effectData.hitCount = hitCount;
            skillData.effectData.hitInterval = hitInterval;
            skillData.effectData.lastHitMultiplier = lastHitMultiplier;

            // 테스트 모드 설정
            if (useTestMode)
            {
                skill.SetTestCooldown(0.5f);
                skill.SetTestNoManaCost(true);
            }

            skill.SetActive(true);
            activeSkills.Add(skill);
        }

        #endregion

        #region Phase 5: 랜덤/소환 스킬

        /// <summary>
        /// 랜덤 타겟팅 스킬 초기화 (Phase 5)
        /// </summary>
        private void InitializeRandomSkills()
        {
            // Water_01: 아이스 스톤 - 범위 5, 랜덤 3회 타격
            CreateRandomStrikeSkill("Water_01", "아이스 스톤",
                range: 5f,
                strikeCount: 3);

            // Water_03: 아이스 샤워 - 범위 8, 랜덤 5회 타격
            CreateRandomStrikeSkill("Water_03", "아이스 샤워",
                range: 8f,
                strikeCount: 5);

            // Water_08: 아이스 타임 - 범위 10, 랜덤 8회 타격
            CreateRandomStrikeSkill("Water_08", "아이스 타임",
                range: 10f,
                strikeCount: 8);

            // Wind_10: 신속 - 6연속 돌진, 범위 7
            CreateMultiDashSkill("Wind_10", "신속",
                radius: 7f,
                dashCount: 6);
        }

        /// <summary>
        /// 연속 돌진 스킬 생성 (Wind_10용)
        /// </summary>
        private void CreateMultiDashSkill(string skillId, string skillName, float radius, int dashCount)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 돌진 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreateActiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);

            // 폭발 이펙트 설정
            if (explosionByElement.TryGetValue(skillData.element, out var effect) && effect != null)
            {
                skill.SetExplosionEffectPrefab(effect);
            }

            // effectData 설정
            if (skillData.effectData == null)
            {
                skillData.effectData = new Data.SkillEffectData();
            }

            // 다회 타격 AOE 설정 (돌진은 시각적으로 복잡하므로 AOE로 대체)
            skillData.effectData.isBlastSkill = true;
            skillData.effectData.explosionRadius = radius;
            skillData.effectData.hitCount = dashCount;
            skillData.effectData.hitInterval = 0.15f;  // 빠른 연속 타격
            skillData.effectData.isRandomHit = false;   // 범위 내 모든 적에게 타격

            // 테스트 모드 설정
            if (useTestMode)
            {
                skill.SetTestCooldown(0.5f);
                skill.SetTestNoManaCost(true);
            }

            skill.SetActive(true);
            activeSkills.Add(skill);
        }

        #endregion

        #region Phase 6: 기본 Passive 스킬

        /// <summary>
        /// 패시브 스킬 초기화
        /// </summary>
        private void InitializePassiveSkills()
        {
            // Fire - 공격력 버프
            CreatePassiveSkill("Fire_02", "불의 검");           // 10초간 공격력 +n%
            CreatePassiveSkill("Earth_06", "강철의 의지");       // 전체 공격력 +n%

            // Water - 마나/공속
            CreatePassiveSkill("Water_02", "마나의 축복");       // 마나 회복 +n%
            CreatePassiveSkill("Water_05", "흐르는 칼날");       // 10초간 공속 +n%

            // Wind - 이속/공속
            CreatePassiveSkill("Wind_03", "고속이동");           // 10초간 이속 +n%
            CreatePassiveSkill("Wind_07", "바람의 검");          // 공격속도 +n%

            // Earth - 체력/공격력
            CreatePassiveSkill("Earth_02", "대지의 축복");       // 체력회복 n%
            // Earth_07 (라이프 마나)은 Phase 8 특수 스킬로 이동
        }

        /// <summary>
        /// 패시브 스킬 생성
        /// </summary>
        private void CreatePassiveSkill(string skillId, string skillName)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 패시브 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreatePassiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);
            skillController.AddPassiveSkill(skill);
            passiveSkills.Add(skill);
        }

        #endregion

        #region Phase 7: 누적형 Passive 스킬

        /// <summary>
        /// 누적형 패시브 스킬 초기화 (Phase 7)
        /// </summary>
        private void InitializeAccumulatingSkills()
        {
            // 시간 기반 누적
            CreateAccumulatingPassiveSkill("Fire_05", "타오르는 검");       // 5초마다 공격력 +
            CreateAccumulatingPassiveSkill("Water_06", "굽이치는 칼날");   // 4초마다 공속 +

            // 공격 기반 누적
            CreateAccumulatingPassiveSkill("Wind_05", "가속의 검");         // 5회 공격마다 공속 +
            CreateAccumulatingPassiveSkill("Earth_05", "땅의 의지");       // 7회 공격마다 공격력 +
        }

        /// <summary>
        /// 누적형 패시브 스킬 생성
        /// </summary>
        private void CreateAccumulatingPassiveSkill(string skillId, string skillName)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 누적형 패시브 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreatePassiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);
            skillController.AddPassiveSkill(skill);
            passiveSkills.Add(skill);
        }

        #endregion

        #region Phase 8: 특수 Passive 스킬

        /// <summary>
        /// 특수 패시브 스킬 초기화 (Phase 8)
        /// 체력 소모형, 지연 발동형, 회복형
        /// </summary>
        private void InitializeSpecialPassiveSkills()
        {
            // Fire: 체력 소모형
            CreateSpecialPassiveSkill("Fire_09", "분노");           // 잃은 체력 비례 공격력
            CreateSpecialPassiveSkill("Fire_11", "워리어 번");      // 체력 50% 소모, 공격력 증가

            // Water: 회피, 쿨타임, 지연 발동
            CreateSpecialPassiveSkill("Water_07", "춤추는 파도");   // n초간 회피
            CreateSpecialPassiveSkill("Water_09", "메디테이션");    // 쿨타임 감소
            CreateSpecialPassiveSkill("Water_11", "격류");          // 2초 후 공속 증가

            // Wind: 체력 소모형
            CreateSpecialPassiveSkill("Wind_11", "뇌신");           // 체력 50% 소모, 공속 증가

            // Earth: 회복, 지연 발동
            CreateSpecialPassiveSkill("Earth_07", "라이프 마나");   // HP+MP 회복
            CreateSpecialPassiveSkill("Earth_11", "괴력난신");      // 20초 후 공격력 증가
        }

        /// <summary>
        /// 특수 패시브 스킬 생성 (Phase 8)
        /// </summary>
        private void CreateSpecialPassiveSkill(string skillId, string skillName)
        {
            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[GameSkillInitializer] 특수 패시브 스킬 데이터 없음: {skillId}");
                return;
            }

            var skill = skillController.CreatePassiveSkill(skillData);
            if (skill == null) return;

            skill.transform.SetParent(transform);
            skillController.AddPassiveSkill(skill);
            passiveSkills.Add(skill);
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 모든 스킬 제거
        /// </summary>
        [ContextMenu("모든 스킬 제거")]
        public void ClearAllSkills()
        {
            // Active 스킬 제거
            foreach (var skill in activeSkills)
            {
                if (skill != null && skill.gameObject != null)
                {
                    Destroy(skill.gameObject);
                }
            }
            activeSkills.Clear();

            // Passive 스킬 제거
            foreach (var skill in passiveSkills)
            {
                if (skill != null && skill.gameObject != null)
                {
                    skill.Deactivate();
                    Destroy(skill.gameObject);
                }
            }
            passiveSkills.Clear();
        }

        /// <summary>
        /// 스킬 상태 로그 출력
        /// </summary>
        [ContextMenu("스킬 상태 로그")]
        public void LogSkillStatus()
        {
            Debug.Log($"=== 게임 스킬 상태 ===");
            Debug.Log($"Active 스킬: {activeSkills.Count}개");
            foreach (var skill in activeSkills)
            {
                if (skill != null && skill.Data != null)
                {
                    Debug.Log($"  - {skill.Data.name}: 활성={skill.gameObject.activeSelf}");
                }
            }

            Debug.Log($"Passive 스킬: {passiveSkills.Count}개");
            foreach (var skill in passiveSkills)
            {
                if (skill != null && skill.Data != null)
                {
                    Debug.Log($"  - {skill.Data.name}: 활성={skill.IsActive}, 버프량={skill.GetBuffValue():F1}");
                }
            }
        }

        #endregion
    }
}
