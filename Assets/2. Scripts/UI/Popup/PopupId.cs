//[승문]
public enum PopupId
{
    None = 0,

    // 기존
    EnhanceStat = 1, // 승급스텟
    Skill = 2,       //스킬
    Equip = 3,       //장비
    Shop = 4,        //상점
                     
    Settings = 5,    //설정
    Mailbox = 6,     //메일함
    Quest = 7,       //퀘스트
    Scroll = 8,      //버프스크롤
    SkillDetail =9,
}
// 장비타입
public enum EquipTab
{
    Weapon,     //무기
    Accessory   //악세서리
}
// 스킬타입
public enum SkillAttribute
{
    Fire,   // 불
    Water,  // 물
    Wind,   // 바람
    Earth   // 땅
}
// 상점타입
public enum ShopTab
{
    Weapon,     // 무기
    Accessory,  // 악세서리
    Skill       // 스킬
}