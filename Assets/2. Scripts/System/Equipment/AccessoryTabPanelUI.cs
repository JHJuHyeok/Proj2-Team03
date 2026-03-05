using System.Collections.Generic;
using UnityEngine;
using SlayerLegend.Equipment;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// Accessory Tab Panel 관리자
    /// 작성자: 조민희
    /// 등급별로 악세서리 번들을 표시하고 관리
    /// 씬에 미리 배치된 Equipment Bar와 Weapon Bundle들을 사용
    /// WeaponBundleUI 컴포넌트를 재사용 (EquipData 기반)
    /// </summary>
    public class AccessoryTabPanelUI : MonoBehaviour
    {
        [Header("등급별 Equipment Bar (각 Bar의 자식 WeaponBundleUI 사용)")]
        [SerializeField] private Transform commonEquipmentBar;
        [SerializeField] private Transform uncommonEquipmentBar;
        [SerializeField] private Transform rareEquipmentBar;
        [SerializeField] private Transform heroEquipmentBar;
        [SerializeField] private Transform legendEquipmentBar;
        [SerializeField] private Transform mythEquipmentBar;

        [Header("참조")]
        [SerializeField] private bool autoRefreshOnEnable = true;

        #region Unity 라이프사이클

        private void OnEnable()
        {
            if (autoRefreshOnEnable)
            {
                StartCoroutine(RefreshWhenDataReady());
            }

            // EquipmentManager 이벤트 구독
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnInventoryChanged += OnInventoryChanged;
            }
        }

        private void OnDisable()
        {
            // EquipmentManager 이벤트 구독 해제
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnInventoryChanged -= OnInventoryChanged;
            }
        }

        private void OnInventoryChanged(EquipType type)
        {
            // 악세서리 타입이 변경되었을 때만 새로고침
            if (type == EquipType.Accessorie)
            {
                RefreshAccessoryList();
            }
        }

        #endregion

        #region 데이터 대기 코루틴

        private System.Collections.IEnumerator RefreshWhenDataReady()
        {
            // DataManager 초기화 대기 (최대 100프레임)
            int waitCount = 0;
            while (DataManager.CurrentSaveData == null && waitCount < 100)
            {
                yield return null;
                waitCount++;
            }

            if (DataManager.CurrentSaveData == null)
            {
                Debug.LogWarning("[AccessoryTabPanelUI] DataManager.CurrentSaveData가 null - 빈 상태로 표시");
            }

            RefreshAccessoryList();
        }

        #endregion

        #region 악세서리 목록 새로고침

        /// <summary>
        /// 악세서리 목록 새로고침 - 기존 배치된 번들에 데이터만 설정
        /// </summary>
        public void RefreshAccessoryList()
        {
            if (EquipmentManager.Instance == null)
            {
                Debug.LogWarning("[AccessoryTabPanelUI] EquipmentManager가 초기화되지 않음");
                return;
            }

            // 각 등급별로 Equipment Bar의 번들에 데이터 설정
            FillEquipmentBar(commonEquipmentBar, EquipGrade.Common);
            FillEquipmentBar(uncommonEquipmentBar, EquipGrade.Uncommon);
            FillEquipmentBar(rareEquipmentBar, EquipGrade.Rare);
            FillEquipmentBar(heroEquipmentBar, EquipGrade.Hero);
            FillEquipmentBar(legendEquipmentBar, EquipGrade.Legend);
            FillEquipmentBar(mythEquipmentBar, EquipGrade.Myth);
        }

        /// <summary>
        /// 특정 등급의 Equipment Bar에 있는 번들들에 데이터 채우기
        /// </summary>
        private void FillEquipmentBar(Transform equipmentBar, EquipGrade grade)
        {
            if (equipmentBar == null)
            {
                Debug.LogWarning($"[AccessoryTabPanelUI] {grade} Equipment Bar가 null");
                return;
            }

            // 해당 Bar의 모든 AccessoryBundleUI 가져오기
            AccessoryBundleUI[] bundles = equipmentBar.GetComponentsInChildren<AccessoryBundleUI>(true);

            if (bundles == null || bundles.Length == 0)
            {
                Debug.LogWarning($"[AccessoryTabPanelUI] {grade} Equipment Bar에 AccessoryBundleUI가 없음");
                return;
            }

            // sibling index 역순으로 정렬 (높은 index가 먼저 오도록 = 왼쪽→오른쪽)
            System.Array.Sort(bundles, (a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));

            // 해당 등급의 악세서리 목록 가져오기
            List<EquipData> accessoriesOfGrade = GetAccessoriesByGrade(grade);

            // 각 번들에 데이터 설정
            for (int i = 0; i < bundles.Length; i++)
            {
                AccessoryBundleUI bundle = bundles[i];

                if (i < accessoriesOfGrade.Count)
                {
                    EquipData accessoryData = accessoriesOfGrade[i];
                    string equipId = accessoryData.GetId();

                    // 개별 악세서리별 보유량과 레벨 조회
                    int count = EquipmentManager.Instance.GetCount(equipId);
                    int level = EquipmentManager.Instance.GetLevel(equipId);

                    bundle.SetEquipData(accessoryData, count, level);
                    bundle.gameObject.SetActive(true);
                }
                else
                {
                    // 데이터가 없으면 비활성화
                    bundle.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 특정 등급의 모든 악세서리 데이터 반환 (gradeStep 순)
        /// </summary>
        private List<EquipData> GetAccessoriesByGrade(EquipGrade grade)
        {
            List<EquipData> result = new List<EquipData>();

            foreach (var accessory in DataManager.accessories.GetAll())
            {
                if (accessory.GetGrade() == grade)
                {
                    result.Add(accessory);
                }
            }

            // gradeStep 순으로 정렬
            result.Sort((a, b) => a.gradeStep.CompareTo(b.gradeStep));

            return result;
        }

        #endregion

        #region 디버그

        [ContextMenu("새로고침")]
        public void DebugRefresh()
        {
            RefreshAccessoryList();
        }

        [ContextMenu("등급별 개수 출력")]
        public void DebugPrintGradeCounts()
        {
            if (EquipmentManager.Instance == null) return;

            var counts = EquipmentManager.Instance.GetCountByGrade(EquipType.Accessorie);
            Debug.Log("=== 악세서리 등급별 보유량 ===");
            foreach (var kvp in counts)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}개");
            }
        }

        #endregion
    }
}
