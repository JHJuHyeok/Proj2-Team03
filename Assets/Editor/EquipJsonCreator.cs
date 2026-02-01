using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

public class EquipJsonCreator : EditorWindow
{
    private EquipDataList weaponList = new EquipDataList { listType = EquipType.Weapon, equipList = new List<EquipData>() };
    private EquipDataList accList = new EquipDataList { listType = EquipType.Accessorie, equipList = new List<EquipData>() };

    private int currentTab = 0;
    private string[] tabNames = { "Weapons", "Accessories" };
    private Vector2 scrollPos;

    [MenuItem("Tools/JSON/Equip Json Creator")]
    public static void ShowWindow() => GetWindow<EquipJsonCreator>("EquipDatabase Creator");

    private void OnGUI()
    {
        GUILayout.Label("장비 JSON 데이터 생성기", EditorStyles.boldLabel);

        // 상단 탭
        currentTab = GUILayout.Toolbar(currentTab, tabNames);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("장비 추가", EditorStyles.toolbarButton)) AddNewItem();
        if (GUILayout.Button("JSON 파일 생성", EditorStyles.toolbarButton)) ExportAllToJson();
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // 현재 선택된 탭의 리스트 가져오기
        EquipDataList activeList = (currentTab == 0) ? weaponList : accList;

        for (int i = 0; i < activeList.equipList.Count; i++)
        {
            DrawEquipItem(activeList.equipList, i);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawEquipItem(List<EquipData> list, int index)
    {
        var data = list[index];
        EditorGUILayout.BeginVertical("helpbox");

        // 제목 줄
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{data.grade} | {data.name}", EditorStyles.boldLabel);
        if (GUILayout.Button("Remove", GUILayout.Width(60))) { list.RemoveAt(index); return; }
        EditorGUILayout.EndHorizontal();

        // 기본 정보
        data.id = EditorGUILayout.TextField("장비 ID", data.id);
        data.name = EditorGUILayout.TextField("장비 명칭", data.name);
        data.spriteName = EditorGUILayout.TextField("이미지 이름", data.spriteName);
        data.grade = (EquipGrade)EditorGUILayout.EnumPopup("장비 등급", data.grade);

        EditorGUILayout.Space(5);

        // 장착 효과
        EditorGUILayout.LabelField("▼ 장착 효과", EditorStyles.miniBoldLabel);
        DrawEffectFields(data.equipEffect);

        EditorGUILayout.Space(5);

        // 보유 효과
        EditorGUILayout.LabelField("▼ 보유 효과 목록", EditorStyles.miniBoldLabel);
        for (int j = 0; j < data.holdEffects.Count; j++)
        {
            EditorGUILayout.BeginHorizontal();
            DrawEffectFields(data.holdEffects[j]);
            if (GUILayout.Button("x", GUILayout.Width(20))) { data.holdEffects.RemoveAt(j); }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("보유 효과 추가", GUILayout.Width(120))) data.holdEffects.Add(new ItemEffect());

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    // Effect 입력 필드를 재사용하기 위한 함수
    private void DrawEffectFields(ItemEffect effect)
    {
        EditorGUILayout.BeginVertical("box");
        effect.type = (EffectType)EditorGUILayout.EnumPopup("효과 타입", effect.type);
        effect.initValue = EditorGUILayout.FloatField("초기치", effect.initValue);
        effect.levelUpValue = EditorGUILayout.FloatField("증가치", effect.levelUpValue);
        EditorGUILayout.EndVertical();
    }

    private void AddNewItem()
    {
        var targetList = (currentTab == 0) ? weaponList.equipList : accList.equipList;
        targetList.Add(new EquipData
        {
            equipEffect = new ItemEffect(),
            holdEffects = new List<ItemEffect>()
        });
    }

    private void ExportAllToJson()
    {
        string path = "Assets/Resources/Json/Equip";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        // 무기와 액세서리 각각 저장
        SaveFile(path, "WeaponList", weaponList);
        SaveFile(path, "AccessorieList", accList);

        AssetDatabase.Refresh();
        Debug.Log("장비 리스트 생성 완료");
    }

    private void SaveFile(string path, string fileName, object data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(Path.Combine(path, $"{fileName}.json"), json);
    }
}