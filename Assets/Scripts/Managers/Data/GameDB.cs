using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Reflection;

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

    /// <summary>
    /// 어드레서블로 Json 파일을 비동기 로드, 데이터베이스 구축
    /// </summary>
    /// <param name="address"> 어드레서블 주소 </param>
    /// <returns></returns>
    public async Task LoadAsync(string address)
    {
        // 1. 에셋 비동기 로드
        var handle = Addressables.LoadAssetAsync<TextAsset>(address);
        TextAsset jsonFile = await handle.Task;

        if (jsonFile == null) return;

        // 2. 백그라운드 파싱
        await Task.Run(() =>
        {
            // Json 역직렬화
            TList list = JsonConvert.DeserializeObject<TList>(jsonFile.text);

            if (list == null) return;

            // 리플렉션 최적화: 루프 밖에서 필드 정보를 미리 가져옴
            FieldInfo idFieldInfo = typeof(T).GetField("id");
            if (idFieldInfo == null)
            {
                Debug.LogError($"클래스에 'id' 필드가 없습니다.");
                return;
            }

            // 기존 데이터 초기화 후 딕셔너리 구성
            _dataDict.Clear();
            foreach (var item in list.GetList())
            {
                var idValue = idFieldInfo.GetValue(item)?.ToString();
                if (!string.IsNullOrEmpty(idValue))
                {
                    _dataDict[idValue] = item;
                }
            }
        });

        // 에셋 핸들 해제
        Addressables.Release(handle);
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
