using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private Dictionary<CurrencyType, double> _currencies = new Dictionary<CurrencyType, double>();

    public event Action<CurrencyType, double> OnCurrencyChanged;

    public CurrencyData currencySave;

    public void Init(CurrencyData data)
    {
        // 1. 저장용 임시 프로퍼티 초기화
        currencySave = data;

        // 2. 딕셔너리에 삽입
        foreach (var currency in data.currencies)
        {
            _currencies[currency.type] = currency.value;
        }
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

    /// <summary>
    /// 로컬 저장소에 재화 데이터 임시 저장(재화 변경 시 호출)
    /// </summary>
    public void SaveCurrencyToLocal()
    {
        List<CurrencyType> types = _currencies.Keys.ToList();
        List<double> values = _currencies.Values.ToList();

        for (int i = 0; i < _currencies.Count; i++)
        {
            currencySave.currencies[i].type = types[i];
            currencySave.currencies[i].value = values[i];
        }

        string json = JsonConvert.SerializeObject(currencySave);
        File.WriteAllText(Application.persistentDataPath + "/temp_currency.json", json);
    }
}

