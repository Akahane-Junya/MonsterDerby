namespace MonsterDerby.Domain.Skill
{
    /// <summary>
    /// スキル効果のポリモーフィック表現。
    ///
    /// - enum + switch を避け、効果追加を「新しいクラス追加」で完結させる。
    /// - Race側は IEffectSink（受け皿）だけを実装すれば良い。
    /// </summary>
    public interface IEffect
    {
        /// <summary>デバッグ/観測用の種別名（ログ・サンプル出力用）。</summary>
        string Kind { get; }

        /// <summary>デバッグ/観測用の代表値（例: 倍率・加算量）。</summary>
        float Magnitude { get; }

        /// <summary>
        /// 効果適用。
        /// 実際の状態変化は sink 側に委譲する（Race以外へ漏らさない）。
        /// </summary>
        void Apply(IEffectSink sink);
    }

    /// <summary>
    /// 効果の受け皿（Raceのルール側で実装）。
    /// ここに並ぶメソッド群が「ゲームルールとして許可する作用」の境界になる。
    /// </summary>
    public interface IEffectSink
    {
        void MultiplySpeed(float multiplier);
        void MultiplyAcceleration(float multiplier);
        void AddSpeed(float metersPerSecond);
        void MultiplySlow(float multiplier);
        void MultiplyStaminaDrain(float multiplier);
    }

    public abstract class EffectBase : IEffect
    {
        public string Kind { get; }
        public float Magnitude { get; }

        protected EffectBase(string kind, float magnitude)
        {
            Kind = kind;
            Magnitude = magnitude;
        }

        public abstract void Apply(IEffectSink sink);
    }

    public sealed class SpeedMultiplierEffect : EffectBase
    {
        public SpeedMultiplierEffect(float multiplier) : base("SpeedMultiplier", multiplier) { }
        public override void Apply(IEffectSink sink) => sink.MultiplySpeed(Magnitude);
    }

    public sealed class SpeedAdditiveEffect : EffectBase
    {
        public SpeedAdditiveEffect(float metersPerSecond) : base("SpeedAdditive", metersPerSecond) { }
        public override void Apply(IEffectSink sink) => sink.AddSpeed(Magnitude);
    }

    public sealed class AccelerationMultiplierEffect : EffectBase
    {
        public AccelerationMultiplierEffect(float multiplier) : base("AccelerationMultiplier", multiplier) { }
        public override void Apply(IEffectSink sink) => sink.MultiplyAcceleration(Magnitude);
    }

    public sealed class SlowMultiplierEffect : EffectBase
    {
        public SlowMultiplierEffect(float multiplier) : base("Slow", multiplier) { }
        public override void Apply(IEffectSink sink) => sink.MultiplySlow(Magnitude);
    }

    public sealed class StaminaDrainMultiplierEffect : EffectBase
    {
        public StaminaDrainMultiplierEffect(float multiplier) : base("StaminaDrainMultiplier", multiplier) { }
        public override void Apply(IEffectSink sink) => sink.MultiplyStaminaDrain(Magnitude);
    }
}
