using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// [주혁] - DataManager 정적 클래스 전환에 의해 코드 수정(41, 96, 113, 123, 213)

// 지역 선택 팝업 UI를 관리하는 매니저
public class AreaUIManager : MonoBehaviour, IPointerDownHandler
{
    [Header("UI 참조")]
    [SerializeField] private RectTransform popupContent;        // 팝업 내부 컨텐츠 영역 (이 밖을 클릭하면 닫힘)

    [Header("지역 정보")]
    [SerializeField] private TMP_Text areaNameText;             // 지역명 텍스트
    [SerializeField] private Image areaBackgroundImage;         // 지역 배경 이미지

    [Header("팝업")]
    [SerializeField] private GameObject areaPopup;              // 팝업 오브젝트 (활성/비활성 대상)
    [SerializeField] private StageDetailPopupUI stageDetailPopup; // 스테이지 상세 팝업

    [Header("배경")]
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer; // 배경 스프라이트 렌더러

    [Header("스테이지 슬롯")]
    [SerializeField] private StageSlotUI stageSlotPrefab;       // 스테이지 슬롯 프리팹
    [SerializeField] private Transform stageSlotContainer;      // 슬롯을 배치할 부모 컨테이너

    private List<StageSlotUI> spawnedSlots = new List<StageSlotUI>();
    private AreaData currentAreaData;
    private int currentAreaIndex = 0;

    private void Start()
    {
        LoadAreaData();
    }

    // 지역 데이터 로드
    private void LoadAreaData()
    {var areaList = DataManager.stages.GetAll();
        if (areaList == null || areaList.Count == 0)
        {
            Debug.LogWarning("[AreaUIManager] 지역 데이터가 없습니다.");
            return;
        }

        // 첫 번째 지역으로 초기화
        SetArea(0);
    }

    // 팝업 열기
    public void OpenPopup()
    {
        if (areaPopup != null)
        {
            Debug.Log("[AreaUIManager] areaPopup이 연결되었습니다.");
            areaPopup.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[AreaUIManager] areaPopup이 연결되지 않았습니다.");
        }

        LoadAreaData();
    }

    // 팝업 닫기
    public void ClosePopup()
    {
        if (areaPopup != null)
        {
            areaPopup.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 영역 밖 클릭 시 닫기 (IPointerDownHandler 구현)
    public void OnPointerDown(PointerEventData eventData)
    {
        if (popupContent == null) return;

        // 클릭된 위치가 컨텐츠 영역 내부인지 확인
        if (!RectTransformUtility.RectangleContainsScreenPoint(popupContent, eventData.position, eventData.pressEventCamera))
        {
            ClosePopup();
        }
    }

    // 지역 설정
    public void SetArea(int areaIndex)
    {
        var areaList = DataManager.stages.GetAll();
        if (areaList == null || areaIndex < 0 || areaIndex >= areaList.Count)
        {
            Debug.LogWarning($"[AreaUIManager] 유효하지 않은 지역 인덱스: {areaIndex}");
            return;
        }

        currentAreaIndex = areaIndex;
        currentAreaData = areaList[areaIndex];

        UpdateAreaUI();
        UpdateStageSlots();
    }

    // 다음 지역으로 전환
    public void NextArea()
    {
        var areaList = DataManager.stages.GetAll();
        if (areaList == null || areaList.Count == 0) return;

        int nextIndex = (currentAreaIndex + 1) % areaList.Count;
        SetArea(nextIndex);
    }

    // 이전 지역으로 전환
    public void PreviousArea()
    {
        var areaList = DataManager.stages.GetAll();
        if (areaList == null || areaList.Count == 0) return;

        int prevIndex = (currentAreaIndex - 1 + areaList.Count) % areaList.Count;
        SetArea(prevIndex);
    }

    // 지역 UI 업데이트
    private async void UpdateAreaUI()
    {
        if (currentAreaData == null) return;

        // 지역명 설정
        if (areaNameText != null)
            areaNameText.text = currentAreaData.name;

        // 배경 이미지 설정
        if (areaBackgroundImage != null && !string.IsNullOrEmpty(currentAreaData.spriteName))
        {
            Sprite sprite = await SpriteManager.GetSprite("Assets/Atlas/Atlas_Map.spriteatlasv2", currentAreaData.spriteName);
            if (sprite != null) areaBackgroundImage.sprite = sprite;
        }
    }

    // 스테이지 슬롯 업데이트
    private void UpdateStageSlots()
    {
        if (currentAreaData == null || currentAreaData.stageList == null) return;

        // 현재 진행 중인 스테이지 ID 가져오기
        string currentStageId = CombatManager.Instance?.StageManager?.CurrentStageData?.id;

        int requiredSlotCount = currentAreaData.stageList.Count;

        // 부족한 슬롯 생성
        while (spawnedSlots.Count < requiredSlotCount)
        {
            StageSlotUI newSlot = Instantiate(stageSlotPrefab, stageSlotContainer);
            spawnedSlots.Add(newSlot);
        }

        // 필요한 슬롯만 활성화하고 데이터 갱신
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (i < requiredSlotCount)
            {
                var slot = spawnedSlots[i];
                var stageData = currentAreaData.stageList[i];

                slot.gameObject.SetActive(true);
                slot.SetStageData(stageData);

                // 클릭 이벤트 연결 (상세 팝업 열기)
                slot.SetOnClickAction(() => OpenStageDetail(stageData));

                // 이동 버튼 이벤트 연결 (스테이지 이동)
                slot.SetMoveAction(() => MoveToStage(stageData));

                slot.SetSelected(stageData.id == currentStageId);
            }
            else
            {
                // 남는 슬롯은 비활성화
                spawnedSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // 스테이지 상세 팝업 열기
    public void OpenStageDetail(StageData stageData)
    {
        if (stageDetailPopup != null)
        {
            stageDetailPopup.Open(stageData);
        }
        else
        {
            Debug.LogWarning("[AreaUIManager] StageDetailPopupUI가 연결되지 않았습니다.");
        }
    }

    // 스테이지 이동 처리
    private async void MoveToStage(StageData stageData)
    {
        if (stageData == null) return;

        // 스테이지 전환
        CombatManager.Instance.MoveToStage(stageData);

        // 해당 스테이지의 지역 데이터로 배경 스프라이트 변경
        AreaData areaData = DataManager.GetAreaByStageId(stageData.id);
        if (areaData != null && backgroundSpriteRenderer != null && !string.IsNullOrEmpty(areaData.spriteName))
        {
            Sprite sprite = await SpriteManager.GetSprite("Assets/Atlas/Atlas_Map.spriteatlasv2", areaData.spriteName);
            if (sprite != null)
                backgroundSpriteRenderer.sprite = sprite;
        }

        // 팝업 닫기
        ClosePopup();
    }

    // 생성된 슬롯 정리
    private void ClearSlots()
    {
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        spawnedSlots.Clear();
    }

    // 현재 지역 데이터 반환
    public AreaData GetCurrentAreaData()
    {
        return currentAreaData;
    }

    // 현재 지역 인덱스 반환
    public int GetCurrentAreaIndex()
    {
        return currentAreaIndex;
    }
}
