using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

public class StageJsonCreator : EditorWindow
{
    private List<AreaData> areaDatas = new List<AreaData>();
    private Vector2 scrollPos;

    private Dictionary<string, bool> areaFoldouts = new Dictionary<string, bool>();

    [MenuItem("Tools/JSON/Stage Json Creator")]
    public static void ShowWindow() => GetWindow<StageJsonCreator>("StageDatabase Creator");

    private void OnGUI()
    {
        GUILayout.Label("스테이지 JSON 데이터 생성기", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("구역 추가", EditorStyles.toolbarButton))
            AddNewArea();
        if (GUILayout.Button("Json 파일 생성", EditorStyles.toolbarButton))
            ExportToJson();
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < areaDatas.Count; i++)
        {
            DrawAreaItem(areaDatas[i], i);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawAreaItem(AreaData area, int index)
    {
        if (!areaFoldouts.ContainsKey(area.id)) areaFoldouts[area.id] = true;

        EditorGUILayout.BeginVertical("helpbox");

        EditorGUILayout.BeginHorizontal();

        areaFoldouts[area.id] = EditorGUILayout.Foldout(areaFoldouts[area.id],
            $"Area: {area.id}", true, EditorStyles.foldoutHeader);

        if (GUILayout.Button("Remove Area", GUILayout.Width(100))) 
        { 
            areaDatas.RemoveAt(index); 
            return; 
        }
        EditorGUILayout.EndHorizontal();

        if (areaFoldouts[area.id])
        {
            EditorGUI.indentLevel++;
            area.id = EditorGUILayout.TextField("지역 ID", area.id);
            area.name = EditorGUILayout.TextField("지역 명칭", area.name);
            area.spriteName = EditorGUILayout.TextField("이미지 이름", area.spriteName);

            EditorGUILayout.Space(5);
            GUILayout.Label("스테이지 리스트", EditorStyles.centeredGreyMiniLabel);

            for (int j = 0; j < area.stageList.Count; j++)
            {
                DrawStageItem(area.stageList, j);
            }

            if (GUILayout.Button("새 스테이지 추가", GUILayout.Width(200)))
            {
                area.stageList.Add(new StageData { id = $"{area.id}_{area.stageList.Count + 1}" });
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }


    private void DrawStageItem(List<StageData> stages, int index)
    {
        var stage = stages[index];
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"스테이지 {index + 1}", EditorStyles.miniBoldLabel);
        if (GUILayout.Button("삭제", GUILayout.Width(60)))
        {
            stages.RemoveAt(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        stage.id = EditorGUILayout.TextField("스테이지 ID", stage.id);
        stage.name = EditorGUILayout.TextField("스테이지 이름", stage.name);
        GUILayout.Space(5);
        stage.monsterId = EditorGUILayout.TextField("등장 몬스터 ID", stage.monsterId);
        stage.monsterCount = EditorGUILayout.IntField("등장 몬스터 수", stage.monsterCount);
        GUILayout.Space(5);
        stage.minGoldDrop = EditorGUILayout.LongField("최소 골드", stage.minGoldDrop);
        stage.maxGoldDrop = EditorGUILayout.LongField("최대 골드", stage.maxGoldDrop);
        stage.expDrop = EditorGUILayout.IntField("경험치 획득량", stage.expDrop);
        GUILayout.Space(5);
        stage.dropEquipID = EditorGUILayout.TextField("드랍 장비 ID", stage.dropEquipID);
        stage.dropPercent = EditorGUILayout.FloatField("드랍 확률", stage.dropPercent);

        EditorGUILayout.EndVertical();
    }

    private void AddNewArea()
    {
        areaDatas.Add(new AreaData
        {
            id = "AREA_0" + (areaDatas.Count + 1),
            stageList = new List<StageData>()
        });
    }
    private void ExportToJson()
    {
        StageDataList dataList = new StageDataList
        {
            areaList = areaDatas
        };

        // 저장될 주소
        string folder = "Assets/Resources/Json/Stage";
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

            string path = Path.Combine(folder, "StageList.json");

            File.WriteAllText(path, json);

            AssetDatabase.Refresh();
            Debug.Log("스테이지 리스트 생성 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"오류 발생: {e.Message}");
        }
    }

}
