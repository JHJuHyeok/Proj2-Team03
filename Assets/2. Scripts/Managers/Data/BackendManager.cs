using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackEnd;
using Newtonsoft.Json;

public class BackendManager : Singleton<BackendManager>
{
    private Dictionary<string, string> _tableInDates = new Dictionary<string, string>();

    protected override void Awake()
    {
        base.Awake();
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
        Debug.Log($"{tableName} 데이터 요청 중...");

        try
        {
            // 뒤끝의 전용 비동기 핸들러를 사용하여 데드락 방지
            BackendReturnObject bro = null;
            bool isCompleted = false;

            Backend.GameData.GetMyData(tableName, new Where(), callback =>
            {
                bro = callback;
                isCompleted = true;
            });

            // 콜백 대기
            while (!isCompleted)
            {
                await Task.Yield();
            }

            if (bro == null)
            {
                Debug.LogError($"[GetDataAsync] {tableName} 응답 객체가 null입니다.");
                return null;
            }

            if (bro.IsSuccess())
            {
                var rows = bro.FlattenRows();
                if (rows.Count > 0)
                {
                    Debug.Log($"[GetDataAsync] {tableName} 조회 성공: {rows[0]["Content"].ToString()}");

                    string inDate = bro.Rows()[0]["inDate"]["S"].ToString();
                    SetInDate(tableName, inDate);

                    return rows[0]["Content"].ToString();
                }
                Debug.Log($"[GetDataAsync] {tableName} 데이터가 존재하지 않습니다.");

                return null;
            }

            Debug.LogWarning($"[GetDataAsync] {tableName} 조회 실패: {bro.GetStatusCode()} / {bro.GetErrorCode()} - {bro.GetMessage()}");
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GetDataAsync] 심각한 오류 발생: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 서버에 유저 데이터 저장
    /// </summary>
    /// <param name="tableName"> 데이터 테이블 명칭 </param>
    /// <param name="dataObject"> 저장할 데이터 </param>
    /// <returns></returns>
    public async Task SaveDataAsync(string tableName, object dataObject)
    {
        Param param = new Param();

        string json = JsonConvert.SerializeObject(dataObject);
        param.Add("Content", json);

        BackendReturnObject bro = null;
        bool isCompleted = false;

        // 뒤끝 비동기 인서트
        Backend.GameData.Insert(tableName, param, callback =>
        {
            bro = callback;
            isCompleted = true;
        });

        // 콜백이 올 때까지 대기
        while (!isCompleted)
        {
            await Task.Yield();
        }

        if (bro.IsSuccess())
        {
            Debug.Log($"{tableName} 서버 저장 성공");
        }
        else
        {
            Debug.LogError($"{tableName} 서버 저장 실패: {bro.GetStatusCode()} / {bro.GetErrorCode()}");
        }
    }

    /// <summary>
    /// 서버에서 데이터를 불러올 때의 inDate를 기록
    /// </summary>
    public void SetInDate(string tableName, string inDate)
    {
        if (_tableInDates.ContainsKey(tableName))
            _tableInDates[tableName] = inDate;
        else
            _tableInDates.Add(tableName, inDate);
    }

    /// <summary>
    /// 테이블 별 inDate 반환
    /// </summary>
    public string GetInDateForTable(string tableName)
    {
        if (_tableInDates.TryGetValue(tableName, out string inDate))
        {
            return inDate;
        }

        Debug.LogError($"[Backend] {tableName}의 inDate를 찾을 수 없습니다. 먼저 데이터를 불러와야 합니다.");
        return string.Empty;
    }
}
