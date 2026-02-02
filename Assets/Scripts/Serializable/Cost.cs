using UnityEngine;
using System.Numerics;

[System.Serializable]
public class Cost
{
    public CurrencyType type;       // 재화 타입
    public BigInteger value;        // 값
}

public enum CurrencyType
{
    Gold,
    StatPoint,
    Emerald,
    Diamond,
    Cube,
    Elemental_Fire,
    Elemental_Water,
    Elemental_Wind,
    Elemental_Earth,
    Feather
}