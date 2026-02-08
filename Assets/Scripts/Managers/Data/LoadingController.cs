using UnityEngine;
using BackEnd;
using Newtonsoft.Json;
using System.Threading.Tasks;

//===============게임 시작 시 로딩==============// 

public class LoadingController : MonoBehaviour
{
    private async void Start()
    {
        // 1. 로그인 스텝
        bool loginSuccess = await AuthStep();
        if (!loginSuccess) return;

        // 2. 서버 데이터 통합 로드
        bool dataLoadSuccess = await LoadGameDataStep();
        if (!dataLoadSuccess) return;

        // 3. 리소스 로드
        await ResourcesLoadStep();

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
        // 1. 각 테이블에서 데이터 호출
        var currencyTask = BackendManager.Instance.GetDataAsync("UserCurrency");
        var saveTask = BackendManager.Instance.GetDataAsync("UserSave");

        // 2. 데이터 불러올 때까지 대기
        await Task.WhenAll(currencyTask, saveTask);

        // 3. 각 매니저 초기화
        if (currencyTask.Result != null && saveTask.Result != null)
        {
            CurrencyManager.Instance.Init(currencyTask.Result);
            DataManager.Instance.Init(saveTask.Result);
            return true;
        }

        Debug.Log("데이터 로드 실패");
        return false;
    }

    private async Task ResourcesLoadStep()
    {
        await SpriteManager.Instance.LoadAllAtlasAsync();
        await Task.Delay(100);      // 로딩 체감되도록 살짝 딜레이
    }
}
