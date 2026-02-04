using UnityEngine;
using System;

public class StageManager : MonoBehaviour
{
    // 이벤트
    public event Action<float> OnProgressChanged; // 0.0 ~ 1.0
    public event Action<int> OnStageChanged;

    public StageData CurrentStageData { get; private set; }
    public float CurrentProgressRatio { get; private set; } // 0.0 ~ 1.0

    private int _currentStageIndex = 0;
    private int _currentKillCount = 0;
    
    public void Initialize(string stageId)
    {
        StageData stageData = DataManager.Instance.GetStage(stageId);
        if (stageData != null)
        {
            SetStage(stageData);
        }
        else
        {
            Debug.LogError($"[StageManager] 스테이지 찾을 수 없음: {stageId}");
        }
    }

    public void SetStage(StageData stageData)
    {
        CurrentStageData = stageData;
        ResetProgress();
        
        OnStageChanged?.Invoke(_currentStageIndex);
    }

    public void AddKill()
    {
        _currentKillCount++;
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        // StageData에서 TargetKillCount 제거됨, 진행률 정의되지 않거나 무한함.
        // 현재는 오류 방지를 위해 비율을 0으로 유지.
        CurrentProgressRatio = 0f; 
        OnProgressChanged?.Invoke(CurrentProgressRatio);
    }

    public void ResetProgress()
    {
        _currentKillCount = 0;
        UpdateProgress();
    }
}
