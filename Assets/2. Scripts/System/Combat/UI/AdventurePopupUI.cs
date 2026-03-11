using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// AdventurePopup 프리팹에 부착하는 팝업 UI 클래스
/// 퀘스트 정보, 보스 정보, 지역 정보, 보상 정보, 퀘스트 슬롯를 관리
/// AdventureUIManager가 이 클래스의 메서드를 호출하여 데이터를 전달함
/// </summary>
public class AdventurePopupUI : MonoBehaviour
{
    [Header("닫기")]
    [SerializeField] private Button closeButton;



    [Header("보스 정보 (Info Panel > Image Panel)")]
    [SerializeField] private Image monsterImage;            // Slot > Mosnter Image
    [SerializeField] private TMP_Text bossNameText;         // Slot > Boss Text

    [Header("지역 정보 (Info Panel > Area Panel)")]
    [SerializeField] private Image areaImage;               // Area Image
    [SerializeField] private Image elementalIcon;           // Area Image > Elemental
    [SerializeField] private TMP_Text areaNameText;         // Area Image > AreaName

    [Header("보상 정보 (Info Panel > Reward Panel)")]
    [SerializeField] private Image reward1Icon;             // GameObject > Image
    [SerializeField] private TMP_Text reward1Text;          // GameObject > Text (TMP)
    [SerializeField] private Image reward2Icon;             // GameObject (1) > Image
    [SerializeField] private TMP_Text reward2Text;          // GameObject (1) > Text (TMP)

    [Header("퀘스트 슬롯 (List Panel 1 + 2의 슬롯들, 총 10개)")]
    [SerializeField] private AdventureSlotUI[] questSlots;

    [Header("하단 (Bottom Text)")]
    [SerializeField] private TMP_Text entryCurrencyText;    // Text (TMP) (1)

    [Header("시작")]
    [SerializeField] private Button startButton;

    private AreaData currentAreaData;
    private StageData selectedStageData;
    private int selectedQuestIndex = -1;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (startButton != null)
            startButton.onClick.AddListener(StartAdventure);

