using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackEnd;
using Newtonsoft.Json;

public class BackendManager : Singleton<BackendManager>
{
    private void Start()
    {
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
        }
        else
        {
            Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
        }
    }

    /// <summary>
    /// 데이터 호출
    /// </summary>
    /// <param name="tableName"> 데이터 테이블 명칭 </param>
    /// <returns></returns>
    public async Task<string> GetDataAsync(string tableName)
    {
        return await Task.Run(() =>
        {
            var bro = Backend.GameData.GetMyData(tableName, new Where());

            if (bro.IsSuccess() && bro.FlattenRows().Count > 0)
            {
                Debug.Log("게임 정보 조회 성공");

                return bro.FlattenRows()[0].ToJson();
            }
            return null;
        });
    }

    /// <summary>
    /// 서버에 유저 데이터 저장
    /// </summary>
    /// <param name="tableName"> 데이터 테이블 명칭 </param>
    /// <param name="dataObject"> 저장할 데이터 </param>
    /// <returns></returns>
    public async Task<bool> SaveDataAsync(string tableName, object dataObject)
    {
        Param param = new Param();

        string json = JsonConvert.SerializeObject(dataObject);
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (var item in dictionary)
        {
            param.Add(item.Key, item.Value);
        }

        return await Task.Run(() =>
        {
            // 2. 먼저 해당 테이블에 내 데이터가 있는지 확인 (업데이트를 위해 inDate가 필요함)
            var getResult = Backend.GameData.GetMyData(tableName, new Where());

            BackendReturnObject bro = null;

            if (getResult.IsSuccess() && getResult.FlattenRows().Count > 0)
            {
                // 데이터가 이미 존재함 -> 업데이트 (Update)
                string inDate = getResult.FlattenRows()[0]["inDate"].ToString();
                bro = Backend.GameData.UpdateV2(tableName, inDate, Backend.UserInDate , param);
                Debug.Log($"[{tableName}] 데이터 업데이트 시도...");
            }
            else
            {
                // 데이터가 없음 -> 신규 생성 (Insert)
                bro = Backend.GameData.Insert(tableName, param);
                Debug.Log($"[{tableName}] 신규 데이터 생성 시도...");
            }

            if (bro.IsSuccess())
            {
                Debug.Log($"[{tableName}] 서버 저장 성공");
                return true;
            }
            else
            {
                Debug.LogError($"[{tableName}] 서버 저장 실패: {bro.GetErrorCode()} - {bro.GetMessage()}");
                return false;
            }
        });
    }
}
