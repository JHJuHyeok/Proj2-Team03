using UnityEngine;
using BackEnd;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

//===============게임 시작 시 로딩==============// 

public class LoadingController : MonoBehaviour
{
    private async void Start()
    {
        // 1. 로그인 스텝
        bool loginSuccess = await AuthStep();
        if (!loginSuccess)
        {
            BackendLogin.Instance.GuestSignUp();
            return;
        }

        // 2. 서버 데이터 통합 로드
        bool dataLoadSuccess = await LoadGameDataStep();
        if (!dataLoadSuccess) return;

        // 4. 씬 이동
    }

    // 자동 로그인
    private async Task<bool> AuthStep()
    {
        var bro = Backend.BMember.LoginWithTheBackendToken();
        return bro.IsSuccess();
    }

    private async Task<bool> LoadGameDataStep()
    {
        // 1. 로컬 저장소의 데이터 호출
        var localCurrency = LoadCurrencyFromLocal();
        var localData = LoadDataFromLocal();

        // 2. 서버 테이블에서 데이터 호출
        var currencyResult = BackendManager.Instance.GetDataAsync("UserCurrency");
        var dataResult = BackendManager.Instance.GetDataAsync("UserSave");

        // 3. 데이터 불러올 때까지 대기
        await Task.WhenAll(currencyResult, dataResult);

        string currencyJson = await currencyResult;
        string dataJson = await dataResult;

        CurrencyData serverCurrency;
        GameData serverData;

        // 4. 서버에 데이터가 없을 경우 초기화
        if (string.IsNullOrEmpty(currencyJson))
        {
            Debug.Log("신규 재화 데이터를 생성하고 서버에 저장합니다.");
            serverCurrency = CurrencyData.CreateDefault();
            await BackendManager.Instance.SaveDataAsync("UserCurrency", serverCurrency);
        }
        else
        {
            serverCurrency = JsonConvert.DeserializeObject<CurrencyData>(currencyJson);
        }
        
        if (string.IsNullOrEmpty(dataJson))
        {
            Debug.Log("신규 게임 데이터를 생성하고 서버에 저장합니다.");
            serverData = GameData.CreateDefault();
            await BackendManager.Instance.SaveDataAsync("UserSave", serverData);
        }
        else
        {
            serverData = JsonConvert.DeserializeObject<GameData>(dataJson);
        }

        // 5. 최신 저장 데이터 비교
        CurrencyData latestCurrency = DataSyncManager.ResolveLatestCurrency(localCurrency, serverCurrency);
        GameData latestData = DataSyncManager.ResolveLatestData(localData, serverData);

        if (latestCurrency != null && latestData != null)
        {
            // 6. 각 매니저의 데이터 초기화
            CurrencyManager.Instance.Init(latestCurrency);
            DataManager.Init(latestData);

            // 7. 아틀라스 스프라이트 로드
            await ResourcesLoadStep();
            // 8. 데이터베이스 로드
            await DataManager.LoadAllDatabase();

            Debug.Log("전체 데이터 로드 완료");
            return true;
        }

        Debug.Log("데이터 로드 실패");
        return false;
    }

    private GameData LoadDataFromLocal()
    {
        string path = Application.persistentDataPath + "/temp_saveData.json";
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<GameData>(json);
    }

    private CurrencyData LoadCurrencyFromLocal()
    {
        string path = Application.persistentDataPath + "/temp_saveCurrency.json";
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<CurrencyData>(json);
    }

    private async Task ResourcesLoadStep()
    {
        await SpriteManager.LoadAllAtlasAsync();
        await Task.Delay(100);      // 로딩 체감되도록 살짝 딜레이
        Debug.Log("리소스 로드 완료");
    }
}
