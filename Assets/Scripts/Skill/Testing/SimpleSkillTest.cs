using UnityEngine;

namespace SlayerLegend.Skill.Testing
{
    /// <summary>
    /// 메뉴 상태 열거형
    /// </summary>
    public enum MenuState
    {
        Main,           // 메인 메뉴
        GeneralSkill,   // 일반 스킬 테스트
        StatusEffect    // 상태이상 스킬 테스트
    }

    /// <summary>
    /// 간단한 스킬 테스트 - 계층형 메뉴 시스템
    /// </summary>
    public class SimpleSkillTest : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private DummyCharacter dummyCharacter;
        [SerializeField] private SkillController skillController;

        [Header("메뉴 상태")]
        [SerializeField] private MenuState currentMenu = MenuState.Main;
        [SerializeField] private bool showMenu = true;

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
            GUILayout.Box("G: 골드 +1000\nH: 체력/마나 회복\nM: 메뉴 숨기기/표시");
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
            }

            GUILayout.EndArea();
        }

        private void DrawMainMenu()
        {
            GUILayout.Box("=== 메인 테스트 메뉴 ===");
            GUILayout.Box(
                "1: 일반 스킬 테스트\n  (파이어볼, 얼음창, 버프)\n\n" +
                "2: 상태이상 스킬 테스트\n  (도트, 기절, 빙결, 속박)");
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
    }
}
