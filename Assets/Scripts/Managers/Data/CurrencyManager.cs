using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private Dictionary<CurrencyType, double> _currencies = new Dictionary<CurrencyType, double>();

    public event Action<CurrencyType, double> OnCurrencyChanged;

    public void Init(string json)
    {
        var currencyDict = JsonConvert.DeserializeObject<GameData>(json);
        Debug.Log("재화 데이터 동기화 완료");
    }

    /// <summary>
    /// 재화 충분한지 확인
    /// </summary>
    /// <param name="type"> 소모하려는 재화 </param>
    /// <param name="amount"> 재화량 </param>
    /// <returns> type이 일치하지 않으면 false, 있다면 재화량 비교 </returns>
    public bool HasEnoughCurrency(CurrencyType type, double amount)
    {
        if (!_currencies.ContainsKey(type)) return false;
        return _currencies[type] >= amount;
    }

    /// <summary>
    /// 재화 획득
    /// </summary>
    /// <param name="type"> 재화 타입 </param>
    /// <param name="amount"> 획득한 재화량 </param>
    public void AddCurrency(CurrencyType type, double amount)
    {
        if (!_currencies.ContainsKey(type)) return;

        _currencies[type] += amount;

        OnCurrencyChanged?.Invoke(type, _currencies[type]);
    }

    /// <summary>
    /// 재화 소모
    /// </summary>
    /// <param name="type"> 재화 타입 </param>
    /// <param name="amount"> 소모 재화량 </param>
    public void ConsumeCurrency(CurrencyType type, double amount)
    {
        if (HasEnoughCurrency(type, amount))
        {
            _currencies[type] -= amount;
            OnCurrencyChanged?.Invoke(type, _currencies[type]);
        }
    }

    /// <summary>
    /// 특정 재화 수량 반환
    /// </summary>
    /// <param name="type"> 재화 타입 </param>
    /// <returns> 재화 수량 </returns>
    public double GetAmount(CurrencyType type) =>
        _currencies.GetValueOrDefault(type, 0);
}

