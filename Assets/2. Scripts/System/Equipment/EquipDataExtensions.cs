using System;
using System.Reflection;

namespace SlayerLegend.Equipment
{
    // EquipData 확장 메서드
    // 리플렉션을 사용하여 팀원의 EquipData 필드에 안전하게 접근
    // 팀원 코드를 직접 수정하지 않고 데이터 읽기
    public static class EquipDataExtensions
    {
        private static readonly Type _equipDataType;
        private static readonly FieldInfo _idField;
        private static readonly FieldInfo _nameField;
        private static readonly FieldInfo _spriteNameField;
        private static readonly FieldInfo _gradeField;
        private static readonly FieldInfo _equipEffectField;
        private static readonly FieldInfo _holdEffectsField;

        static EquipDataExtensions()
        {
            _equipDataType = typeof(EquipData);
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            _idField = _equipDataType.GetField("id", flags);
            _nameField = _equipDataType.GetField("name", flags);
            _spriteNameField = _equipDataType.GetField("spriteName", flags);
            _gradeField = _equipDataType.GetField("grade", flags);
            _equipEffectField = _equipDataType.GetField("equipEffect", flags);
            _holdEffectsField = _equipDataType.GetField("holdEffects", flags);
        }

        // 장비 ID 가져오기
        public static string GetId(this EquipData data)
        {
            if (data == null || _idField == null) return string.Empty;
            return _idField.GetValue(data) as string;
        }

        // 장비 이름 가져오기
        public static string GetName(this EquipData data)
        {
            if (data == null || _nameField == null) return string.Empty;
            return _nameField.GetValue(data) as string;
        }

        // 스프라이트 이름 가져오기
        public static string GetSpriteName(this EquipData data)
        {
            if (data == null || _spriteNameField == null) return string.Empty;
            return _spriteNameField.GetValue(data) as string;
        }

        // 장비 등급 가져오기
        public static EquipGrade GetGrade(this EquipData data)
        {
            if (data == null || _gradeField == null) return EquipGrade.Common;
            var value = _gradeField.GetValue(data);
            return value is EquipGrade ? (EquipGrade)value : EquipGrade.Common;
        }

        // 메인 효과 가져오기
        public static ItemEffect GetEquipEffect(this EquipData data)
        {
            if (data == null || _equipEffectField == null) return null;
            return _equipEffectField.GetValue(data) as ItemEffect;
        }

        // 추가 효과 리스트 가져오기
        public static System.Collections.Generic.List<ItemEffect> GetHoldEffects(this EquipData data)
        {
            if (data == null || _holdEffectsField == null) return null;
            return _holdEffectsField.GetValue(data) as System.Collections.Generic.List<ItemEffect>;
        }

        // 등급을 문자열로 변환
        public static string GetGradeString(this EquipData data)
        {
            EquipGrade grade = GetGrade(data);
            return grade.ToString();
        }

        // 등급에 따른 색상 코드 반환 (UI용)
        public static string GetGradeColor(this EquipData data)
        {
            EquipGrade grade = GetGrade(data);
            return grade switch
            {
                EquipGrade.Common => "#FFFFFF",      // 흰색
                EquipGrade.Uncommon => "#00FF00",    // 초록
                EquipGrade.Rare => "#0080FF",        // 파랑
                EquipGrade.Hero => "#FF00FF",        // 자주
                EquipGrade.Legend => "#FF8000",      // 주황
                EquipGrade.Myth => "#FF0000",        // 빨강
                EquipGrade.Infinite => "#FFD700",    // 금색
                _ => "#FFFFFF"
            };
        }
    }
}
