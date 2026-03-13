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
    }

    /// <summary>
    /// 데이터 호출
    /// </summary>
    /// <param name="tableName"> 데이터 테이블 명칭 </param>
    /// <returns></returns>
    public async Task<string> GetDataAsync(string tableName)
    {
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
                return null;
            }

            if (bro.IsSuccess())
            {
                var rows = bro.FlattenRows();
                if (rows.Count > 0)
                {
                    string inDate = bro.Rows()[0]["inDate"]["S"].ToString();
                    SetInDate(tableName, inDate);

                    return rows[0]["Content"].ToString();
                }

                return null;
            }

            return null;
        }
        catch (System.Exception e)
        {
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

        return string.Empty;
    }
}
