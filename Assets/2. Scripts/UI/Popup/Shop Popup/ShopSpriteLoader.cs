using UnityEngine;

/*
[승문]
ShopSpriteLoader
- Resources 경로 기준으로 상점/장비/스킬 아이콘 스프라이트를 로드
- Weapon / Accessory / Skill 탭에 따라 기본 경로를 다르게 사용
*/
public static class ShopSpriteLoader
{
    private const string WeaponBasePath = "Slayer Legend/Bookmark UI/Equip UI/Weapon";
    private const string AccessoryBasePath = "Slayer Legend/Bookmark UI/Equip UI/Accessory";
    private const string SkillBasePath = "Slayer Legend/Bookmark UI/skillicon";

    public static Sprite LoadSprite(ShopTab tab, string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return null;

        string path = "";

        switch (tab)
        {
            case ShopTab.Weapon:
                path = WeaponBasePath + "/" + spriteName;
                break;

            case ShopTab.Accessory:
                path = AccessoryBasePath + "/" + spriteName;
                break;

            case ShopTab.Skill:
                path = SkillBasePath + "/" + spriteName;
                break;
        }

        Sprite sp = Resources.Load<Sprite>(path);

        if (sp == null)
        {
            Debug.LogWarning("[ShopSpriteLoader] Sprite not found: " + path);
        }

        return sp;
    }
}