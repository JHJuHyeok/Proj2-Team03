﻿using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    /// <summary>
    /// 상태이상 기반 클래스
    /// 모든 상태이상(도트 데미지, CC, 버프 등)이 상속받음
    /// </summary>
    public abstract class StatusEffect : MonoBehaviour
    {
        [Header("상태이상 설정")]
        [SerializeField] protected float duration = 5f;
        [SerializeField] protected string effectName = "Status Effect";

        protected float remainingTime;
        protected bool isExpired = false;

        public string EffectName => effectName;
        public float RemainingTime => remainingTime;
        public bool IsExpired => isExpired;
        public float Duration => duration;

        protected virtual void Awake()
        {
            remainingTime = duration;
        }

        protected virtual void Update()
        {
            if (isExpired) return;

            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                OnExpire();
                isExpired = true;
                Destroy(this);
                return; // 즉시 종료하여 같은 프레임에서의 추가 접근 방지
            }
            else
            {
                OnTick();
            }
        }

        /// <summary>
        /// 매 프레임 호출됨 (지속 효과 처리)
        /// </summary>
        protected virtual void OnTick()
        {
            // 하위 클래스에서 오버라이드
        }

        /// <summary>
        /// 상태이상 만료 시 호출
        /// </summary>
        protected virtual void OnExpire()
        {
            Debug.Log($"[{effectName}] 상태이상 만료");
        }

        /// <summary>
        /// 상태이상 강제 종료
        /// </summary>
        public virtual void EndEffect()
        {
            OnExpire();
            isExpired = true;
            Destroy(this);
        }

        /// <summary>
        /// 남은 시간 설정 (디버그/테스트용)
        /// </summary>
        public void SetRemainingTime(float time)
        {
            remainingTime = Mathf.Clamp(time, 0f, duration);
        }
    }
}
