using System.Collections.Generic;
using UnityEngine;
using SlayerLegend.Equipment;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// Weapon Tab Panel 관리자
    /// 작성자: 조민희
    /// 등급별로 장비 번들을 표시하고 관리
    /// 씬에 미리 배치된 Equipment Bar와 Weapon Bundle들을 사용
    /// </summary>
    public class WeaponTabPanelUI : MonoBehaviour
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
                Debug.LogWarning("[WeaponTabPanelUI] DataManager.CurrentSaveData가 null - 빈 상태로 표시");
            }

            RefreshWeaponList();
        }

        #endregion

        #region 무기 목록 새로고침

        /// <summary>
        /// 무기 목록 새로고침 - 기존 배치된 번들에 데이터만 설정
        /// </summary>
        public void RefreshWeaponList()
        {
            if (EquipmentManager.Instance == null)
            {
                Debug.LogWarning("[WeaponTabPanelUI] EquipmentManager가 초기화되지 않음");
                return;
            }

            // 등급별 보유량 조회
            var gradeCounts = EquipmentManager.Instance.GetCountByGrade(EquipType.Weapon);

            // 각 등급별로 Equipment Bar의 번들에 데이터 설정
            FillEquipmentBar(commonEquipmentBar, EquipGrade.Common, gradeCounts);
            FillEquipmentBar(uncommonEquipmentBar, EquipGrade.Uncommon, gradeCounts);
            FillEquipmentBar(rareEquipmentBar, EquipGrade.Rare, gradeCounts);
            FillEquipmentBar(heroEquipmentBar, EquipGrade.Hero, gradeCounts);
            FillEquipmentBar(legendEquipmentBar, EquipGrade.Legend, gradeCounts);
            FillEquipmentBar(mythEquipmentBar, EquipGrade.Myth, gradeCounts);

            Debug.Log("[WeaponTabPanelUI] 무기 목록 새로고침 완료");
        }

        /// <summary>
        /// 특정 등급의 Equipment Bar에 있는 번들들에 데이터 채우기
        /// </summary>
        private void FillEquipmentBar(Transform equipmentBar, EquipGrade grade, Dictionary<EquipGrade, int> gradeCounts)
        {
            if (equipmentBar == null)
            {
                Debug.LogWarning($"[WeaponTabPanelUI] {grade} Equipment Bar가 null");
                return;
            }

            // 해당 Bar의 모든 WeaponBundleUI 가져오기
            WeaponBundleUI[] bundles = equipmentBar.GetComponentsInChildren<WeaponBundleUI>(true);

            if (bundles == null || bundles.Length == 0)
            {
                Debug.LogWarning($"[WeaponTabPanelUI] {grade} Equipment Bar에 WeaponBundleUI가 없음");
                return;
            }

            // sibling index 역순으로 정렬 (높은 index가 먼저 오도록 = 왼쪽→오른쪽)
            System.Array.Sort(bundles, (a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));

            // 해당 등급의 장비 목록 가져오기
            List<EquipData> weaponsOfGrade = GetWeaponsByGrade(grade);

            // 보유 개수 가져오기
            gradeCounts.TryGetValue(grade, out int ownedCount);

            Debug.Log($"[WeaponTabPanelUI] {grade}: 번들 {bundles.Length}개, 해당 등급 무기 {weaponsOfGrade.Count}개, 보유 {ownedCount}개");

            // 각 번들에 데이터 설정
            for (int i = 0; i < bundles.Length; i++)
            {
                WeaponBundleUI bundle = bundles[i];

                if (i < weaponsOfGrade.Count)
                {
                    EquipData weaponData = weaponsOfGrade[i];
                    int level = EquipmentManager.Instance.GetLevel(weaponData.GetId());

                    bundle.SetEquipData(weaponData, ownedCount, level);
                    bundle.gameObject.SetActive(true);

                    Debug.Log($"[WeaponTabPanelUI] 번들 {i} 설정: {weaponData.GetId()}, spriteName={weaponData.spriteName}");
                }
                else
                {
                    // 데이터가 없으면 비활성화
                    bundle.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 특정 등급의 모든 무기 데이터 반환 (gradeStep 순)
        /// </summary>
        private List<EquipData> GetWeaponsByGrade(EquipGrade grade)
        {
            List<EquipData> result = new List<EquipData>();

            foreach (var weapon in DataManager.weapons.GetAll())
            {
                if (weapon.GetGrade() == grade)
                {
                    result.Add(weapon);
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
            RefreshWeaponList();
        }

        [ContextMenu("등급별 개수 출력")]
        public void DebugPrintGradeCounts()
        {
            if (EquipmentManager.Instance == null) return;

            var counts = EquipmentManager.Instance.GetCountByGrade(EquipType.Weapon);
            Debug.Log("=== 무기 등급별 보유량 ===");
            foreach (var kvp in counts)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}개");
            }
        }

        #endregion
    }
}
