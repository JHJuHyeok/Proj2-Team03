using UnityEngine;
using System.Collections.Generic;
using SlayerLegend.Resource;

namespace SlayerLegend.Testing
{
    /// <summary>
    /// 테스트용 장비 더미 데이터 이니셜라이저
    /// 작성자: 조민희
    /// DataManager.CurrentSaveData가 null일 때 테스트 데이터 생성
    /// </summary>
    public class TestEquipmentInitializer : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private bool enableTestMode = true;
        [SerializeField] private bool initializeOnStart = true;

        // 각 등급별 테스트 무기 설정
        private readonly Dictionary<string, (int count, int level)> testWeapons = new()
        {
            // Common (녹슨검 ~ 강철검)
            { "WP_000", (5, 3) },   // 녹슨검: 5개, +3
            { "WP_001", (3, 1) },   // 넓은검: 3개, +1
            { "WP_002", (2, 5) },   // 장검: 2개, +5
            { "WP_003", (1, 2) },   // 강철검: 1개, +2

            // Uncommon (은검 ~ 세이버)
            { "WP_004", (4, 7) },   // 은검: 4개, +7
            { "WP_005", (2, 3) },   // 장식용 보검: 2개, +3
            { "WP_006", (1, 1) },   // 저주받은 톱검: 1개, +1

            // Rare (덩굴 레이피어 ~ 엘프족 장검)
            { "WP_008", (3, 10) },  // 덩굴 레이피어: 3개, +10
            { "WP_009", (1, 5) },   // 해적의 언월도: 1개, +5

            // Hero (광기의 칼날 ~ 타오르는 신념)
            { "WP_012", (2, 15) },  // 광기의 칼날: 2개, +15
            { "WP_013", (1, 8) },   // 흑기사 장검: 1개, +8

            // Legend (혹한의 칼날 ~ 마검 - 스틱스)
            { "WP_016", (1, 20) },  // 혹한의 칼날: 1개, +20
            { "WP_017", (1, 12) },  // 왕가의 보검: 1개, +12

            // Myth (파괴의 검 ~ 귀멸의 검)
            { "WP_020", (1, 30) },  // 파괴의 검 - 루인: 1개, +30
            { "WP_023", (1, 25) },  // 귀멸의 검: 1개, +25
        };

        private void Awake()
        {
            // AssetBundle 초기화 (Resources.Load보다 먼저)
            if (enableTestMode)
            {
                AssetBundleLoader.Instance.Initialize();
            }
        }

        private void Start()
        {
            if (initializeOnStart && enableTestMode)
            {
                InitializeTestData();
            }
        }

        /// <summary>
        /// 테스트 데이터 초기화
        /// </summary>
        [ContextMenu("테스트 데이터 초기화")]
        public void InitializeTestData()
        {
            if (!enableTestMode)
            {
                Debug.Log("[TestEquipmentInitializer] 테스트 모드 비활성화됨");
                return;
            }

            // 이미 데이터가 있으면 건너뜀
            if (DataManager.CurrentSaveData != null)
            {
                Debug.Log("[TestEquipmentInitializer] 이미 GameData가 존재함 - 초기화 건너뜀");
                return;
            }

            // 새 GameData 생성
            GameData testData = GameData.CreateDefault();

            // 테스트 장비 데이터 추가
            foreach (var kvp in testWeapons)
            {
                string weaponId = kvp.Key;
                int count = kvp.Value.count;
                int level = kvp.Value.level;

                testData.equipInfo[weaponId] = new Possesion
                {
                    count = count,
                    level = level
                };
            }

            // DataManager 초기화
            DataManager.Init(testData);

            Debug.Log($"[TestEquipmentInitializer] 테스트 데이터 초기화 완료 - {testWeapons.Count}개 무기 추가됨");

            // 로그 출력
            LogTestDataSummary();
        }

        /// <summary>
        /// 테스트 데이터 요약 로그
        /// </summary>
        private void LogTestDataSummary()
        {
            Debug.Log("=== 테스트 장비 데이터 요약 ===");
            foreach (var kvp in testWeapons)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.count}개, +{kvp.Value.level}");
            }
        }

        /// <summary>
        /// 테스트 데이터 초기화 (에디터 테스트용)
        /// </summary>
        [ContextMenu("강제 재초기화")]
        public void ForceReinitialize()
        {
            GameData testData = GameData.CreateDefault();

            foreach (var kvp in testWeapons)
            {
                testData.equipInfo[kvp.Key] = new Possesion
                {
                    count = kvp.Value.count,
                    level = kvp.Value.level
                };
            }

            DataManager.Init(testData);
            Debug.Log("[TestEquipmentInitializer] 강제 재초기화 완료");
            LogTestDataSummary();
        }
    }
}
