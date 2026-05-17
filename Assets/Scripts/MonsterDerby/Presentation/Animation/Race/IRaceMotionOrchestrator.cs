using MonsterDerby.Domain.Race;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// レースサンプルをモーション命令へ変換して配信する抽象。
    /// </summary>
    public interface IRaceMotionOrchestrator
    {
        void RegisterRunner(MonsterId monsterId, IRaceRunnerAnimationController controller);
        void BindLane(int laneIndex, MonsterId monsterId);
        void ApplySample(RaceSample sample, float raceTimeSeconds);
        void Update(float raceTimeSeconds);
        void Clear();
    }
}