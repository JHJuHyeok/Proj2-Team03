using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class CurrencyManager : Singleton<CurrencyManager>
{
    // 실시간 조회용 딕셔너리
    private Dictionary<CurrencyType, double> _currencies = new Dictionary<CurrencyType, double>();
    // 저장용 데이터
    public CurrencyData currencySave;

    public void Init(CurrencyData data)
    {
        // 1. 저장용 임시 프로퍼티 초기화
        currencySave = data;
        _currencies.Clear();

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
        if (amount <= 0) return;

        UpdateValue(type, amount);
        InfoHandler.UpdateUserData();
    }

    /// <summary>
    /// 재화 소모
    /// </summary>
    /// <param name="type"> 재화 타입 </param>
    /// <param name="amount"> 소모 재화량 </param>
    public void ConsumeCurrency(CurrencyType type, double amount)
    {
        if (amount <= 0) return;
        if (!HasEnoughCurrency(type, amount)) return;

        UpdateValue(type, -amount);
        InfoHandler.UpdateUserData();
    }

    /// <summary>
    /// 특정 재화 수량 반환
    /// </summary>
    /// <param name="type"> 재화 타입 </param>
    /// <returns> 재화 수량 </returns>
    public double GetAmount(CurrencyType type) =>
        _currencies.GetValueOrDefault(type, 0);

    /// <summary>
    /// 딕셔너리, 세이브용 데이터 동시 업데이트
    /// </summary>
    /// <param name="type"> 재화 타입 </param>
    /// <param name="offset"> 변경값 </param>
    private void UpdateValue(CurrencyType type, double offset)
    {
        // 1. 딕셔너리 업데이트
        if (!_currencies.ContainsKey(type)) _currencies[type] = 0;
        _currencies[type] += offset;

        // 2. 세이브 객체 업데이트
        var target = currencySave.currencies.Find(c => c.type == type);
        if (target != null)
            target.value = _currencies[type];
        else
            currencySave.currencies.Add(new Currency { type = type, value = _currencies[type] });

        // 3. 로컬 저장
        SaveManager.Instance.SaveDataToLocal(currencySave);
    }
}

