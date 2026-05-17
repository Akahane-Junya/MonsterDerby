namespace MonsterDerby.Domain.SharedKernel
{
    using System;

    /// <summary>
    /// 固定小数。float/double を避けるための値オブジェクト。
    /// - 例: Scale=1000 の場合、Raw=1500 は 1.5 を表す。
    /// </summary>
    public readonly struct Fixed
    {
        public const int Scale = 1000;
        public int Raw { get; }

        private Fixed(int raw)
        {
            Raw = raw;
        }

        public static Fixed FromRaw(int raw) => new Fixed(raw);

        public static Fixed FromInt(int value) => new Fixed(checked(value * Scale));

        public static Fixed Zero => new Fixed(0);

        public static Fixed operator +(Fixed a, Fixed b) => new Fixed(checked(a.Raw + b.Raw));

        public static Fixed operator -(Fixed a, Fixed b) => new Fixed(checked(a.Raw - b.Raw));

        public static Fixed operator /(Fixed a, int divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            return new Fixed(a.Raw / divisor);
        }

        /// <summary>
        /// Banker's rounding（0.5は偶数へ）。
        /// 丸めバイアスを減らす。
        /// </summary>
        public int ToIntBankersRounded()
        {
            int baseValue = Raw / Scale;
            int remainder = Raw % Scale;
            int absR = Math.Abs(remainder);
            int half = Scale / 2;

            if (absR < half) return baseValue;
            if (absR > half) return baseValue + Math.Sign(remainder);

            // exactly half
            return (baseValue % 2 == 0) ? baseValue : baseValue + Math.Sign(remainder);
        }

        public override string ToString()
        {
            // デバッグ用途（Domainのロジックはこれに依存しない）
            return $"{Raw}/{Scale}";
        }
    }
}
