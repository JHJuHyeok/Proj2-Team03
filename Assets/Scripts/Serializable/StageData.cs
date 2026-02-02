using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StageDataList : IDataList<AreaData>
{
    public List<AreaData> areaList;
    public List<AreaData> GetList() => areaList;
}

[System.Serializable]
public class AreaData
{
    public string id;               // 지역 ID
    public string name;             // 지역 명칭
    public string spriteName;       // 스프라이트 이름

    public List<StageData> stageList;   // 내부 스테이지 리스트
}

[System.Serializable]
public class StageData
{
    public string id;           // 스테이지 ID
    public string name;         // 스테이지 이름

    public string monsterId;    // 등장 몬스터 ID
    public long minGoldDrop;     // 드랍 골드 최소치
    public long maxGoldDrop;     // 드랍 골드 최대치
    public int expDrop;         // 획득 경험치 수치

    public string dropEquipID;  // 드랍 장비 ID
    public float dropPercent;   // 장비 드랍 확률
}
