#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SlayerLegend.Skill.UI.Grid.Editor
{
    /// <summary>
    /// 스킬 셀 이미지용 임시 스프라이트 생성기
    /// 비정형 스킬(L자, T자)의 개별 셀 표시에 사용
    /// </summary>
    public static class SkillCellSpriteGenerator
    {
        private const string PATH = "Assets/Resources/Skill/Grid";

        [MenuItem("Tools/Skill Grid/Create Temp Cell Sprites")]
        public static void CreateTempSprites()
        {
            // 디렉토리 생성
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Skill"))
                AssetDatabase.CreateFolder("Assets/Resources", "Skill");
            if (!AssetDatabase.IsValidFolder(PATH))
                AssetDatabase.CreateFolder("Assets/Resources/Skill", "Grid");

            // 단색 스프라이트 생성 (파란색 계열)
            CreateSprite("TempCellSprite", new Color(0.3f, 0.6f, 0.9f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SkillCellSpriteGenerator] 임시 셀 스프라이트 생성 완료");
        }

        private static void CreateSprite(string name, Color color)
        {
            // 64x64 텍스처 생성
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            // PNG로 저장
            string path = $"{PATH}/{name}.png";
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();

            // 스프라이트로 임포트 설정
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
