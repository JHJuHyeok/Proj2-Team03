﻿using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    // 상태이상을 받을 수 있는 대상 인터페이스
    public interface IStatusEffectAble
    {
        // 상태이상을 적용함
        void ApplyStatusEffect(StatusEffect effect);

        // 특정 타입의 상태이상 제거 (제네릭)
        void RemoveStatusEffect<T>() where T : StatusEffect;

        // 특정 타입의 상태이상 제거 (Type 파라미터)
        void RemoveStatusEffect(System.Type effectType);

        // 모든 상태이상 제거
        void ClearAllStatusEffects();

        // 현재 적용된 상태이상 목록
        List<StatusEffect> ActiveEffects { get; }

        // 최대 체력 (체력 비례 데미지 계산용)
        float MaxHealth { get; }
    }

    // 상태이상 시스템을 위한 MonoBehaviour 확장
    // IStatusEffectAble을 구현하는 클래스에서 사용할 수 있는 기본 구현
    public abstract class StatusEffectContainer : MonoBehaviour, IStatusEffectAble
    {
        [SerializeField] protected List<StatusEffect> activeEffects = new List<StatusEffect>();

        public List<StatusEffect> ActiveEffects => activeEffects;

        // 최대 체력 - 파생 클래스에서 반드시 구현해야 함
        // 체력 비례 데미지 계산에 사용됨
        public abstract float MaxHealth { get; }

        public virtual void ApplyStatusEffect(StatusEffect effect)
        {
            if (effect == null) return;

            // 동일한 타입의 상태이상이 있다면 기존 것 제거 (중복 방지)
            RemoveStatusEffect(effect.GetType());

            // 상태이상 컴포넌트 추가
            var addedEffect = gameObject.AddComponent(effect.GetType()) as StatusEffect;
            if (addedEffect != null)
            {
                // 기존 effect의 설정을 복사 (AddComponent로 생성된 객체는 필드가 초기화되므로)
                CopyEffectSettings(effect, addedEffect);

                activeEffects.Add(addedEffect);
                Debug.Log($"[StatusEffectContainer] 상태이상 적용: {effect.EffectName}");
            }
        }

        public virtual void RemoveStatusEffect<T>() where T : StatusEffect
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

        public virtual void RemoveStatusEffect(System.Type effectType)
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

        public virtual void ClearAllStatusEffects()
        {
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                {
                    effect.EndEffect();
                }
            }
            activeEffects.Clear();
            Debug.Log("[StatusEffectContainer] 모든 상태이상 제거");
        }

        private void CopyEffectSettings(StatusEffect source, StatusEffect target)
        {
            // DotEffect 특별 처리
            if (source is DotEffect sourceDot && target is DotEffect targetDot)
            {
                targetDot.Initialize(
                    sourceDot.Duration,
                    sourceDot.DamagePerTick,
                    sourceDot.TickInterval,
                    sourceDot.Target, // 원본 타겟 사용
                    null,
                    sourceDot.IsPercentageBased,
                    sourceDot.TargetMaxHp
                );
            }
            else
            {
                // 기본 필드 복사 (리플렉션 사용 가능하지만 간단하게 수동 복사)
                // AddComponent로 생성된 컴포넌트는 Awake에서 초기화됨
            }
        }

        protected virtual void OnDestroy()
        {
            // 객체 파괴 시 모든 상태이상 정리
            ClearAllStatusEffects();
        }
    }
}
