namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// レース中のランナー1体に対するモーション命令インターフェース。
    /// </summary>
    public interface IRaceRunnerAnimationController
    {
        void SetRunState(bool isRunning, float raceTimeSeconds);
        void Tick(float raceTimeSeconds);
        void PlaySkillOverlay(float raceTimeSeconds);
        void PlayDamageOverlay(float raceTimeSeconds);
    }
}