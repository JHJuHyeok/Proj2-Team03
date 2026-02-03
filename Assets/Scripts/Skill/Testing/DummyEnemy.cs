﻿using UnityEngine;
using SlayerLegend.Skill;
using SlayerLegend.Skill.StatusEffects;
using System.Collections.Generic;

namespace SlayerLegend.Skill.Testing
{
    /// <summary>
    /// 테스트용 더미 적 캐릭터
    /// 체력을 가지고 액티브 스킬의 데미지를 받음
    /// CC 상태이상 테스트를 위한 IStunnable, IFreezable, IRootable 구현
    /// </summary>
    public class DummyEnemy : MonoBehaviour, IDamageable, IStatusEffectAble, IStunnable, IFreezable, IRootable
    {
        [Header("스탯")]
        [SerializeField] private float maxHealth = 500f;

        [Header("상태이상")]
        [SerializeField] private List<StatusEffect> activeEffects = new List<StatusEffect>();
        public List<StatusEffect> ActiveEffects => activeEffects;

        // CC 상태
        private bool isStunned = false;
        private bool isRooted = false;
        private float freezeSlowAmount = 0f; // 빙결로 감소한 속도량
        private int freezeStacks = 0;
        private float baseMoveSpeed = 2f; // 기본 이동 속도

        public float CurrentHealth { get; private set; }

        // CC 인터페이스 프로퍼티
        public bool IsStunned => isStunned;
        public bool IsRooted => isRooted;
        public int FreezeStacks => freezeStacks;
        public float CurrentMoveSpeed => isRooted ? 0f : (baseMoveSpeed * (1f - freezeSlowAmount));
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void Start()
        {
            Debug.Log($"[DummyEnemy] 적 생성 - HP: {CurrentHealth}");
        }

