using System;
using UnityEngine;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 장비 융합 관리자
    /// 같은 장비 5개를 합성하여 상위 등급 장비 1개로 변환
    /// </summary>
    public class FusionManager : MonoBehaviour
    {
        public static FusionManager Instance { get; private set; }

        [Header("설정")]
        [SerializeField] private int fusionMaterialCount = 5;

        private EquipmentManager equipmentManager;

        /// <summary>
        /// 융합 완료 이벤트 (재료 장비, 결과 장비)
        /// </summary>
        public event Action<EquipData, EquipData> OnFusionComplete;

        private void Awake()
        {
            // 싱글톤 패턴
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 융합 관리자 초기화
        /// </summary>
        /// <param name="equipmentManager">장비 관리자</param>
        public void Initialize(EquipmentManager equipmentManager)
        {
            this.equipmentManager = equipmentManager;
            EquipmentDatabase.Initialize();
            Debug.Log("[FusionManager] 초기화 완료");
        }

        /// <summary>
        /// 합성 가능 여부 확인
        /// </summary>
        /// <param name="equipment">확인할 장비</param>
        /// <returns>합성 가능 여부</returns>
        public bool CanFuse(EquipData equipment)
        {
            if (equipment == null) return false;
            if (equipmentManager == null) return false;

            // 재료 개수 확인
            int count = equipmentManager.GetEquipmentCount(equipment);
            if (count < fusionMaterialCount) return false;

            // 등급 확인 (Myth는 합성 불가)
            EquipGrade grade = equipment.GetGrade();
            if (grade >= EquipGrade.Myth) return false;

            // 다음 등급 장비 존재 확인
            EquipData nextGrade = EquipmentDatabase.GetNextGradeEquipment(equipment);
            return nextGrade != null;
        }

        /// <summary>
        /// 합성 시도
        /// </summary>
        /// <param name="equipment">합성할 장비</param>
        /// <param name="result">결과 장비</param>
        /// <returns>성공 여부</returns>
        public bool TryFuse(EquipData equipment, out EquipData result)
        {
            result = null;

            if (!CanFuse(equipment))
            {
                return false;
            }

            // 재료 5개 제거
            for (int i = 0; i < fusionMaterialCount; i++)
            {
                equipmentManager.RemoveFromInventory(equipment);
            }

            // 결과 장비 획득 (레벨 1)
            result = EquipmentDatabase.GetNextGradeEquipment(equipment);

            // 결과 장비 추가
            equipmentManager.AddToInventory(result, level: 1);

            // 이벤트 발생
            OnFusionComplete?.Invoke(equipment, result);

            string materialName = equipment.GetName();
            string resultName = result.GetName();
            Debug.Log($"[FusionManager] 융합 성공: {materialName} x{fusionMaterialCount} → {resultName}");

            return true;
        }

        /// <summary>
        /// 합성 불가 사유 메시지
        /// </summary>
        /// <param name="equipment">확인할 장비</param>
        /// <returns>불가 사유</returns>
        public string GetCannotFuseReason(EquipData equipment)
        {
            if (equipment == null) return "장비가 없습니다";

            int count = equipmentManager.GetEquipmentCount(equipment);
            if (count < fusionMaterialCount)
                return $"재료 부족 ({count}/{fusionMaterialCount})";

            EquipGrade grade = equipment.GetGrade();
            if (grade >= EquipGrade.Myth)
                return "최고 등급입니다";

            EquipData nextGrade = EquipmentDatabase.GetNextGradeEquipment(equipment);
            if (nextGrade == null)
                return "다음 등급 장비가 없습니다";

            return "알 수 없는 이유";
        }
    }
}
