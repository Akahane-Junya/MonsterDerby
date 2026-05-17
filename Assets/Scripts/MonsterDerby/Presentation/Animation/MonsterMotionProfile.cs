using UnityEngine;

namespace MonsterDerby.Presentation.Animation
{
    [CreateAssetMenu(fileName = "MonsterMotionProfile", menuName = "MonsterDerby/Animation/Monster Motion Profile")]
    public sealed class MonsterMotionProfile : ScriptableObject
    {
        [Header("State Names")]
        [SerializeField] private string _homeIdleStateName = "IdleHome";
        [SerializeField] private string _raceIdleStateName = "IdleRace";
        [SerializeField] private string _runStateName = "Run";
        [SerializeField] private string _skillStateName = "Skill";
        [SerializeField] private string _damageStateName = "Damage";

        [Header("Fade Seconds")]
        [SerializeField] private float _idleFadeSeconds = 0.12f;
        [SerializeField] private float _runFadeSeconds = 0.08f;
        [SerializeField] private float _overlayFadeSeconds = 0.05f;

        [Header("Overlay Durations")]
        [SerializeField] private float _skillDurationSeconds = 0.35f;
        [SerializeField] private float _damageDurationSeconds = 0.30f;

        public string HomeIdleStateName => _homeIdleStateName;
        public string RaceIdleStateName => _raceIdleStateName;
        public string RunStateName => _runStateName;
        public string SkillStateName => _skillStateName;
        public string DamageStateName => _damageStateName;

        public float IdleFadeSeconds => _idleFadeSeconds;
        public float RunFadeSeconds => _runFadeSeconds;
        public float OverlayFadeSeconds => _overlayFadeSeconds;
        public float SkillDurationSeconds => _skillDurationSeconds;
        public float DamageDurationSeconds => _damageDurationSeconds;
    }
}