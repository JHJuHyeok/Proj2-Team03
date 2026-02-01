using Newtonsoft.Json;
using System.Numerics;
using System.Collections.Generic;

public class MonsterDataList : IDataList<MonsterData>
{
    public List<MonsterData> monsterList;
    public List<MonsterData> GetList() => monsterList;
}

[System.Serializable]
public class MonsterData
{
    public string id;                   // 몬스터 ID
    public string name;                 // 몬스터 명칭
    public string spriteName;           // 이미지 이름

    public BigInteger maxHp;            // 체력
    public BigInteger Attack;           // 공격력

    public MonsterType type;            // 몬스터 타입
    public SkillElement weakElement;    // 약점 속성
}

public enum MonsterType
{
    normal,
    Boss
}