using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ISavable
{
    long lastSaveTime { get; set; }
}

public class SaveManager : Singleton<SaveManager>
{
    private bool _isDirty = false;              // 데이터 변경 여부
    private CancellationTokenSource _cts;
    private float _saveTimer = 0f;
    private const int saveInterval = 300;       // 자동 저장 간격 5분

    private void Start()
    {
        _cts = new CancellationTokenSource();

        // 저장 루프 시작
        _ = AutoSaveLoop(_cts.Token);       // '_' 는 리턴값을 무시해도 된다는 의미 
    }

    private async Task AutoSaveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 5분 대기
                await Task.Delay(TimeSpan.FromSeconds((double)saveInterval), token);

                // 저장 함수 호출
                if (_isDirty)
                {
                    Debug.Log("자동 저장 조건을 만족하여 서버 저장을 시작합니다.");
                    await SaveToRemote();
                }
            }
            catch (OperationCanceledException)
            {
                // 게임 종료 등으로 인해 작업 취소
                break;
            }
        }
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 루프 종료
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    /// <summary>
    /// 서버에 현재 데이터 원격 저장
    /// </summary>
    public async Task SaveToRemote()
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
