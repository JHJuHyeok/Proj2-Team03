using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

public class SkillJsonCreator : EditorWindow
{
    private List<SkillData> skillDatas = new List<SkillData>();
    private Vector2 scrollPos;

    // ������ â ǥ��
    [MenuItem("Tools/JSON/Skill JSON Creator")]
    public static void ShowWindow()
    {
        GetWindow<SkillJsonCreator>("SkillDatabase Creator");
    }

    // ��ũ�� ����
    [SerializeField] bool boolBar = true;

    private void OnGUI()
    {
        GUILayout.Label("��ų JSON ������ ������");

        // ��� ��ư��
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("��ų �߰�"))
        {
            skillDatas.Add(new SkillData());
        }
        if (GUILayout.Button("JSON ���� ����"))
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

        skill.id = EditorGUILayout.TextField("��ų ID", skill.id);
        skill.name = EditorGUILayout.TextField("��ų ��Ī", skill.name);
        skill.spriteName = EditorGUILayout.TextField("�̹��� �̸�", skill.spriteName);
        skill.explain = EditorGUILayout.TextField("��ų ����", skill.explain);
        skill.effect = EditorGUILayout.TextField("��ų ȿ��", skill.effect);

        skill.grade = (SkillGrade)EditorGUILayout.EnumPopup("��ų ���", skill.grade);
        skill.type = (SkillType)EditorGUILayout.EnumPopup("��ų Ÿ��", skill.type);
        skill.request = (SkillRequest)EditorGUILayout.EnumPopup("�ߵ� ���", skill.request);
        skill.element = (SkillElement)EditorGUILayout.EnumPopup("��ų �Ӽ�", skill.element);

        skill.maxLevel = EditorGUILayout.IntField("�ִ� ����", skill.maxLevel);
        skill.needMp = EditorGUILayout.IntField("�䱸 MP", skill.needMp);
        skill.initialRate = EditorGUILayout.FloatField("초기 수치", skill.initialRate); // 조민희: initialRate 타입이 float로 변경되어 IntField → FloatField로 수정
        skill.levelUpValue = EditorGUILayout.FloatField("���� �� ���ġ", skill.levelUpValue);

        if (GUILayout.Button("�� �ʵ� ����", GUILayout.Width(100)))
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

        // ����� �ּ�
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

            // Json ���Ϸ� ��ȯ
            string json = JsonConvert.SerializeObject(dataList, settings);

            string path = Path.Combine(folder, "SkillList.json");

            File.WriteAllText(path, json);

            AssetDatabase.Refresh();
            Debug.Log("��ų ����Ʈ ���� �Ϸ�");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"���� �߻�: {e.Message}");
        }
    }
}
