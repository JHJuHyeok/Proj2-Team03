using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CurrencyData
{
    public List<Currency> currencies;
    public long lastSaveTime;
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