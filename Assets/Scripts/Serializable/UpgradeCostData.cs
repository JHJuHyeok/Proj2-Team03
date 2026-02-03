using UnityEngine;
using System.Numerics;
using System.Collections.Generic;

[System.Serializable]
public class UpgradeCostList
{
    public List<UpgradeCostData> upgradeCostList;
}

[System.Serializable]
public class UpgradeCostData
{
    public StatType type;           // 강화될 스탯 타입
    public BigInteger baseCost;     // 기본값
    public float growth;            // 성장치
}
