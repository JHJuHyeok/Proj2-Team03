using UnityEngine;

public class DataSyncManager
{
    public static GameData ResolveLatestData(GameData localData, GameData serverData)
    {
        // 두 데이터가 전부 존재하지 않을 경우 : 새 데이터 반환
        if (localData == null && serverData == null) return GameData.CreateDefault();
        // 하나의 데이터가 없을 경우 : 다른 쪽 데이터 반환
        if (localData == null) return serverData;
        if (serverData == null) return localData;

        // 더 나중에 저장된 데이터 반환
        if (localData.lastSaveTime > serverData.lastSaveTime) return localData;
        else if (localData.lastSaveTime < serverData.lastSaveTime) return serverData;

        // 저장 시간이 같은 경우 : 서버 데이터 우선 반환
        return serverData;
    }

    public static CurrencyData ResolveLatestCurrency(CurrencyData localCurrency, CurrencyData serverCurrency)
    {
        // 두 재화정보가 전부 존재하지 않을 경우 : 새 데이터 반환
        if (localCurrency == null && serverCurrency == null) return CurrencyData.CreateDefault();
        // 하나의 재화정보가 없을 경우 : 다른 쪽 데이터 반환
        if (localCurrency == null) return serverCurrency;
        if (serverCurrency == null) return localCurrency;

        // 더 나중에 저장된 재화 반환
        if (localCurrency.lastSaveTime > serverCurrency.lastSaveTime) return localCurrency;
        else if (localCurrency.lastSaveTime < serverCurrency.lastSaveTime) return serverCurrency;

        // 저장 시간이 같은 경우 : 서버 데이터 우선 반환
        return serverCurrency;
    }
}
