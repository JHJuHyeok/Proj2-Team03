using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackEnd;
using Newtonsoft.Json;

public class BackendManager : Singleton<BackendManager>
{
    public async Task<string> GetDataAsync(string tableName)
    {
        return await Task.Run(() =>
        {
            var bro = Backend.GameData.GetMyData(tableName, new Where());
            if (bro.IsSuccess() && bro.FlattenRows().Count > 0)
            {
                return bro.FlattenRows()[0].ToJson();
            }
            return null;
        });
    }

    //public async Task<bool> UpdateDataAsync(string tableName, object dataObject)
    //{
    //    Param param = new Param();

    //    string json = JsonConvert.SerializeObject(dataObject);
    //    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

    //    foreach (var item in dictionary)
    //    {
    //        param.Add(item.Key, item.Value);
    //    }

    //    return await Task.Run(() =>
    //    {
    //        var getResult = Backend.GameData.GetMyData(tableName, new Where());

    //        BackendReturnObject bro;

    //        if (getResult.IsSuccess() && getResult.FlattenRows().Count > 0)
    //        {
    //            string inDate = getResult.FlattenRows()[0]["inDate"].ToString();
    //            bro = Backend.GameData.Update(tableName, inDate, param);
    //        }
    //    });
    //}
}
