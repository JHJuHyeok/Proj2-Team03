using System;
using UnityEngine;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 장비 강화 관리자
    /// 작성자: 조민희
    /// 장비 레벨업 및 강화 비용 관리
    /// </summary>
    public class EnhanceManager : MonoBehaviour
    {
        public static EnhanceManager Instance { get; private set; }

        [Header("설정")]
        [SerializeField] private int maxLevel = 100;
        [SerializeField] private long baseCost = 1000;

        private EquipmentManager equipmentManager;

        /// <summary>강화 완료 이벤트 (장비 ID, 이전 레벨, 새 레벨)</summary>
        public event Action<string, int, int> OnEnhanceComplete;

        /// <summary>강화 실패 이벤트 (장비 ID, 사유)</summary>
        public event Action<string, string> OnEnhanceFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>강화 관리자 초기화</summary>
        public void Initialize(EquipmentManager equipmentManager)
        {
            this.equipmentManager = equipmentManager;
            Debug.Log("[EnhanceManager] 초기화 완료");
        }

        /// <summary>강화 가능 여부 확인</summary>
        public bool CanEnhance(EquipData equipment)
        {
            if (equipment == null) return false;
            if (equipmentManager == null) return false;

            string equipId = equipment.GetId();
            int currentLevel = equipmentManager.GetLevel(equipId);

            // 최대 레벨 확인
            if (currentLevel >= maxLevel) return false;

            // 보유 여부 확인
            int count = equipmentManager.GetCount(equipId);
            return count > 0;
        }

        /// <summary>강화 비용 계산</summary>
        public long GetEnhanceCost(EquipData equipment)
        {
            if (equipment == null) return 0;

            string equipId = equipment.GetId();
            int currentLevel = equipmentManager.GetLevel(equipId);

            // 기본 비용 * 레벨^2 (EquipmentManager와 동일한 공식)
            return baseCost * currentLevel * currentLevel;
        }

        /// <summary>강화 시도</summary>
        public bool TryEnhance(EquipData equipment)
        {
            if (!CanEnhance(equipment))
            {
                string reason = GetCannotEnhanceReason(equipment);
                OnEnhanceFailed?.Invoke(equipment.GetId(), reason);
                Debug.LogWarning($"[EnhanceManager] 강화 불가: {reason}");
                return false;
            }

            string equipId = equipment.GetId();
            int previousLevel = equipmentManager.GetLevel(equipId);

            // EquipmentManager의 Enhance 메서드 호출
            bool success = equipmentManager.Enhance(equipId);

            if (success)
            {
                int newLevel = equipmentManager.GetLevel(equipId);
                OnEnhanceComplete?.Invoke(equipId, previousLevel, newLevel);
                Debug.Log($"[EnhanceManager] 강화 성공: {equipment.GetName()} Lv.{previousLevel} → Lv.{newLevel}");
            }

            return success;
        }

        /// <summary>강화 불가 사유 메시지</summary>
        public string GetCannotEnhanceReason(EquipData equipment)
        {
            if (equipment == null) return "장비가 없습니다";
            if (equipmentManager == null) return "장비 관리자가 초기화되지 않았습니다";

            string equipId = equipment.GetId();
            int currentLevel = equipmentManager.GetLevel(equipId);

            if (currentLevel >= maxLevel)
                return $"최대 레벨 ({maxLevel})에 도달했습니다";

            int count = equipmentManager.GetCount(equipId);
            if (count <= 0)
                return "보유하지 않은 장비입니다";

            return "알 수 없는 이유";
        }

        /// <summary>강화 정보 조회 (UI 표시용)</summary>
        public EnhanceInfo GetEnhanceInfo(EquipData equipment)
        {
            if (equipment == null) return null;

            string equipId = equipment.GetId();
            int currentLevel = equipmentManager.GetLevel(equipId);
            long cost = GetEnhanceCost(equipment);
            bool canEnhance = CanEnhance(equipment);

            return new EnhanceInfo
            {
                equipmentId = equipId,
                currentLevel = currentLevel,
                nextLevel = currentLevel + 1,
                cost = cost,
                canEnhance = canEnhance,
                cannotReason = canEnhance ? "" : GetCannotEnhanceReason(equipment)
            };
        }
    }

    /// <summary>강화 정보 구조체</summary>
    public class EnhanceInfo
    {
        public string equipmentId;
        public int currentLevel;
        public int nextLevel;
        public long cost;
        public bool canEnhance;
        public string cannotReason;
    }
}
