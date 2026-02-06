using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CurrencySaveData
{
    public List<Currency> currencies;
}

[System.Serializable]
public class Currency
{
    public CurrencyType type;       // 재화 타입
    public double value;            // 값
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