        private void Update()
        {
            // 만료된 상태이상 제거
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i] == null || activeEffects[i].IsExpired)
                {
                    activeEffects.RemoveAt(i);
                }
            }

            // 기절 상태일 때는 추가 행동 불가 (테스트용 더미는 이동이 없으므로 로그만)
            if (isStunned)
            {
                // 기절 중 행동 불가
            }
        }

        /// <summary>
        /// 데미지를 입음
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isStunned)
            {
                Debug.Log($"[DummyEnemy] 기절 중! 추가 피격!"); // 기절 중에는 추가 콤보 가능
            }
            CurrentHealth -= damage;
            Debug.Log($"[DummyEnemy] 피격! 데미지: {damage:F1}, 남은 HP: {CurrentHealth:F1}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 스킬 이름과 함께 데미지를 입음 (테스트용)
        /// </summary>
        public void TakeDamage(float damage, string skillName)
        {
            if (isStunned)
            {
                Debug.Log($"[DummyEnemy] 기절 중! [{skillName}] 추가 피격!");
            }
            CurrentHealth -= damage;
            Debug.Log($"[DummyEnemy] [{skillName}] 피격! 데미지: {damage:F1}, 남은 HP: {CurrentHealth:F1}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"[DummyEnemy] 적 처치!");
            ClearAllStatusEffects();
            // 테스트를 위해 3초 후 부활
            Invoke(nameof(Respawn), 3f);
        }

        private void Respawn()
        {
            CurrentHealth = maxHealth;
            ClearAllStatusEffects();
            Debug.Log($"[DummyEnemy] 적 리스폰 - HP: {CurrentHealth}");
        }

        #region IStatusEffectAble 구현
        public void ApplyStatusEffect(StatusEffect effect)
        {
            if (effect == null) return;

            // 동일한 타입의 상태이상이 있다면 기존 것 제거 (중복 방지)
            RemoveStatusEffect(effect.GetType());

            // 상태이상 컴포넌트 추가
            StatusEffect addedEffect = null;

            if (effect is DotEffect dotEffect)
            {
                addedEffect = gameObject.AddComponent<DotEffect>();
                (addedEffect as DotEffect).Initialize(
                    dotEffect.Duration,
                    dotEffect.DamagePerTick,
                    dotEffect.TickInterval,
                    this,
                    null,
                    dotEffect.IsPercentageBased,
                    dotEffect.TargetMaxHp > 0 ? dotEffect.TargetMaxHp : maxHealth
                );
            }
            else if (effect is StunEffect stunEffect)
            {
                addedEffect = gameObject.AddComponent<StunEffect>();
                (addedEffect as StunEffect).Initialize(stunEffect.Duration, this);
            }
            else if (effect is FreezeEffect freezeEffect)
            {
                addedEffect = gameObject.AddComponent<FreezeEffect>();
                // FreezeEffect.Initialize 내부에서 freezeStacks를 자동 계산하므로 별도 증가 불필요
                (addedEffect as FreezeEffect).Initialize(freezeEffect.Duration, this);
            }
            else if (effect is RootEffect rootEffect)
            {
                addedEffect = gameObject.AddComponent<RootEffect>();
                (addedEffect as RootEffect).Initialize(rootEffect.Duration, this);
            }
            else
            {
                addedEffect = gameObject.AddComponent(effect.GetType()) as StatusEffect;
            }

            if (addedEffect != null)
            {
                activeEffects.Add(addedEffect);
                Debug.Log($"[DummyEnemy] 상태이상 적용: {effect.EffectName}");
            }
        }

        public void RemoveStatusEffect<T>() where T : StatusEffect
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i] is T)
                {
                    activeEffects[i].EndEffect();
                    activeEffects[i] = null; // 가비지 컬렉션 돕기
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public void RemoveStatusEffect(System.Type effectType)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i] != null && activeEffects[i].GetType() == effectType)
                {
                    activeEffects[i].EndEffect();
                    activeEffects[i] = null; // 가비지 컬렉션 돕기
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public void ClearAllStatusEffects()
        {
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                {
                    effect.EndEffect();
                }
            }
            activeEffects.Clear();
            // CC 상태 초기화
            isStunned = false;
            isRooted = false;
            freezeSlowAmount = 0f;
            freezeStacks = 0;
        }
        #endregion

        private void OnGUI()
        {
            // 화면 비율에 따른 스케일 계산
            float scaleFactor = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            scaleFactor = Mathf.Max(scaleFactor, 0.7f);

            // 폰트 크기 조정
            int originalFontSize = GUI.skin.box.fontSize;
            GUI.skin.box.fontSize = Mathf.RoundToInt(12 * scaleFactor);

            // 적의 HP를 화면에 표시
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            float hpPercent = CurrentHealth / MaxHealth;

            float boxWidth = 120 * scaleFactor;
            float boxHeight = 22 * scaleFactor;
            float yOffset = 0;

            // HP 바
            GUI.Box(new Rect(screenPos.x - boxWidth / 2, Screen.height - screenPos.y - 50 - yOffset, boxWidth, boxHeight),
                $"Enemy HP: {hpPercent * 100:F0}% ({CurrentHealth:F0}/{MaxHealth:F0})");
            yOffset += boxHeight + 3 * scaleFactor;

            // CC 상태 표시
            if (isStunned)
            {
                GUI.Box(new Rect(screenPos.x - boxWidth / 2, Screen.height - screenPos.y - 50 - yOffset, boxWidth, boxHeight),
                    "[기절] 행동 불가!");
                yOffset += boxHeight + 3 * scaleFactor;
            }
            if (isRooted)
            {
                GUI.Box(new Rect(screenPos.x - boxWidth / 2, Screen.height - screenPos.y - 50 - yOffset, boxWidth, boxHeight),
                    "[속박] 이동 불가!");
                yOffset += boxHeight + 3 * scaleFactor;
            }
            if (freezeStacks > 0)
            {
                GUI.Box(new Rect(screenPos.x - boxWidth / 2, Screen.height - screenPos.y - 50 - yOffset, boxWidth, boxHeight),
                    $"[빙결] 이속 {freezeSlowAmount * 100:F0}% 감소");
                yOffset += boxHeight + 3 * scaleFactor;
            }

            // 활성화된 상태이상 표시
            if (activeEffects.Count > 0)
            {
                GUI.Box(new Rect(screenPos.x - boxWidth / 2, Screen.height - screenPos.y - 50 - yOffset, boxWidth, boxHeight),
                    $"상태이상: {activeEffects.Count}개");
                yOffset += boxHeight + 3 * scaleFactor;

                foreach (var effect in activeEffects)
                {
                    if (effect != null)
                    {
                        GUI.Box(new Rect(screenPos.x - boxWidth / 2, Screen.height - screenPos.y - 50 - yOffset, boxWidth, boxHeight),
                            $"{effect.EffectName}: {effect.RemainingTime:F1}s");
                        yOffset += boxHeight + 3 * scaleFactor;
                    }
                }
            }

            // 폰트 크기 복원
            GUI.skin.box.fontSize = originalFontSize;
        }

        private void OnDestroy()
        {
            ClearAllStatusEffects();
        }

        #region IStunnable 구현 (기절)
        public void ApplyStun(bool stunned)
        {
            isStunned = stunned;
            Debug.Log($"[DummyEnemy] 기절 상태: {(stunned ? "ON" : "OFF")}");
        }
        #endregion

        #region IFreezable 구현 (빙결)
        public void ApplyFreeze(float slowPercent)
        {
            freezeSlowAmount += slowPercent;
            freezeSlowAmount = Mathf.Min(freezeSlowAmount, 0.9f); // 최대 90% 감소
            Debug.Log($"[DummyEnemy] 빙결 적용! 총 이속 감소: {freezeSlowAmount * 100:F0}% (중첩: {freezeStacks})");
        }

        public void RemoveFreeze(float slowPercent)
        {
            freezeSlowAmount -= slowPercent;
            freezeSlowAmount = Mathf.Max(freezeSlowAmount, 0f);
            Debug.Log($"[DummyEnemy] 빙결 해제! 총 이속 감소: {freezeSlowAmount * 100:F0}%");
        }

        public void IncrementFreezeStacks()
        {
            freezeStacks++;
            Debug.Log($"[DummyEnemy] 빙결 중첩 증가! 현재 중첩: {freezeStacks}");
        }

        public void DecrementFreezeStacks()
        {
            freezeStacks--;
            freezeStacks = Mathf.Max(freezeStacks, 0); // 음수 방지
            Debug.Log($"[DummyEnemy] 빙결 중첩 감소! 현재 중첩: {freezeStacks}");
        }
        #endregion

        #region IRootable 구현 (속박)
        public void ApplyRoot(bool rooted)
        {
            isRooted = rooted;
            Debug.Log($"[DummyEnemy] 속박 상태: {(rooted ? "ON" : "OFF")}");
        }
        #endregion
    }
}
