using BackEnd;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

//===============게임 시작 시 로딩==============// 

public class LoadingController : MonoBehaviour
{
    /// <summary>
    /// [승문 추가] 
    /// - 게임 시작 시 로그인/데이터/리소스/DB 로드를 담당
    /// - 로딩이 끝난 뒤 씬 이동은 LoadingSceneFlow(또는 다른 Flow)에서 처리
    /// - LoadingSceneFlow에서 "데이터 로드 완료 후, 화면 터치로 씬 이동"을 만들기 위해
    /// 로딩 상태를 외부에서 확인할 수 있는 프로퍼티 + await 가능한 RunAsync() 진입점을 추가함.
    /// </summary>

    // 외부에서 상태 확인용
    public bool IsRunning { get; private set; }     // 현재 로딩 진행 중인가?
    public bool IsDone { get; private set; }        // 로딩 프로세스가 끝났는가? (성공/실패 포함)
    public bool IsSuccess { get; private set; }     // 최종 성공 여부
    public bool HasError { get; private set; }      // 에러 발생 여부
    public string ErrorMessage { get; private set; } // 에러 상세(디버깅/토스트 등에 활용)

    // (선택) 로딩바/단계 UI가 필요하면 활용
    public float Progress { get; private set; }     // 0~1 진행률
    public string CurrentStep { get; private set; } // Auth / LoadGameData 등 단계명

    private void Awake()
    {
        ResetState();
    }

    private void ResetState()
    {
        IsRunning = false;
        IsDone = false;
        IsSuccess = false;
        HasError = false;
        ErrorMessage = string.Empty;

        Progress = 0f;
        CurrentStep = string.Empty;
    }

    // LoadingSceneFlow가 호출해서 "기다릴 수 있는" 진입점
    // - 기존 Start() 자동 실행 방식 대신, 외부 Flow에서 로딩을 제어하기 위한 함수
    // - 내부에서 AuthStep -> LoadGameDataStep 순서로 실행
    public async Task<bool> RunAsync()
    {
        if (IsRunning) return false;

        ResetState();
        IsRunning = true;

        try
        {
            // 1. 로그인 스텝
            CurrentStep = "Auth";
            Progress = 0.05f;

            bool loginSuccess = await AuthStep();
            if (!loginSuccess)
            {
                // 기존 코드 존중: 실패하면 게스트 가입 시도
                BackendLogin.Instance.GuestSignUp();

                Fail("AuthStep failed (backend token login).");
                return false;
            }

            // 2. 서버 데이터 통합 로드
            CurrentStep = "LoadGameData";
            Progress = 0.2f;

            bool dataLoadSuccess = await LoadGameDataStep();
            if (!dataLoadSuccess)
            {
                Fail("LoadGameDataStep failed.");
                return false;
            }

            Progress = 1f;
            CurrentStep = "Done";

            IsSuccess = true;
            IsDone = true;
            IsRunning = false;
            return true;
        }
        catch (System.Exception e)
        {
            Fail("Exception: " + e.Message);
            Debug.LogError(e);
            return false;
        }
    }

    private void Fail(string msg)
    {
        HasError = true;
        ErrorMessage = msg;

        IsSuccess = false;
        IsDone = true;
        IsRunning = false;

        CurrentStep = "Error";
        Progress = 0f;
    }
    /*private async void Start()
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
    }*/

    // 자동 로그인
    private async Task<bool> AuthStep()
    {
        var bro = Backend.BMember.LoginWithTheBackendToken();
        await Task.Yield(); // 혹시 모를 프레임 분리(안전)
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
            // 9. [조민희] 장비 아이콘 로드 (AssetBundleLoader 초기화)
            SlayerLegend.Resource.AssetBundleLoader.Instance.Initialize();

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
