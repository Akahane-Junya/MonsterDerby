namespace MonsterDerby.Domain.Race
{
    using MonsterDerby.Domain.SharedKernel;
    using System.Collections.Generic;

    internal sealed class RaceState
    {
        public float TimeSeconds { get; private set; }
        public int Tick { get; private set; }

        public RunnerState[] Runners { get; }
        public List<ProjectileState> Projectiles { get; }
        public List<MonsterId> FinishOrder { get; }

        public RaceState(RunnerState[] runners)
        {
            Runners = runners ?? throw new System.ArgumentNullException(nameof(runners));
            Projectiles = new List<ProjectileState>(capacity: 32);
            FinishOrder = new List<MonsterId>(capacity: runners.Length);
            TimeSeconds = 0f;
            Tick = 0;
        }

        public void AdvanceTime(float dtSeconds)
        {
            Guard.Positive(dtSeconds, nameof(dtSeconds));
            TimeSeconds += dtSeconds;
            Tick += 1;
        }
    }
}