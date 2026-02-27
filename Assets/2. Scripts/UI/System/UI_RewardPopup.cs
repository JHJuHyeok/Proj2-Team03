using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_RewardPopup : MonoBehaviour
{
    [Header("오프라인 경과 시간")]
    [SerializeField] private TMP_Text _elapsedTimeText; 

    [Header("누적 보상")]
    [SerializeField] private TMP_Text _totalGoldText;
    [SerializeField] private TMP_Text _totalExpText;
    [SerializeField] private TMP_Text _totalCubeText;

    [Header("현재 스테이지 분당 획득 정보")]
    [SerializeField] private TMP_Text _goldPerMinText;
    [SerializeField] private TMP_Text _expPerMinText;

    [Header("확인 버튼")]
    [SerializeField] private Button _confirmBtn;

    private OfflineReward _currentReward;

    private void Awake()
    {
        // 확인(수령) 버튼 클릭 시 팝업 닫기
        if (_confirmBtn != null)
        {
            _confirmBtn.onClick.AddListener(ClosePopup);
        }
    }

    /// <summary>
    /// UI에 각 정보 주입 and 팝업 열기
    /// </summary>
    /// <param name="reward"> 오프라인 보상 정보 </param>
    /// <param name="stage"> 현재 스테이지 데이터 </param>
    public void SetInfo(OfflineReward reward, StageData stage)
    {
        _currentReward = reward;

        _elapsedTimeText.text = reward.elapsedMin.ToString();

        _totalGoldText.text = reward.gold.ToString("#,##0");
        _totalExpText.text = reward.exp.ToString("#,##0");
        _totalCubeText.text = reward.cube.ToString("#,##0");

        _goldPerMinText.text = $"{stage.goldPerMin}/m";
        _expPerMinText.text = $"{stage.expPerMin}/m";

        this.gameObject.SetActive(true);
    }

    private async void ClosePopup()
    {
        if (_currentReward != null)
            _currentReward.ApplyRewards();

        // 마지막 저장 시간 갱신(중복 수령 방지)
        DataManager.CurrentSaveData.lastSaveTime = DateTime.UtcNow.Ticks;
        // 데이터 원격 저장
        await BackendManager.Instance.SaveDataAsync("UserSave", DataManager.CurrentSaveData);

        this.gameObject.SetActive(false);
    }
}
