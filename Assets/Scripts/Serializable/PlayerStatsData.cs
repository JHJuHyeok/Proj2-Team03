using System.Collections.Generic;

public class PlayerStatsDataList : IDataList<PlayerStatsData>
{
    public List<PlayerStatsData> playerStatsList;
    public List<PlayerStatsData> GetList() => playerStatsList;
}

[System.Serializable]
public class PlayerStatsData
{
    public string id;                       // 스탯 프로필 ID

    // 기본 스탯
    public float maxHp;                     // 최대 체력
    public float attackDamage;              // 공격력
    public float defense;                   // 방어력
    public float attackSpeed;               // 공격 속도
    public float attackRange;               // 공격 사거리
    public float detectionRange;            // 탐지 범위

    // 마나 시스템
    public float maxMana;                   // 최대 마나
    public float manaRegenPerSecond;        // 초당 마나 회복량

    // 회복
    public float hpRegenPerSecond;          // 초당 체력 회복량

    // 치명타 (Critical)
    public float criticalRate;              // 치명타 확률 (0.0 ~ 1.0)
    public float criticalDamage;            // 치명타 공격력 배율 (예: 2.0 = 200%)

    // 회심의 일격 (Blow) - 치명타 확률 100% 달성 시 해금
    public float blowDamage;                // 회심의 일격 공격력 배율
    public float blowProbability;           // 회심의 일격 확률

    // 기타 스탯
    public float accuracy;                  // 명중률
    public float dodge;                     // 회피율
    public float goldGainPercent;           // 골드 추가 획득량 (%)
    public float expGainPercent;            // 경험치 추가 획득량 (%)
}
