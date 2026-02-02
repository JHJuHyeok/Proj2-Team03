﻿using UnityEngine;
using SlayerLegend.Skill;
using SlayerLegend.Skill.StatusEffects;
using System.Collections.Generic;

namespace SlayerLegend.Skill.Testing
{
    /// <summary>
    /// 테스트용 더미 적 캐릭터
    /// 체력을 가지고 액티브 스킬의 데미지를 받음
    /// 상태이상 시스템 지원
    /// </summary>
    public class DummyEnemy : MonoBehaviour, IDamageable, IStatusEffectAble
    {
        [Header("스탯")]
        [SerializeField] private float maxHealth = 500f;

        [Header("상태이상")]
        [SerializeField] private List<StatusEffect> activeEffects = new List<StatusEffect>();

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public List<StatusEffect> ActiveEffects => activeEffects;

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
        }

        /// <summary>
        /// 데미지를 입음
        /// </summary>
        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            Debug.Log($"[DummyEnemy] 피격! 데미지: {damage:F1}, 남은 HP: {CurrentHealth:F1}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"[DummyEnemy] 적 처치!");
            // 모든 상태이상 제거
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
                    null
                );
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
        }
        #endregion

        private void OnGUI()
        {
            // 적의 HP를 화면에 표시
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            float hpPercent = CurrentHealth / MaxHealth;

            float yOffset = 0;
            GUI.Box(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50 - yOffset, 100, 20),
                $"Enemy HP: {hpPercent * 100:F0}% ({CurrentHealth:F0}/{MaxHealth:F0})");
            yOffset += 25;

            // 활성화된 상태이상 표시
            if (activeEffects.Count > 0)
            {
                GUI.Box(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50 - yOffset, 100, 20),
                    $"상태이상: {activeEffects.Count}개");
                yOffset += 25;

                foreach (var effect in activeEffects)
                {
                    if (effect != null)
                    {
                        GUI.Box(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50 - yOffset, 100, 18),
                            $"{effect.EffectName}: {effect.RemainingTime:F1}s");
                        yOffset += 20;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            ClearAllStatusEffects();
        }
    }
}
