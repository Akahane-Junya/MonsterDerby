// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。


namespace MonsterDerby.Domain.SharedKernel
{
    using System;

    /// <summary>
    /// 小さな値オブジェクトID。
    /// シミュレーション内部に生の文字列を持ち込まないための型。
    /// </summary>
    public readonly struct MonsterId : IEquatable<MonsterId>
    {
        public string Value { get; }
        public MonsterId(string value) { Value = Guard.NotNullOrEmpty(value, nameof(value)); }
        public bool Equals(MonsterId other) => StringComparer.Ordinal.Equals(Value, other.Value);
        public override bool Equals(object obj) => obj is MonsterId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
        public static bool operator ==(MonsterId a, MonsterId b) => a.Equals(b);
        public static bool operator !=(MonsterId a, MonsterId b) => !a.Equals(b);
    }

    public readonly struct SpeciesId : IEquatable<SpeciesId>
    {
        public string Value { get; }
        public SpeciesId(string value) { Value = Guard.NotNullOrEmpty(value, nameof(value)); }
        public bool Equals(SpeciesId other) => StringComparer.Ordinal.Equals(Value, other.Value);
        public override bool Equals(object obj) => obj is SpeciesId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
        public static bool operator ==(SpeciesId a, SpeciesId b) => a.Equals(b);
        public static bool operator !=(SpeciesId a, SpeciesId b) => !a.Equals(b);
    }

    public readonly struct SkillId : IEquatable<SkillId>
    {
        public string Value { get; }
        public SkillId(string value) { Value = Guard.NotNullOrEmpty(value, nameof(value)); }
        public bool Equals(SkillId other) => StringComparer.Ordinal.Equals(Value, other.Value);
        public override bool Equals(object obj) => obj is SkillId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
        public static bool operator ==(SkillId a, SkillId b) => a.Equals(b);
        public static bool operator !=(SkillId a, SkillId b) => !a.Equals(b);
    }

    public readonly struct CourseId : IEquatable<CourseId>
    {
        public string Value { get; }
        public CourseId(string value) { Value = Guard.NotNullOrEmpty(value, nameof(value)); }
        public bool Equals(CourseId other) => StringComparer.Ordinal.Equals(Value, other.Value);
        public override bool Equals(object obj) => obj is CourseId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
        public static bool operator ==(CourseId a, CourseId b) => a.Equals(b);
        public static bool operator !=(CourseId a, CourseId b) => !a.Equals(b);
    }
}
