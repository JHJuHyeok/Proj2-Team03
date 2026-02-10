using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public interface ISavable
{
    long lastSaveTime { get; set; }
}

public class SaveManager : Singleton<SaveManager>
{
    private bool _isDirty = false;              // 데이터 변경 여부
    private float _saveTimer = 0f;
    private const float saveInterval = 300f;    // 자동 저장 간격 5분

    private void Update()
    {
        _saveTimer += Time.deltaTime;

        // 5분 간격으로 자동 저장
        if (_saveTimer >= saveInterval)
        {
            if (_isDirty)
            {
                SaveToRemote();
            }
            _saveTimer = 0f;
        }
    }

    /// <summary>
    /// 서버에 현재 데이터 원격 저장
    /// </summary>
    public async void SaveToRemote()
    {
        GameData saveData = DataManager.Instance.currentSaveData;
        CurrencyData saveCurrency = CurrencyManager.Instance.currencySave;

        // 저장 시간 기록
        PrepareForSave(saveData);
        PrepareForSave(saveCurrency);

        Task<bool> saveResultTask = SaveToBackend("UserSave", DataManager.Instance.currentSaveData);
        Task<bool> saveCurrencyTask = SaveToBackend("UserCurrency", CurrencyManager.Instance.currencySave);

        bool[] results = await Task.WhenAll(saveResultTask, saveCurrencyTask);

        if (results[0] && results[1])
        {
            _isDirty = false;
            _saveTimer = 0f;
        }
    }

    /// <summary>
    /// 로컬 저장소에 임시 저장(변경사항 발생 시 호출)
    /// GameData, CurrencyData만 이용 권장
    /// </summary>
    public void SaveDataToLocal<T>(T data) where T : ISavable
    {
        PrepareForSave(data);

        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + $"/temp_{data.ToString()}.json", json);
    }

    private async Task<bool> SaveToBackend<T>(string tableName, T data) where T : ISavable
    {
        return await BackendManager.Instance.SaveDataAsync(tableName, data);
    }

    /// <summary>
    /// 저장 시간 기록
    /// </summary>
    public void PrepareForSave<T>(T data) where T : ISavable
    {
        if (data != null)
            data.lastSaveTime = DateTime.UtcNow.Ticks;
    }
}
