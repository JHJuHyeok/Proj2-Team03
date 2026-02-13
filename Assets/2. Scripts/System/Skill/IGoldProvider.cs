namespace SlayerLegend.Skill
{
    // 골드 시스템 인터페이스
    public interface IGoldProvider
    {
        long CurrentGold { get; }
        bool HasEnoughGold(long amount);
        bool SpendGold(long amount);
        void AddGold(long amount);
        event System.Action<long> OnGoldChanged;
    }
}
