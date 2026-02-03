using UnityEngine;
using System.Numerics;

//===============스탯 기본형===============//

[System.Serializable]
public class StatValue
{
    public StatType type;
    public BigInteger baseValue;    // 기본 수치(합연산)
    public double multiplier;       // 증가 퍼센트(곱연산)
}

public enum StatType
{
    STR,            // 공격력
    HP,             // 체력
    VIT_HP,         // 체력 회복량
    CTI_DMG,        // 크리티컬 데미지
    CRI_Per,        // 크리티컬 확률
    MANA,           // 마나
    VIT_MANA,       // 마나 회복량
    ACC,            // 명중
    DODGE,          // 회피
    ADD_GOLD,       // 추가 골드 획득량
    ADD_EXP         // 추가 경험치
}