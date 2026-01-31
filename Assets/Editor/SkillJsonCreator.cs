using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

public class SkillJsonCreator : EditorWindow
{
    private List<SkillData> skillDatas = new List<SkillData>();
    private Vector2 scrollPos;

    // 에디터 창 표시
    [MenuItem("Tools/JSON/Skill JSON Creator")]
    public static void ShowWindow()
    {
        GetWindow<SkillJsonCreator>("SkillDatabase Creator");
    }

    // 스크롤 변수
    [SerializeField] bool boolBar = true;

    private void OnGUI()
    {
        GUILayout.Label("스킬 JSON 데이터 생성기");

        // 상단 버튼들
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("스킬 추가"))
        {
            skillDatas.Add(new SkillData());
        }
        if (GUILayout.Button("JSON 파일 생성"))
        {
            ExportToJson();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < skillDatas.Count; i++)
        {
            SkillItem(i);
        }
        EditorGUILayout.EndScrollView();
    }

    private void SkillItem(int index)
    {
        SkillData skill = skillDatas[index];
        EditorGUILayout.BeginVertical("box");

        skill.id = EditorGUILayout.TextField("스킬 ID", skill.id);
        skill.name = EditorGUILayout.TextField("스킬 명칭", skill.name);
        skill.explain = EditorGUILayout.TextField("스킬 설명", skill.explain);
        skill.effect = EditorGUILayout.TextField("스킬 효과", skill.effect);

        skill.grade = (SkillGrade)EditorGUILayout.EnumPopup("스킬 등급", skill.grade);
        skill.type = (SkillType)EditorGUILayout.EnumPopup("스킬 타입", skill.type);
        skill.request = (SkillRequest)EditorGUILayout.EnumPopup("발동 방식", skill.request);
        skill.element = (SkillElement)EditorGUILayout.EnumPopup("스킬 속성", skill.element);

        skill.maxLevel = EditorGUILayout.IntField("최대 레벨", skill.maxLevel);
        skill.needMp = EditorGUILayout.IntField("요구 MP", skill.needMp);
        skill.initialRate = EditorGUILayout.IntField("초기 수치", skill.initialRate);
        skill.levelUpValue = EditorGUILayout.FloatField("레벨 별 상승치", skill.levelUpValue);

        if (GUILayout.Button("이 필드 삭제", GUILayout.Width(100)))
        {
            skillDatas.RemoveAt(index);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void ExportToJson()
    {
        SkillDataList dataList = new SkillDataList
        {
            skillList = skillDatas
        };

        // 저장될 주소
        string folder = "Assets/Resources/Json/Skill";
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

            string path = Path.Combine(folder, "SkillList.json");

            File.WriteAllText(path, json);

            AssetDatabase.Refresh();
            Debug.Log("스킬 리스트 생성 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"오류 발생: {e.Message}");
        }
    }
}
