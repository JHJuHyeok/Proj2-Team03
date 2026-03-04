using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Skill.UI.Grid
{
    /*
    [조민희]
    SkillPresetSaveData
    - 5개의 스킬 프리셋 저장
    - 각 프리셋별로 독립적인 스킬 그리드 배치 저장
    - 현재 선택된 프리셋 인덱스 관리

    수정 (2026-03-04): List<PlacedSkillData>[] 대신 개별 리스트 사용
    - Unity JsonUtility는 List<List<>> 또는 List[] 직렬화 미지원
    - 개별 필드(preset0, preset1, ...)로 변경하여 직렬화 문제 해결
    */
    [Serializable]
    public class SkillPresetSaveData
    {
        public const int MAX_PRESETS = 5;  // 최대 프리셋 개수

        public int currentPresetIndex = 0;  // 현재 선택된 프리셋 (0~4)

        // 개별 프리셋 리스트로 변경 (Unity JsonUtility 호환) - 조민희 수정
        public List<PlacedSkillData> preset0 = new List<PlacedSkillData>();
        public List<PlacedSkillData> preset1 = new List<PlacedSkillData>();
        public List<PlacedSkillData> preset2 = new List<PlacedSkillData>();
        public List<PlacedSkillData> preset3 = new List<PlacedSkillData>();
        public List<PlacedSkillData> preset4 = new List<PlacedSkillData>();

        // 인덱스로 프리셋 리스트 가져오기
        private List<PlacedSkillData> GetPresetList(int index)
        {
            switch (index)
            {
                case 0: return preset0;
                case 1: return preset1;
                case 2: return preset2;
                case 3: return preset3;
                case 4: return preset4;
                default: return preset0;
            }
        }

        // 현재 프리셋 데이터 가져오기
        public List<PlacedSkillData> GetCurrentPreset()
        {
            return GetPresetList(currentPresetIndex);
        }

        // 특정 프리셋 데이터 가져오기
        public List<PlacedSkillData> GetPreset(int index)
        {
            if (index < 0 || index >= MAX_PRESETS)
            {
                Debug.LogWarning($"[SkillPresetSaveData] 잘못된 프리셋 인덱스: {index}");
                return preset0;
            }
            return GetPresetList(index);
        }

        // 현재 프리셋 데이터 설정
        public void SetCurrentPreset(List<PlacedSkillData> skills)
        {
            SetPreset(currentPresetIndex, skills);
        }

        // 특정 프리셋 데이터 설정
        public void SetPreset(int index, List<PlacedSkillData> skills)
        {
            if (index < 0 || index >= MAX_PRESETS)
            {
                Debug.LogWarning($"[SkillPresetSaveData] 잘못된 프리셋 인덱스: {index}");
                return;
            }

            var targetPreset = GetPresetList(index);
            targetPreset.Clear();
            if (skills != null)
            {
                targetPreset.AddRange(skills);
            }
        }

        // 프리셋 전환
        public bool SwitchPreset(int newIndex)
        {
            if (newIndex < 0 || newIndex >= MAX_PRESETS)
            {
                Debug.LogWarning($"[SkillPresetSaveData] 잘못된 프리셋 인덱스: {newIndex}");
                return false;
            }

            if (newIndex == currentPresetIndex)
            {
                return false;  // 이미 선택된 프리셋
            }

            currentPresetIndex = newIndex;
            return true;
        }

        // 현재 프리셋 비우기
        public void ClearCurrentPreset()
        {
            GetCurrentPreset()?.Clear();
        }

        // 특정 프리셋 비우기
        public void ClearPreset(int index)
        {
            if (index >= 0 && index < MAX_PRESETS)
            {
                GetPresetList(index)?.Clear();
            }
        }

        // 모든 프리셋 비우기
        public void ClearAllPresets()
        {
            preset0?.Clear();
            preset1?.Clear();
            preset2?.Clear();
            preset3?.Clear();
            preset4?.Clear();
            currentPresetIndex = 0;
        }

        // JSON으로 변환
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        // JSON에서 로드
        public static SkillPresetSaveData FromJson(string json)
        {
            try
            {
                var data = JsonUtility.FromJson<SkillPresetSaveData>(json);

                // 각 프리셋 초기화 확인
                if (data.preset0 == null) data.preset0 = new List<PlacedSkillData>();
                if (data.preset1 == null) data.preset1 = new List<PlacedSkillData>();
                if (data.preset2 == null) data.preset2 = new List<PlacedSkillData>();
                if (data.preset3 == null) data.preset3 = new List<PlacedSkillData>();
                if (data.preset4 == null) data.preset4 = new List<PlacedSkillData>();

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillPresetSaveData] JSON 파싱 실패: {e.Message}");
                return new SkillPresetSaveData();
            }
        }

        // 복사본 생성
        public SkillPresetSaveData Clone()
        {
            string json = ToJson();
            return FromJson(json);
        }
    }
}
