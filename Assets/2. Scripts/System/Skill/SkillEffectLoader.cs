using UnityEngine;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// 스킬 이펙트 로더
    /// - SkillEffectDatabase에서 프리팹 조회
    /// - 캐싱으로 성능 최적화
    /// - 조민희 수정: Addressables → ScriptableObject 방식으로 변경 (2026-02-26)
    /// </summary>
    public class SkillEffectLoader : MonoBehaviour
    {
        private static SkillEffectLoader _instance;
        public static SkillEffectLoader Instance => _instance;

        [Header("이펙트 데이터베이스")]
        [SerializeField] private SkillEffectDatabase effectDatabase;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (effectDatabase == null)
            {
                Debug.LogWarning("[SkillEffectLoader] effectDatabase가 할당되지 않았습니다!");
            }
            else
            {
                Debug.Log($"[SkillEffectLoader] 초기화 완료 - 등록된 이펙트: {effectDatabase.Count}개");
            }
        }

        /// <summary>
        /// 스킬 ID로 폭발 이펙트 프리팹 로드
        /// </summary>
        public GameObject LoadEffect(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;

            if (effectDatabase == null)
            {
                Debug.LogWarning($"[SkillEffectLoader] 데이터베이스 없음 - 이펙트 로드 실패: {skillId}");
                return null;
            }

            return effectDatabase.GetEffect(skillId);
        }

        /// <summary>
        /// 이펙트 인스턴스화 (지정된 위치에 생성)
        /// </summary>
        public GameObject SpawnEffect(string skillId, Vector3 position)
        {
            GameObject prefab = LoadEffect(skillId);
            if (prefab == null)
            {
                Debug.LogWarning($"[SkillEffectLoader] 이펙트 없음: {skillId}");
                return null;
            }

            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            return instance;
        }

        /// <summary>
        /// 스킬 ID에서 속성 추출 (Fire_01 → Fire)
        /// </summary>
        private string GetElementFromSkillId(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;

            int underscoreIndex = skillId.IndexOf('_');
            if (underscoreIndex > 0)
            {
                return skillId.Substring(0, underscoreIndex);
            }
            return null;
        }

        /// <summary>
        /// 프리팹이 등록되어 있는지 확인
        /// </summary>
        public bool HasEffect(string skillId)
        {
            return LoadEffect(skillId) != null;
        }
    }
}
