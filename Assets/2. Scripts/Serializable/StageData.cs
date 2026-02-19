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
    public double monsterHp;    // 몬스터 HP
    public int monsterCount;    // 등장 몬스터 수

    public string bossId;       // 등장 보스 ID
    public double bossHp;       // 보스 HP
    public double bossAtk;      // 보스 공격력

    public long goldDrop;       // 드랍 골드 최대치
    public int expDrop;         // 획득 경험치 수치
    public int cubeCount;       // 강화 큐브 획득 수량
    public float cubeRate;      // 강화큐브 획득 확률

    public long goldPerMin;     // 분당 자동사냥 골드
    public long expPerMin;      // 분당 자동사냥 경험치

    public string dropEquipID;  // 드랍 장비 ID
    public float dropPercent;   // 장비 드랍 확률
}
