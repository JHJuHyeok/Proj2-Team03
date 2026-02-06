using UnityEngine;
using SlayerLegend.Equipment;  // 장비 시스템

namespace SlayerLegend.Skill.Testing
{
    // 메뉴 상태 열거형
    public enum MenuState
    {
        Main,           // 메인 메뉴
        GeneralSkill,   // 일반 스킬 테스트
        StatusEffect,   // 상태이상 스킬 테스트
        Equipment,      // 장비 테스트 메인
        WeaponTest,     // 무기 테스트
        AccessoryTest,  // 악세서리 테스트
        Fusion,         // 장비 융합
        LevelUp         // 장비 레벨업
    }

    // 간단한 스킬 테스트 - 계층형 메뉴 시스템
    public class SimpleSkillTest : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private DummyCharacter dummyCharacter;
        [SerializeField] private SkillController skillController;
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private FusionManager fusionManager;

        [Header("메뉴 상태")]
        [SerializeField] private MenuState currentMenu = MenuState.Main;
        [SerializeField] private bool showMenu = true;

        // 캐싱된 테스트 장비 데이터 (UI 표시용)
        private EquipData[] cachedWeapons;
        private EquipData[] cachedAccessories;

        private void Start()
        {
            // EquipmentManager가 없으면 생성
            if (equipmentManager == null)
            {
                GameObject managerObj = new GameObject("EquipmentManager");
                equipmentManager = managerObj.AddComponent<EquipmentManager>();
            }

            // DummyCharacter null 체크
            if (dummyCharacter == null)
            {
                Debug.LogError("[SimpleSkillTest] DummyCharacter 참조가 없습니다! Inspector에서 할당해주세요.");
                return;
            }

            // DummyCharacter를 장착 대상으로 설정
            equipmentManager.EquipTarget = dummyCharacter;

            // 테스트 장비 데이터 생성 및 캐싱
            cachedWeapons = TestEquipmentData.CreateTestWeapons();
            cachedAccessories = TestEquipmentData.CreateTestAccessories();

            // 기본 장비 3개씩 인벤토리에 추가

            // 각 무기를 3개씩 추가
            for (int i = 0; i < cachedWeapons.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    equipmentManager.AddToInventory(cachedWeapons[i]);
                }
            }

