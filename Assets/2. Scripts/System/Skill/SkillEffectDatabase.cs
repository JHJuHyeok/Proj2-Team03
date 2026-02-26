using UnityEngine;
using System.Collections.Generic;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// 스킬 이펙트 데이터베이스
    /// 인스펙터에서 스킬 ID별 프리팹을 할당하여 사용
    /// Addressables나 Resources 없이 직접 참조 방식
    /// </summary>
    [CreateAssetMenu(fileName = "SkillEffectDatabase", menuName = "Skill/SkillEffectDatabase")]
    public class SkillEffectDatabase : ScriptableObject
    {
        [System.Serializable]
        public class EffectEntry
        {
            public string skillId;
            public GameObject effectPrefab;
        }

        [Header("스킬 이펙트 목록")]
        [SerializeField] private List<EffectEntry> effects = new List<EffectEntry>();

        // 빠른 조회용 딕셔너리
        private Dictionary<string, GameObject> effectDict;

        private void OnEnable()
        {
            BuildDictionary();
        }

        private void BuildDictionary()
        {
            if (effectDict == null)
                effectDict = new Dictionary<string, GameObject>();
            else
                effectDict.Clear();

            foreach (var entry in effects)
            {
                if (!string.IsNullOrEmpty(entry.skillId) && entry.effectPrefab != null)
                {
                    effectDict[entry.skillId] = entry.effectPrefab;
                }
            }
        }

        /// <summary>
        /// 스킬 ID로 이펙트 프리팹 조회
        /// </summary>
        public GameObject GetEffect(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;

            // 딕셔너리가 없으면 재생성
            if (effectDict == null)
                BuildDictionary();

            if (effectDict.TryGetValue(skillId, out var prefab))
            {
                return prefab;
            }

            return null;
        }

        /// <summary>
        /// 등록된 이펙트 수
        /// </summary>
        public int Count => effects.Count;

#if UNITY_EDITOR
        /// <summary>
        /// 에디터용: 자동으로 스킬 ID 목록 채우기
        /// </summary>
        [ContextMenu("Auto Fill Skill IDs")]
        public void AutoFillSkillIds()
        {
            string[] skillIds = {
                "Fire_01", "Fire_03", "Fire_04", "Fire_07", "Fire_08", "Fire_10",
                "Water_01", "Water_04", "Water_08", "Water_10",
                "Wind_01", "Wind_02", "Wind_04", "Wind_06", "Wind_08", "Wind_09", "Wind_10",
                "Earth_01", "Earth_03", "Earth_04", "Earth_08", "Earth_09", "Earth_10"
            };

            // 기존 항목 유지하면서 새 항목 추가
            var existingIds = new HashSet<string>();
            foreach (var entry in effects)
            {
                existingIds.Add(entry.skillId);
            }

            foreach (var id in skillIds)
            {
                if (!existingIds.Contains(id))
                {
                    effects.Add(new EffectEntry { skillId = id, effectPrefab = null });
                }
            }

            BuildDictionary();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[SkillEffectDatabase] 자동 채우기 완료: {effects.Count}개 항목");
        }
#endif
    }
}
