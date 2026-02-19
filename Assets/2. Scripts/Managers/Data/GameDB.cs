using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Reflection;

// ��� ������ ����Ʈ�� ��ӹ��� �������̽�
public interface IDataList<T>
{
    List<T> GetList();
}

public class GameDB<T, TList> where TList : IDataList<T>
{
    // Dictionary<string, T>    -> string�� ID, T�� ������ Ŭ����
    // ID ��� ������ Ž���� ��ųʸ�
    private Dictionary<string, T> _dataDict = new Dictionary<string, T>();

    /// <summary>
    /// ��巹������ Json ������ �񵿱� �ε�, �����ͺ��̽� ����
    /// </summary>
    /// <param name="address"> ��巹���� �ּ� </param>
    /// <returns></returns>
    public async Task LoadAsync(string address)
    {
        // 1. ���� �񵿱� �ε�
        var handle = Addressables.LoadAssetAsync<TextAsset>(address);
        TextAsset jsonFile = await handle.Task;

        if (jsonFile == null) return;

        // [조민희 수정] Unity API는 메인 스레드에서만 호출 가능하므로 미리 text 가져오기
        // 기존: Task.Run 내부에서 jsonFile.text 직접 호출 → UnityException 발생
        string jsonText = jsonFile.text;

        // 2. ��׶��� �Ľ�
        await Task.Run(() =>
        {
            // Json ������ȭ
            TList list = JsonConvert.DeserializeObject<TList>(jsonText);

            if (list == null) return;

            // ���÷��� ����ȭ: ���� �ۿ��� �ʵ� ������ �̸� ������
            FieldInfo idFieldInfo = typeof(T).GetField("id");
            if (idFieldInfo == null)
            {
                Debug.LogError($"Ŭ������ 'id' �ʵ尡 �����ϴ�.");
                return;
            }

            // ���� ������ �ʱ�ȭ �� ��ųʸ� ����
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

        // ���� �ڵ� ����
        Addressables.Release(handle);
    }

    /// <summary>
    /// ID ��� ������ Ž��
    /// </summary>
    /// <param name="id"> ã���� �ϴ� �������� ID �� </param>
    /// <returns> �ش� ������ </returns>
    public T Get(string id)
    {
        if (_dataDict.TryGetValue(id, out T value)) return value;
        return default;
    }

    /// <summary>
    /// ������ ����Ʈ ��ü ȣ��
    /// </summary>
    /// <returns> ����� ��� ������ </returns>
    public List<T> GetAll() => new List<T>(_dataDict.Values);
}
