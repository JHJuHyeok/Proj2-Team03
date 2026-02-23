using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class CurrencyData : ISavable
{
    public List<Currency> currencies;
    public long lastSaveTime { get; set; }

    // 재화 초기화
    public static CurrencyData CreateDefault()
    {
        var data = new CurrencyData
        {
            currencies = new List<Currency>(),
            lastSaveTime = DateTime.UtcNow.Ticks
        };

        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            data.currencies.Add(new Currency { type = type, value = 0 });
        }

        return data;
    }
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