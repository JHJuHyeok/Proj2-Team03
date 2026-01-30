using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;

public class MonsterJsonCreator : EditorWindow
{
    List<MonsterData> monsters = new List<MonsterData>();
    private UnityEngine.Vector2 scrollPos;

    // 에디터 창 표시
    [MenuItem("Tools/JSON/Monster JSON Creator")]
    public static void ShowWindow()
    {
        GetWindow<MonsterJsonCreator>("MonsterDatabase Creator");
    }

    // 스크롤 변수
    [SerializeField] bool boolBar = true;

    private void OnGUI()
    {
        GUILayout.Label("몬스터 JSON 데이터 생성기");

        // 상단 버튼들
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("몬스터 추가"))
        {
            monsters.Add(new MonsterData());
        }
        if (GUILayout.Button("JSON 파일 생성"))
        {
            ExportToJson();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterItem(i);
        }
        EditorGUILayout.EndScrollView();
    }

    private void MonsterItem(int index)
    {
        MonsterData monster = monsters[index];
        EditorGUILayout.BeginVertical("box");

        monster.id = EditorGUILayout.TextField("몬스터 ID", monster.id);
        monster.name = EditorGUILayout.TextField("몬스터 명칭", monster.name);
        monster.spriteName = EditorGUILayout.TextField("이미지 이름", monster.spriteName);

        // BigInteger 입력 필드
        monster.maxHp = ConvertBigIntField("최대 체력", monster.maxHp);
        monster.Attack = ConvertBigIntField("공격력", monster.Attack);

        monster.type = (MonsterType)EditorGUILayout.EnumPopup("몬스터 타입", monster.type);
        monster.weakElement = (SkillElement)EditorGUILayout.EnumPopup("약한 속성", monster.weakElement);

        if (GUILayout.Button("이 필드 삭제", GUILayout.Width(100)))
        {
            monsters.RemoveAt(index);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    /// BigInteger 값을 string으로 입력 받기
    private BigInteger ConvertBigIntField(string label, BigInteger value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);

        // BigInteger 값을 string으로 변환
        string currentStringValue = value.ToString();
        string input = EditorGUILayout.TextField(currentStringValue);

        BigInteger resultValue = value;

        // 유효성 검증
        if (BigInteger.TryParse(input, out BigInteger parsedValue))
        {
            resultValue = parsedValue;
        }
        else
        {
            GUI.color = Color.red;
            GUILayout.Label("유효하지 않은 값입니다.", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        EditorGUILayout.EndHorizontal();

        return resultValue;
    }

    private void ExportToJson()
    {
        MonsterDataList dataList = new MonsterDataList
        {
            monsterList = monsters
        };

        // 저장될 주소
        string folder = "Assets/Resources/Json/Monster";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        try
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            // Json 파일로 변환
            string json = JsonConvert.SerializeObject(dataList, settings);

            string path = Path.Combine(folder, "MonsterList.json");

            File.WriteAllText(path, json);

            AssetDatabase.Refresh();
            Debug.Log("몬스터 리스트 생성 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"오류 발생: {e.Message}");
        }
    }
}
