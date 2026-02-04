using UnityEngine;
using System.Numerics;
using System.Collections.Generic;

[System.Serializable]
public class UpgradeConfigList
{
    public List<UpgradeConfigData> upgradeConfigs;
}

[System.Serializable]
public class UpgradeConfigData
{
    public StatType type;           // 스탯 타입
    public int increaseValue;       // 레벨당 증가량
    public int startCost;           // 초기 비용
    public float costIncreseRatio;  // 비용 성장치
}
