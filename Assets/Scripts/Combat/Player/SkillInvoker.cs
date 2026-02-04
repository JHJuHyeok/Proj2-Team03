// using UnityEngine;
// using System;
// using System.Collections.Generic;
// using SlayerLegend.Skill;

// // 쿨다운과 마나 소모를 확인하여 스킬 발동을 관리함
// // 전투 시스템과 기존 스킬 시스템을 연결함
// public class SkillInvoker : MonoBehaviour
// {
//     [Header("참조")]
//     [SerializeField] private PlayerStats playerStats;

//     [Header("설정")]
//     [SerializeField] private bool isAutoMode = true;
//     [SerializeField] private int maxSkillSlots = 4;

//     // 스킬 추적
//     private Dictionary<int, float> _skillCooldowns = new Dictionary<int, float>();
//     private Dictionary<int, ActiveSkill> _registeredSkills = new Dictionary<int, ActiveSkill>();

//     // 이벤트
//     public event Action<int, float> OnSkillUsed; // skillIndex, cooldown
//     public event Action<int> OnSkillFailed; // skillIndex

//     public bool IsAutoMode
//     {
//         get => isAutoMode;
//         set => isAutoMode = value;
//     }

//     private void Awake()
//     {
//         if (playerStats == null)
//             playerStats = GetComponent<PlayerStats>();

//         if (playerStats == null)
//         {
//             Debug.LogError("[SkillInvoker] No PlayerStats found!");
//             enabled = false;
//         }
//     }

//     private void Update()
//     {
//         // 쿨다운 업데이트
//         List<int> keys = new List<int>(_skillCooldowns.Keys);
//         foreach (int skillIndex in keys)
//         {
//             if (_skillCooldowns[skillIndex] > 0)
//             {
//                 _skillCooldowns[skillIndex] -= Time.deltaTime;
//                 if (_skillCooldowns[skillIndex] < 0)
//                     _skillCooldowns[skillIndex] = 0;
//             }
//         }

//         // 자동 모드: 준비되면 자동으로 스킬 사용 시도
//         if (isAutoMode)
//         {
//             foreach (var kvp in _registeredSkills)
//             {
//                 int index = kvp.Key;
//                 ActiveSkill skill = kvp.Value;

//                 if (skill != null && skill.Data != null)
//                 {
//                     TryUseSkill(index, skill.Data.GetCooldown(skill.CurrentLevel), skill.Data.baseManaCost);
//                 }
//             }
//         }
//     }

//     // 특정 슬롯 인덱스에 스킬 등록
//     public void RegisterSkill(int skillIndex, ActiveSkill skill)
//     {
//         if (skillIndex < 0 || skillIndex >= maxSkillSlots)
//         {
//             Debug.LogWarning($"[SkillInvoker] 유효하지 않은 스킬 인덱스: {skillIndex}");
//             return;
//         }

//         _registeredSkills[skillIndex] = skill;
//         _skillCooldowns[skillIndex] = 0f;

//         Debug.Log($"[SkillInvoker] 스킬 등록됨 인덱스 {skillIndex}: {skill.Data.skillName}");
//     }

//     // 슬롯에서 스킬 등록 해제
//     public void UnregisterSkill(int skillIndex)
//     {
//         _registeredSkills.Remove(skillIndex);
//         _skillCooldowns.Remove(skillIndex);
//     }

//     // 스킬 사용 시도 - 쿨다운과 마나 확인
//     // skillIndex: 스킬 슬롯 인덱스
//     // cooldown: 쿨다운 시간 (초)
//     // manaCost: 스킬 사용 마나 비용
//     // 반환값: 스킬이 성공적으로 사용되었는지 여부
//     public bool TryUseSkill(int skillIndex, float cooldown, float manaCost)
//     {
//         // 스킬 인덱스 유효성 검사
//         if (!_registeredSkills.ContainsKey(skillIndex))
//         {
//             Debug.LogWarning($"[SkillInvoker] No skill registered at index {skillIndex}");
//             return false;
//         }

//         // 쿨다운 확인
//         if (_skillCooldowns.ContainsKey(skillIndex) && _skillCooldowns[skillIndex] > 0)
//         {
//             return false; // 아직 쿨다운 중
//         }

//         // 마나 비용 확인
//         if (!playerStats.UseMana(manaCost))
//         {
//             Debug.Log($"[SkillInvoker] 스킬 {skillIndex} 마나 부족. 필요: {manaCost}, 보유: {playerStats.CurrentMana}");
//             OnSkillFailed?.Invoke(skillIndex);
//             return false;
//         }

//         // 기존 스킬 시스템을 통해 스킬 실행
//         ActiveSkill skill = _registeredSkills[skillIndex];
//         if (skill != null)
//         {
//             // ActiveSkill 컴포넌트가 실제 실행을 처리함
//             // 여기서는 사용됨으로 표시하고 쿨다운 설정만 함
//             _skillCooldowns[skillIndex] = cooldown;
//             OnSkillUsed?.Invoke(skillIndex, cooldown);

//             Debug.Log($"[SkillInvoker] 스킬 사용함 {skillIndex}: {skill.Data.skillName}");
//             return true;
//         }

//         return false;
//     }

//     // 수동 스킬 발동 (UI 버튼 누름 등)
//     public void ActivateSkill(int skillIndex)
//     {
//         if (!_registeredSkills.ContainsKey(skillIndex))
//             return;

//         ActiveSkill skill = _registeredSkills[skillIndex];
//         if (skill != null && skill.Data != null)
//         {
//             TryUseSkill(skillIndex, skill.Data.GetCooldown(skill.CurrentLevel), skill.Data.baseManaCost);
//         }
//     }

//     // 스킬의 남은 쿨다운 가져오기
//     public float GetSkillCooldown(int skillIndex)
//     {
//         return _skillCooldowns.ContainsKey(skillIndex) ? _skillCooldowns[skillIndex] : 0f;
//     }

//     // 스킬 사용 가능 여부 확인
//     public bool IsSkillReady(int skillIndex)
//     {
//         if (!_registeredSkills.ContainsKey(skillIndex))
//             return false;

//         return !_skillCooldowns.ContainsKey(skillIndex) || _skillCooldowns[skillIndex] <= 0;
//     }

//     // 정규화된 쿨다운 가져오기 (0.0 = 준비됨, 1.0 = 방금 사용함)
//     public float GetSkillCooldownNormalized(int skillIndex)
//     {
//         if (!_registeredSkills.ContainsKey(skillIndex))
//             return 0f;

//         ActiveSkill skill = _registeredSkills[skillIndex];
//         if (skill == null || skill.Data == null)
//             return 0f;

//         float maxCooldown = skill.Data.GetCooldown(skill.CurrentLevel);
//         if (maxCooldown <= 0)
//             return 0f;

//         return GetSkillCooldown(skillIndex) / maxCooldown;
//     }
// }
