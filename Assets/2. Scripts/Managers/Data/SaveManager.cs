using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using BackEnd;

public interface ISavable
{
    long lastSaveTime { get; set; }
}

public class SaveManager : Singleton<SaveManager>
{
    private bool _isDirty = true;              // 데이터 변경 여부
    private CancellationTokenSource _cts;
    private const int saveInterval = 300;       // 자동 저장 간격 5분

    private void Start()
    {
        if (_cts != null) return;

        _cts = new CancellationTokenSource();
        // 저장 루프 시작
        _ = AutoSaveLoop(_cts.Token);       // '_' 는 리턴값을 무시해도 된다는 의미 
    }

    private async Task AutoSaveLoop(CancellationToken token)
    {
        Debug.Log("자동 저장 루프 시작");
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 5분 대기
                await Task.Delay(TimeSpan.FromSeconds((double)saveInterval), token);
                SetDirty();

                // 저장 함수 호출
                if (_isDirty)
                {
                    await SaveToRemote();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                await Task.Delay(2000, token);
            }
        }
    }

    // 외부에서 원격 저장 요구 시 호출
    public void SetDirty() => _isDirty = true;

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 루프 종료
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        // 일시 정지 시 저장
        if (pause && _isDirty)
        {
            _ = SaveToRemote();
        }
    }

    private void OnApplicationQuit()
    {
        _ = SaveToRemote();
    }

    /// <summary>
    /// 서버에 현재 데이터 원격 저장
    /// </summary>
    public async Task SaveToRemote()
    {
        try
        {
            GameData saveData = DataManager.CurrentSaveData;
            CurrencyData saveCurrency = CurrencyManager.Instance.currencySave;

            // 저장 시간 기록
            PrepareForSave(saveData);
            PrepareForSave(saveCurrency);

            Task<bool> saveDataTask = SaveToBackend("UserSave", saveData);
            Task<bool> saveCurrencyTask = SaveToBackend("UserCurrency", saveCurrency);

            bool[] results = await Task.WhenAll(saveDataTask, saveCurrencyTask);

            if (results[0] && results[1])
            {
                _isDirty = false;
            }
        }
        catch (Exception e)
        {
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

    private async Task<bool> SaveToBackend<T>(string tableName, T data)
    {
        var tcs = new TaskCompletionSource<bool>();

        string inDate = BackendManager.Instance.GetInDateForTable(tableName);
        string jsonContent = JsonUtility.ToJson(data);

        Param param = new Param();
        param.Add("Content", jsonContent);

        // 뒤끝 내부의 Async 메서드와 콜백 사용
        Backend.GameData.UpdateV2(tableName, inDate, Backend.UserInDate, param, callback =>
        {
            if (callback.IsSuccess()) tcs.SetResult(true);
            else
            {
                Debug.LogError($"[Backend Error] {callback.GetStatusCode()} : {callback.GetMessage()}");
                tcs.SetResult(false);
            }
        });

        return await tcs.Task;
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