            // 각 악세서리를 3개씩 추가
            for (int i = 0; i < cachedAccessories.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    equipmentManager.AddToInventory(cachedAccessories[i]);
                }
            }

            // FusionManager 생성 및 초기화
            if (fusionManager == null)
            {
                GameObject fusionObj = new GameObject("FusionManager");
                fusionManager = fusionObj.AddComponent<FusionManager>();
            }
            fusionManager.Initialize(equipmentManager);

            // EquipmentLevelManager는 EquipmentManager가 이미 생성하므로 제거
            // EquipmentManager.Awake()에서 자동 생성됨

            Debug.Log("[SimpleSkillTest] 초기화 완료 - EquipmentManager 연결됨");
        }

        private void Update()
        {
            // 메뉴 토글 (M 키)
            if (Input.GetKeyDown(KeyCode.M))
            {
                showMenu = !showMenu;
            }

            // 기능키 (모든 메뉴에서 사용 가능)
            if (Input.GetKeyDown(KeyCode.G))
            {
                dummyCharacter.TestAddGold(1000);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                dummyCharacter.TestHeal();
            }
            // J: 모든 장비 +1씩 추가 (융합 테스트용)
            if (Input.GetKeyDown(KeyCode.J))
            {
                AddAllEquipment();
            }

            // 메뉴 상태별 키 입력 처리
            switch (currentMenu)
            {
                case MenuState.Main:
                    HandleMainMenuInput();
                    break;
                case MenuState.GeneralSkill:
                    HandleGeneralSkillMenuInput();
                    break;
                case MenuState.StatusEffect:
                    HandleStatusEffectMenuInput();
                    break;
                case MenuState.Equipment:
                    HandleEquipmentMenuInput();
                    break;
                case MenuState.WeaponTest:
                    HandleWeaponTestInput();
                    break;
                case MenuState.AccessoryTest:
                    HandleAccessoryTestInput();
                    break;
                case MenuState.Fusion:
                    HandleFusionMenuInput();
                    break;
                case MenuState.LevelUp:
                    HandleLevelUpMenuInput();
                    break;
            }
        }

        #region 메인 메뉴 입력 처리
        private void HandleMainMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentMenu = MenuState.GeneralSkill;
                Debug.Log("[Menu] 일반 스킬 테스트 메뉴로 진입");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentMenu = MenuState.StatusEffect;
                Debug.Log("[Menu] 상태이상 스킬 테스트 메뉴로 진입");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentMenu = MenuState.Equipment;
                Debug.Log("[Menu] 장비 테스트 메뉴로 진입");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                currentMenu = MenuState.LevelUp;
                Debug.Log("[Menu] 장비 레벨업 메뉴로 진입");
            }
        }
        #endregion

        #region 일반 스킬 테스트 메뉴 입력 처리
        private void HandleGeneralSkillMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddSkill("fireball", "파이어볼", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AddSkill("ice_spear", "얼음 창", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AddSkill("attack_boost", "공격력 강화", false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AddSkill("crit_boost", "치명타 강화", false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                currentMenu = MenuState.Main;
                Debug.Log("[Menu] 메인 메뉴로 돌아감");
            }
        }
        #endregion

        #region 상태이상 스킬 테스트 메뉴 입력 처리
        private void HandleStatusEffectMenuInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddSkill("poison", "독 (DoT)", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AddSkill("burn", "화상 (DoT)", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AddSkill("stun_bash", "기절 공격", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AddSkill("freeze_blast", "빙결 폭발", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                AddSkill("root_vine", "속박 덩굴", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                currentMenu = MenuState.Main;
                Debug.Log("[Menu] 메인 메뉴로 돌아감");
            }
        }
        #endregion

        private void AddSkill(string id, string name, bool isActive)
        {
            var data = DataManager.Instance?.skills?.Get(id);
            if (data == null)
            {
                Debug.LogError($"스킬 데이터를 찾을 수 없습니다: {id}");
                return;
            }

            if (isActive)
            {
                var skill = skillController.CreateActiveSkill(data);
                skill.transform.SetParent(dummyCharacter.transform);
                skillController.AddActiveSkill(skill);
                Debug.Log($"[Test] 액티브 스킬 장착: {name}");
            }
            else
            {
                var skill = skillController.CreatePassiveSkill(data);
                skill.transform.SetParent(dummyCharacter.transform);
                skillController.AddPassiveSkill(skill);
                Debug.Log($"[Test] 패시브 스킬 장착: {name}");
            }
        }

        private void OnGUI()
        {
            if (!showMenu) return;

            // 화면 비율에 따른 스케일 계산 (기준: 1920x1080)
            float scaleFactor = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            scaleFactor = Mathf.Max(scaleFactor, 0.6f); // 최소 60% 보장

            // 폰트 크기 조정
            GUI.skin.box.fontSize = Mathf.RoundToInt(16 * scaleFactor);
            GUI.skin.label.fontSize = Mathf.RoundToInt(14 * scaleFactor);

            float xOffset = 10;
            float yOffset = 10;
            float boxWidth = 300 * scaleFactor;

            GUILayout.BeginArea(new Rect(xOffset, yOffset, boxWidth, Screen.height - yOffset * 2));

            // 공통 기능 안내
            GUILayout.Box("=== 공통 기능 ===");
            GUILayout.Box("G: 골드 +1000\nH: 체력/마나 회복\nJ: 모든 장비 +1씩\nM: 메뉴 숨기기/표시");
            GUILayout.Space(10 * scaleFactor);

            // 현재 메뉴 상태에 따른 UI 표시
            switch (currentMenu)
            {
                case MenuState.Main:
                    DrawMainMenu();
                    break;
                case MenuState.GeneralSkill:
                    DrawGeneralSkillMenu();
                    break;
                case MenuState.StatusEffect:
                    DrawStatusEffectMenu();
                    break;
                case MenuState.Equipment:
                    DrawEquipmentMenu();
                    break;
                case MenuState.WeaponTest:
                    DrawWeaponTestMenu();
                    break;
                case MenuState.AccessoryTest:
                    DrawAccessoryTestMenu();
                    break;
                case MenuState.Fusion:
                    DrawFusionMenu();
                    break;
                case MenuState.LevelUp:
                    DrawLevelUpMenu();
                    break;
            }

            GUILayout.EndArea();
        }

        private void DrawMainMenu()
        {
            GUILayout.Box("=== 메인 테스트 메뉴 ===");
            GUILayout.Box(
                "1: 일반 스킬 테스트\n  (파이어볼, 얼음창, 버프)\n\n" +
                "2: 상태이상 스킬 테스트\n  (도트, 기절, 빙결, 속박)\n\n" +
                "3: 장비 테스트\n  (무기, 악세서리 장착)\n\n" +
                "4: 장비 레벨업\n  (같은 ID 장비 레벨 공유)");
            GUILayout.Box($"현재 상태: [메인 메뉴]");
        }

        private void DrawGeneralSkillMenu()
        {
            GUILayout.Box("=== 일반 스킬 테스트 ===");
            GUILayout.Box(
                "1: 파이어볼 (액티브)\n" +
                "2: 얼음 창 (액티브)\n" +
                "3: 공격력 강화 (패시브)\n" +
                "4: 치명타 강화 (패시브)\n\n" +
                "9: 메인 메뉴로 돌아가기");
            GUILayout.Box($"현재 상태: [일반 스킬]");
        }

        private void DrawStatusEffectMenu()
        {
            GUILayout.Box("=== 상태이상 스킬 테스트 ===");
            GUILayout.Box(
                "1: 독 (DoT)\n" +
                "2: 화상 (DoT)\n\n" +
                "3: 기절 공격 (Stun)\n" +
                "4: 빙결 폭발 (Freeze)\n" +
                "5: 속박 덩굴 (Root)\n\n" +
                "9: 메인 메뉴로 돌아가기");
            GUILayout.Box($"현재 상태: [상태이상 스킬]");
        }

        #region 장비 테스트 메뉴 입력 처리
        private void HandleEquipmentMenuInput()
        {
            // 1: 무기 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentMenu = MenuState.WeaponTest;
                Debug.Log("[Menu] 무기 테스트 메뉴로 진입");
            }
            // 2: 악세서리 테스트
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentMenu = MenuState.AccessoryTest;
                Debug.Log("[Menu] 악세서리 테스트 메뉴로 진입");
            }
            // 3: 장비 융합
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentMenu = MenuState.Fusion;
                Debug.Log("[Menu] 융합 메뉴로 진입");
            }
            // 메인 복귀
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                currentMenu = MenuState.Main;
                Debug.Log("[Menu] 메인 메뉴로 돌아감");
            }
        }

        private void HandleWeaponTestInput()
        {
            // 무기 장착 (1~6)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EquipTestWeapon(0);  // 녹슨 검
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EquipTestWeapon(1);  // 은검
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                EquipTestWeapon(2);  // 덩굴 레이피어
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                EquipTestWeapon(3);  // 광기의 칼날
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                EquipTestWeapon(4);  // 혹한의 칼날
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                EquipTestWeapon(5);  // 파괴의 검-루인
            }
            // 무기 해제
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                equipmentManager?.UnequipItem(EquipType.Weapon);
            }
            // 장비 테스트 메뉴 복귀
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                currentMenu = MenuState.Equipment;
                Debug.Log("[Menu] 장비 테스트 메뉴로 돌아감");
            }
        }

        private void HandleAccessoryTestInput()
        {
            // 악세서리 장착 (1~6)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EquipTestAccessory(0);  // 녹슨 팔찌
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EquipTestAccessory(1);  // 순수의 반지
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                EquipTestAccessory(2);  // 소원의 팔찌
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                EquipTestAccessory(3);  // 단련의 팔찌
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                EquipTestAccessory(4);  // 야망의 팔찌
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                EquipTestAccessory(5);  // 투신의 팔찌
            }
            // 악세서리 해제
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                equipmentManager?.UnequipItem(EquipType.Accessorie);
            }
            // 장비 테스트 메뉴 복귀
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                currentMenu = MenuState.Equipment;
                Debug.Log("[Menu] 장비 테스트 메뉴로 돌아감");
            }
        }
        #endregion

        #region 장비 테스트 UI
        private void DrawEquipmentMenu()
        {
            GUILayout.Box("=== 장비 테스트 ===");
            GUILayout.Box(
                "1: 무기 테스트\n" +
                "2: 악세서리 테스트\n" +
                "3: 장비 융합\n\n" +
                "9: 메인 메뉴로 돌아가기");
        }

        private void DrawWeaponTestMenu()
        {
            GUILayout.Box("=== 무기 테스트 ===");
            GUILayout.Box(
                "=== 무기 장착 (1~6) ===\n" +
                $"1: 녹슨 검 (Common) - 보유: {GetInventoryCount(cachedWeapons, 0)}개\n" +
                $"2: 은검 (Uncommon) - 보유: {GetInventoryCount(cachedWeapons, 1)}개\n" +
                $"3: 덩굴 레이피어 (Rare) - 보유: {GetInventoryCount(cachedWeapons, 2)}개\n" +
                $"4: 광기의 칼날 (Hero) - 보유: {GetInventoryCount(cachedWeapons, 3)}개\n" +
                $"5: 혹한의 칼날 (Legend) - 보유: {GetInventoryCount(cachedWeapons, 4)}개\n" +
                $"6: 파괴의 검-루인 (Myth) - 보유: {GetInventoryCount(cachedWeapons, 5)}개\n\n" +
                "Q: 무기 해제\n\n" +
                "9: 장비 테스트 메뉴로 돌아가기");

            // 플레이어 스탯 표시
            DrawPlayerStats();

            // 현재 장착 중인 장비 표시
            DrawEquippedItems();
        }

        private void DrawAccessoryTestMenu()
        {
            GUILayout.Box("=== 악세서리 테스트 ===");
            GUILayout.Box(
                "=== 악세서리 장착 (1~6) ===\n" +
                $"1: 녹슨 팔찌 (Common) - 보유: {GetInventoryCount(cachedAccessories, 0)}개\n" +
                $"2: 순수의 반지 (Uncommon) - 보유: {GetInventoryCount(cachedAccessories, 1)}개\n" +
                $"3: 소원의 팔찌 (Rare) - 보유: {GetInventoryCount(cachedAccessories, 2)}개\n" +
                $"4: 단련의 팔찌 (Hero) - 보유: {GetInventoryCount(cachedAccessories, 3)}개\n" +
                $"5: 야망의 팔찌 (Legend) - 보유: {GetInventoryCount(cachedAccessories, 4)}개\n" +
                $"6: 투신의 팔찌 (Myth) - 보유: {GetInventoryCount(cachedAccessories, 5)}개\n\n" +
                "Q: 악세서리 해제\n\n" +
                "9: 장비 테스트 메뉴로 돌아가기");

            // 플레이어 스탯 표시
            DrawPlayerStats();

            // 현재 장착 중인 장비 표시
            DrawEquippedItems();
        }

        private void DrawPlayerStats()
        {
            if (dummyCharacter != null)
            {
                GUILayout.Space(5);
                GUILayout.Box("=== 플레이어 스탯 ===");
                GUILayout.Label($"공격력: {dummyCharacter.AttackDamage:F1}");
                GUILayout.Label($"체력: {dummyCharacter.MaxHealth:F1}");
                GUILayout.Label($"크리율: {dummyCharacter.CriticalRate:P2}");
                GUILayout.Label($"크리뎀: {dummyCharacter.CriticalDamage:F2}");
                GUILayout.Label($"골드: {dummyCharacter.CurrentGold}");
            }
        }

        private void DrawEquippedItems()
        {
            if (equipmentManager != null)
            {
                GUILayout.Space(5);
                var slots = equipmentManager.GetAllSlots();
                foreach (var kvp in slots)
                {
                    EquipData equip = kvp.Value.EquippedItem;
                    string equipInfo = equip != null
                        ? $"[{equip.GetGradeString()}] {equip.GetName()} (Lv.{kvp.Value.Level})"
                        : "(비어있음)";
                    GUILayout.Label($"{kvp.Key}: {equipInfo}");
                }
            }
        }

        // 인벤토리 보유량 조회 헬퍼
        private int GetInventoryCount(EquipData[] items, int index)
        {
            if (items == null || equipmentManager == null || index < 0 || index >= items.Length)
                return 0;
            return equipmentManager.GetEquipmentCount(items[index]);
        }
        #endregion

        #region 장비 장착 헬퍼
        private void EquipTestWeapon(int index)
        {
            if (cachedWeapons != null && index >= 0 && index < cachedWeapons.Length)
            {
                // 먼저 인벤토리에 추가 (테스트용)
                equipmentManager?.AddToInventory(cachedWeapons[index]);
                // 그 다음 장착
                equipmentManager?.EquipItem(cachedWeapons[index]);
            }
        }

        private void EquipTestAccessory(int index)
        {
            if (cachedAccessories != null && index >= 0 && index < cachedAccessories.Length)
            {
                // 먼저 인벤토리에 추가 (테스트용)
                equipmentManager?.AddToInventory(cachedAccessories[index]);
                // 그 다음 장착
                equipmentManager?.EquipItem(cachedAccessories[index]);
            }
        }
        #endregion

        #region 장비 융합 메뉴
        private void HandleFusionMenuInput()
        {
            // 무기 융합 (1-5: Common ~ Legend)
            for (int i = 0; i < 5; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryFusionWeapon(i);
                }
            }

            // 악세서리 융합 (Q-T: Common ~ Legend)
            if (Input.GetKeyDown(KeyCode.Q)) TryFusionAccessory(0);
            else if (Input.GetKeyDown(KeyCode.W)) TryFusionAccessory(1);
            else if (Input.GetKeyDown(KeyCode.E)) TryFusionAccessory(2);
            else if (Input.GetKeyDown(KeyCode.R)) TryFusionAccessory(3);
            else if (Input.GetKeyDown(KeyCode.T)) TryFusionAccessory(4);

            // 장비 메뉴 복귀
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                currentMenu = MenuState.Equipment;
                Debug.Log("[Menu] 장비 메뉴로 돌아감");
            }
        }

        private void TryFusionWeapon(int index)
        {
            if (cachedWeapons == null || index >= cachedWeapons.Length) return;
            EquipData weapon = cachedWeapons[index];

            if (fusionManager != null && fusionManager.TryFuse(weapon, out EquipData result))
            {
                Debug.Log($"[융합 성공] {weapon.GetName()} x5 → {result.GetName()}");
            }
            else
            {
                string reason = fusionManager?.GetCannotFuseReason(weapon) ?? "FusionManager 없음";
                Debug.Log($"[융합 실패] {reason}");
            }
        }

        private void TryFusionAccessory(int index)
        {
            if (cachedAccessories == null || index >= cachedAccessories.Length) return;
            EquipData accessory = cachedAccessories[index];

            if (fusionManager != null && fusionManager.TryFuse(accessory, out EquipData result))
            {
                Debug.Log($"[융합 성공] {accessory.GetName()} x5 → {result.GetName()}");
            }
            else
            {
                string reason = fusionManager?.GetCannotFuseReason(accessory) ?? "FusionManager 없음";
                Debug.Log($"[융합 실패] {reason}");
            }
        }

        private void DrawFusionMenu()
        {
            GUILayout.Box("=== 장비 융합 ===");
            GUILayout.Box("같은 등급 장비 5개를 합성하여 상위 등급 1개로 변환\n(레벨 1 초기화, 무료)");

            // 무기 융합
            GUILayout.Space(5);
            GUILayout.Box("=== 무기 융합 (키 1~5) ===");
            for (int i = 0; i < 5; i++)
            {
                EquipData weapon = cachedWeapons[i];
                int count = GetInventoryCount(cachedWeapons, i);
                bool canFuse = count >= 5;
                string status = canFuse ? "가능" : $"부족 ({count}/5)";

                GUILayout.Label($"{i + 1}: {weapon.GetName()} ({weapon.GetGrade()}) - 보유: {count}개 [{status}]");
            }

            // 악세서리 융합
            GUILayout.Space(5);
            GUILayout.Box("=== 악세서리 융합 (키 Q~T) ===");
            string[] keys = { "Q", "W", "E", "R", "T" };
            for (int i = 0; i < 5; i++)
            {
                EquipData acc = cachedAccessories[i];
                int count = GetInventoryCount(cachedAccessories, i);
                bool canFuse = count >= 5;
                string status = canFuse ? "가능" : $"부족 ({count}/5)";

                GUILayout.Label($"{keys[i]}: {acc.GetName()} ({acc.GetGrade()}) - 보유: {count}개 [{status}]");
            }

            GUILayout.Space(5);
            GUILayout.Box("9: 장비 메뉴로 돌아가기");
        }
        #endregion

        // 공통 기능: 모든 장비 1개씩 추가
        private void AddAllEquipment()
        {
            if (cachedWeapons == null || cachedAccessories == null || equipmentManager == null)
            {
                Debug.LogWarning("[SimpleSkillTest] 장비 데이터 또는 매니저가 초기화되지 않았습니다.");
                return;
            }

            // 모든 무기 1개씩 추가
            foreach (var weapon in cachedWeapons)
            {
                equipmentManager.AddToInventory(weapon);
            }

            // 모든 악세서리 1개씩 추가
            foreach (var accessory in cachedAccessories)
            {
                equipmentManager.AddToInventory(accessory);
            }

            Debug.Log("[SimpleSkillTest] 모든 장비 +1씩 추가됨");
        }

        #region 장비 레벨업 메뉴
        private void HandleLevelUpMenuInput()
        {
            // 무기 레벨업 (1-6)
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryLevelUpWeapon(i);
                }
            }

            // 악세서리 레벨업 (Q-Y: 1-6)
            if (Input.GetKeyDown(KeyCode.Q)) TryLevelUpAccessory(0);
            else if (Input.GetKeyDown(KeyCode.W)) TryLevelUpAccessory(1);
            else if (Input.GetKeyDown(KeyCode.E)) TryLevelUpAccessory(2);
            else if (Input.GetKeyDown(KeyCode.R)) TryLevelUpAccessory(3);
            else if (Input.GetKeyDown(KeyCode.T)) TryLevelUpAccessory(4);
            else if (Input.GetKeyDown(KeyCode.Y)) TryLevelUpAccessory(5);

            // 메인 복귀
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                currentMenu = MenuState.Main;
                Debug.Log("[Menu] 메인 메뉴로 돌아감");
            }
        }

        private void TryLevelUpWeapon(int index)
        {
            if (cachedWeapons == null || index >= cachedWeapons.Length) return;
            EquipData weapon = cachedWeapons[index];

            int currentLevel = weapon.level;
            weapon.level++;
            Debug.Log($"[레벨업 성공] {weapon.GetName()}: Lv.{currentLevel} → Lv.{weapon.level}");
        }

        private void TryLevelUpAccessory(int index)
        {
            if (cachedAccessories == null || index >= cachedAccessories.Length) return;
            EquipData accessory = cachedAccessories[index];

            int currentLevel = accessory.level;
            accessory.level++;
            Debug.Log($"[레벨업 성공] {accessory.GetName()}: Lv.{currentLevel} → Lv.{accessory.level}");
        }

        private void DrawLevelUpMenu()
        {
            GUILayout.Box("=== 장비 레벨업 ===");
            GUILayout.Box("장비의 레벨을 직접 올립니다");

            // 무기 레벨업
            GUILayout.Space(5);
            GUILayout.Box("=== 무기 레벨업 (키 1~6) ===");
            for (int i = 0; i < 6; i++)
            {
                EquipData weapon = cachedWeapons[i];
                GUILayout.Label($"{i + 1}: {weapon.GetName()} ({weapon.GetGrade()}) - Lv.{weapon.level}");
            }

            // 악세서리 레벨업
            GUILayout.Space(5);
            GUILayout.Box("=== 악세서리 레벨업 (키 Q~Y) ===");
            string[] keys = { "Q", "W", "E", "R", "T", "Y" };
            for (int i = 0; i < 6; i++)
            {
                EquipData acc = cachedAccessories[i];
                GUILayout.Label($"{keys[i]}: {acc.GetName()} ({acc.GetGrade()}) - Lv.{acc.level}");
            }

            // 플레이어 스탯 표시
            DrawPlayerStats();

            GUILayout.Space(5);
            GUILayout.Box("9: 메인 메뉴로 돌아가기");
        }
        #endregion
    }
}
