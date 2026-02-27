using UnityEngine;
using System;

public class OfflineReward
{
    public int elapsedMin;
    public long gold;
    public long exp;
    public int cube;

    public void ApplyRewards()
    {
        if (gold > 0)
            CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, gold);
        if (exp > 0)
            LevelManager.Instance.AddExp(exp);
        if (cube > 0)
            CurrencyManager.Instance.AddCurrency(CurrencyType.Cube, cube);

        Debug.Log("자동사냥 보상 지급 완료");
    }
}

public static class OfflineRewardManager
{
    private const int MAX_MIN = 600;    // 최대 600분

    /// <summary>
    /// 오프라인 보상 계산
    /// </summary>
    /// <param name="lastSaveTime"> 유저 데이터의 마지막 저장 시각 </param>
    /// <param name="stage"> 현재 스테이지 데이터 </param>
    /// <returns></returns>
    public static OfflineReward CalcReward(long lastSaveTime, StageData stage)
    {
        if (lastSaveTime <= 0) return null;

        // 1. 시간 차이 계산
        DateTime lastTime = new DateTime(lastSaveTime);
        TimeSpan span = DateTime.UtcNow - lastTime;
        int min = Mathf.Min((int)span.TotalMinutes, MAX_MIN);

        if (min < 1) return null;

        // 2. 보상 객체 생성 및 계산
        OfflineReward reward = new();
        reward.elapsedMin = min;

        reward.gold = stage.goldPerMin * min;
        reward.exp = stage.expPerMin * min;

        float killsPerMin = 10f;
        reward.cube = Mathf.FloorToInt(stage.cubeRate * stage.cubeCount * killsPerMin);

        return reward;
    }
}
