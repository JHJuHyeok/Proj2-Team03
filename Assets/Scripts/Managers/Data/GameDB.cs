using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

// 모든 데이터 리스트가 상속받을 인터페이스
public interface IDataList<T>
{
    List<T> GetList();
}

public class GameDB<T, TList> where TList : IDataList<T>
{
    // Dictionary<string, T>    -> string은 ID, T는 데이터 클래스
    // ID 기반 데이터 탐색용 딕셔너리
    private Dictionary<string, T> _dataDict = new Dictionary<string, T>();

    public void Load(string path)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile == null)
        {
            Debug.Log($"{path} 경로에 파일이 존재하지 않습니다.");
            return;
        }

        // 데이터 리스트 클래스로 역직렬화
        TList list = JsonConvert.DeserializeObject<TList>(jsonFile.text);

        // 불러온 리스트를 딕셔너리로 변환
        foreach (var item in list.GetList())
        {
            var idField = typeof(T).GetField("id").GetValue(item).ToString();
            _dataDict[idField] = item;
        }
    }

    /// <summary>
    /// ID 기반 데이터 탐색
    /// </summary>
    /// <param name="id"> 찾고자 하는 데이터의 ID 값 </param>
    /// <returns> 해당 데이터 </returns>
    public T Get(string id)
    {
        if (_dataDict.TryGetValue(id, out T value)) return value;
        return default;
    }

    /// <summary>
    /// 데이터 리스트 전체 호출
    /// </summary>
    /// <returns> 저장된 모든 데이터 </returns>
    public List<T> GetAll() => new List<T>(_dataDict.Values);
}
