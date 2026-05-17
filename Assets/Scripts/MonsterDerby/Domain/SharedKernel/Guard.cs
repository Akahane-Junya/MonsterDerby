// MonsterDerby.Domain.Race（C# 9 / クリーン版）
// 方針: Domain の型は「任意」を null で表現しない。
//       任意のコレクションは空配列（Array.Empty<T>()）で表現する。
//       公開コンストラクタは必須入力を検証し、不正なら即例外（Fail-Fast）。


namespace MonsterDerby.Domain.SharedKernel
{
    using System;

    internal static class Guard
    {
        // 境界（入力点）で不変条件を検証する小さなヘルパー。
        // Domain ルール: 不正入力は早期に例外で落とす（Fail-Fast）。

        public static T NotNull<T>(T value, string name) where T : class
            => value ?? throw new ArgumentNullException(name);

        public static string NotNullOrEmpty(string value, string name)
            => string.IsNullOrEmpty(value) ? throw new ArgumentException("Value cannot be null or empty.", name) : value;

        public static void NonNegative(float value, string name)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(name, value, "Value must be >= 0.");
        }

        public static void NonNegative(int value, string name)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(name, value, "Value must be >= 0.");
        }

        public static void Positive(float value, string name)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(name, value, "Value must be > 0.");
        }

        public static void Positive(int value, string name)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(name, value, "Value must be > 0.");
        }

        public static void InRangeInclusive(float value, float min, float max, string name)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(name, value, $"Value must be between {min} and {max} (inclusive).");
            }
        }

        public static void InRangeInclusive(int value, int min, int max, string name)
        {
            if (value < min || value > max)
            {
                throw new ArgumentOutOfRangeException(name, value, $"Value must be between {min} and {max} (inclusive).");
            }
        }
    }
}