        // 퀘스트 슬롯 클릭 이벤트 연결
        if (questSlots != null)
        {
            for (int i = 0; i < questSlots.Length; i++)
            {
                int index = i;
                if (questSlots[i] != null)
                    questSlots[i].SetOnClickAction(() => SelectQuest(index));
            }
        }
    }

    // ═══════════════════════════════════════
    //  팝업 열기 / 닫기
    // ═══════════════════════════════════════

    /// <summary>
    /// 팝업 열기 — AdventureUIManager에서 호출
    /// </summary>
    public void Open(AreaData areaData)
    {
        Debug.Log($"[AdventurePopupUI] Open 호출됨: areaData={areaData?.name}");
        if (areaData == null) return;
        
        currentAreaData = areaData;
        selectedQuestIndex = -1;
        selectedStageData = null;

        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(true);

        AdventureSaveManager.Load();
        UpdateAreaInfo();
        ClearQuestInfo();
        UpdateQuestSlots();
        AutoSelectNextQuest();
        UpdateEntryCurrency();
    }

    public void Close()
    {
        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(false);
    }

    // ═══════════════════════════════════════
    //  팝업 UI 갱신
    // ═══════════════════════════════════════

    private async void UpdateAreaInfo()
    {
        if (currentAreaData == null) return;

        if (areaNameText != null)
            areaNameText.text = currentAreaData.name;

        if (areaImage != null && !string.IsNullOrEmpty(currentAreaData.spriteName))
        {
            Sprite sprite = await SpriteManager.GetSprite(
                SpriteManager.AtlasBase + "Atlas_Map.spriteatlasv2", currentAreaData.spriteName);
            if (sprite != null) areaImage.sprite = sprite;
        }

        if (elementalIcon != null && !string.IsNullOrEmpty(currentAreaData.spriteName))
        {
            Sprite icon = await SpriteManager.GetSprite(
                SpriteManager.AtlasBase + "Atlas_UI.spriteatlasv2", currentAreaData.spriteName);
            if (icon != null) elementalIcon.sprite = icon;
        }
    }

    private void UpdateQuestSlots()
    {
        if (currentAreaData == null || currentAreaData.stageList == null) return;
        if (questSlots == null) return;

        int firstUncleared = currentAreaData.stageList.Count;
        for (int i = 0; i < currentAreaData.stageList.Count; i++)
        {
            if (!AdventureSaveManager.IsCleared(currentAreaData.stageList[i].id))
            {
                firstUncleared = i;
                break;
            }
        }

        for (int i = 0; i < questSlots.Length; i++)
        {
            if (questSlots[i] == null) continue;

            if (i < currentAreaData.stageList.Count)
            {
                questSlots[i].gameObject.SetActive(true);
                questSlots[i].SetData(currentAreaData.stageList[i], i);
                questSlots[i].SetSelected(false);

                AdventureSlotState state;
                if (AdventureSaveManager.IsCleared(currentAreaData.stageList[i].id))
                    state = AdventureSlotState.Cleared;
                else if (i == firstUncleared)
                    state = AdventureSlotState.Available;
                else
                    state = AdventureSlotState.Locked;

                questSlots[i].SetSlotState(state);
            }
            else
            {
                questSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // ═══════════════════════════════════════
    //  퀘스트 선택
    // ═══════════════════════════════════════

    private void AutoSelectNextQuest()
    {
        if (currentAreaData == null || currentAreaData.stageList == null) return;

        for (int i = 0; i < currentAreaData.stageList.Count; i++)
        {
            if (!AdventureSaveManager.IsCleared(currentAreaData.stageList[i].id))
            {
                SelectQuest(i);
                return;
            }
        }
    }

    private bool IsSlotAvailable(int index)
    {
        if (AdventureSaveManager.IsCleared(currentAreaData.stageList[index].id)) return false;
        for (int i = 0; i < index; i++)
            if (!AdventureSaveManager.IsCleared(currentAreaData.stageList[i].id)) return false;
        return true;
    }

    public void SelectQuest(int index)
    {
        if (currentAreaData == null || currentAreaData.stageList == null) return;
        if (index < 0 || index >= currentAreaData.stageList.Count) return;

        if (!IsSlotAvailable(index)) return;

        // 이전 선택 해제
        if (selectedQuestIndex >= 0 && selectedQuestIndex < questSlots.Length && questSlots[selectedQuestIndex] != null)
            questSlots[selectedQuestIndex].SetSelected(false);

        selectedQuestIndex = index;
        selectedStageData = currentAreaData.stageList[index];

        // 새 선택 표시
        if (questSlots != null && index < questSlots.Length && questSlots[index] != null)
            questSlots[index].SetSelected(true);

        // 퀘스트 정보 갱신
        UpdateQuestInfo();
    }

    // ═══════════════════════════════════════
    //  퀘스트 정보 표시
    // ═══════════════════════════════════════

    private void UpdateQuestInfo()
    {
        if (selectedStageData == null) return;

        UpdateBossInfo();
    }

    private void ClearQuestInfo()
    {
        if (bossNameText != null)
            bossNameText.text = "";
    }

    private async void UpdateBossInfo()
    {
        if (selectedStageData == null || string.IsNullOrEmpty(selectedStageData.bossId)) return;

        MonsterData bossData = DataManager.monsters.Get(selectedStageData.bossId);
        if (bossData == null) return;

        if (bossNameText != null)
            bossNameText.text = $"{bossData.name} 처치";

        if (monsterImage != null && !string.IsNullOrEmpty(bossData.spriteName))
        {
            Sprite sprite = await SpriteManager.GetSprite(
                SpriteManager.AtlasBase + "Atlas_Monster.spriteatlasv2", bossData.spriteName);
            if (sprite != null) monsterImage.sprite = sprite;
        }
    }

    // ═══════════════════════════════════════
    //  입장 재화 / 시작
    // ═══════════════════════════════════════

    private void UpdateEntryCurrency()
    {
        if (entryCurrencyText != null)
        {
            int featherCount = (int)CurrencyManager.Instance.GetAmount(CurrencyType.Feather);
            entryCurrencyText.text = $"{featherCount}/1";
        }
    }

    private void StartAdventure()
    {
        if (selectedStageData == null)
        {
            Debug.LogWarning("[AdventurePopupUI] 선택된 퀘스트가 없습니다.");
            return;
        }

        Debug.Log($"[AdventurePopupUI] 모험 시작: {selectedStageData.id}");
        CombatManager.Instance.StartAdventure(selectedStageData);
        Close();
    }
}
