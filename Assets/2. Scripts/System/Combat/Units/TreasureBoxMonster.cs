using UnityEngine;

// 보물상자 적 - 처치 시 스테이지 루프 리셋
public class RewardBoxMonster : MonsterBase
{
    public override bool IsBoss => false;
    public override bool IsRewardBox => true;

}
