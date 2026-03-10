using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        // 각 등급별 테스트 악세서리 설정 (조민희 추가)
        private readonly Dictionary<string, (int count, int level)> testAccessories = new()
        {
            // Common (녹슨 팔찌 ~ 오래된 팬던트)
            { "AC_000", (5, 3) },   // 녹슨 팔찌: 5개, +3
            { "AC_001", (3, 1) },   // 나태의 귀걸이: 3개, +1
            { "AC_002", (2, 5) },   // 초조의 반지: 2개, +5
            { "AC_003", (1, 2) },   // 오래된 팬던트: 1개, +2

            // Uncommon (순수의 팔찌 ~ 기대의 팬던트)
            { "AC_004", (4, 7) },   // 순수의 팔찌: 4개, +7
            { "AC_005", (2, 3) },   // 항해의 귀걸이: 2개, +3
            { "AC_006", (1, 1) },   // 희생의 반지: 1개, +1
            { "AC_007", (1, 2) },   // 기대의 팬던트: 1개, +2

            // Rare (소원의 팔찌 ~ 갈망의 팬던트)
            { "AC_008", (3, 10) },  // 소원의 팔찌: 3개, +10
            { "AC_009", (1, 5) },   // 호박 귀걸이: 1개, +5
            { "AC_010", (2, 8) },   // 인내의 팔찌: 2개, +8
            { "AC_011", (1, 3) },   // 갈망의 팬던트: 1개, +3

            // Hero (단련의 팔찌 ~ 지혜의 팬던트)
            { "AC_012", (2, 15) },  // 단련의 팔찌: 2개, +15
            { "AC_013", (1, 8) },   // 성숙의 귀걸이: 1개, +8
            { "AC_014", (1, 5) },   // 연금술의 반지: 1개, +5
            { "AC_015", (1, 3) },   // 지혜의 팬던트: 1개, +3

            // Legend (야망의 팔찌 ~ 용기의 팬던트)
            { "AC_016", (1, 20) },  // 야망의 팔찌: 1개, +20
            { "AC_017", (1, 12) },  // 십자가 귀걸이: 1개, +12
            { "AC_018", (1, 8) },   // 마왕의 반지: 1개, +8
            { "AC_019", (1, 5) },   // 용기의 팬던트: 1개, +5

            // Myth (투신의 팔찌 ~ 혼돈의 팬던트)
            { "AC_020", (1, 30) },  // 투신의 팔찌: 1개, +30
            { "AC_021", (1, 25) },  // 고요의 귀걸이: 1개, +25
            { "AC_022", (1, 20) },  // 환상의 반지: 1개, +20
            { "AC_023", (1, 15) },  // 혼돈의 팬던트: 1개, +15
        };

        private void Awake()
        {
            // AssetBundle 초기화 (Resources.Load보다 먼저)
            if (enableTestMode)
            {
                AssetBundleLoader.Instance.Initialize();

                // [조민희] Awake에서 즉시 초기화 (OnEnable보다 먼저 실행되도록)
                if (initializeOnStart)
                {
                    InitializeTestData();
                }
            }
        }

        private async void Start()
        {
            // [조민희] Start에서 Addressables 데이터 로드
            if (enableTestMode)
            {
                await DataManager.LoadAllDatabase();
                Debug.Log("[TestEquipmentInitializer] 데이터베이스 로드 완료");
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

            // 테스트 무기 데이터 추가
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

            // 테스트 악세서리 데이터 추가 (조민희 추가)
            foreach (var kvp in testAccessories)
            {
                string accessoryId = kvp.Key;
                int count = kvp.Value.count;
                int level = kvp.Value.level;

                testData.equipInfo[accessoryId] = new Possesion
                {
                    count = count,
                    level = level
                };
            }

            // DataManager 초기화
            DataManager.Init(testData);

            Debug.Log($"[TestEquipmentInitializer] 테스트 데이터 초기화 완료 - {testWeapons.Count}개 무기, {testAccessories.Count}개 악세서리 추가됨");

            // 로그 출력
            LogTestDataSummary();
        }

        /// <summary>
        /// 테스트 데이터 요약 로그
        /// </summary>
        private void LogTestDataSummary()
        {
            Debug.Log("=== 테스트 장비 데이터 요약 ===");
            Debug.Log("-- 무기 --");
            foreach (var kvp in testWeapons)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.count}개, +{kvp.Value.level}");
            }
            Debug.Log("-- 악세서리 --");
            foreach (var kvp in testAccessories)
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

            // 무기 데이터 추가
            foreach (var kvp in testWeapons)
            {
                testData.equipInfo[kvp.Key] = new Possesion
                {
                    count = kvp.Value.count,
                    level = kvp.Value.level
                };
            }

            // 악세서리 데이터 추가 (조민희 추가)
            foreach (var kvp in testAccessories)
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
