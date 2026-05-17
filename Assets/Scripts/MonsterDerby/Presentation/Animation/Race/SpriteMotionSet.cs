using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Race
{
    public enum SpriteMotionType
    {
        Idle,
        Ready,
        Run,
        Attack,
        Death,
    }

    [CreateAssetMenu(fileName = "SpriteMotionSet", menuName = "MonsterDerby/Animation/Sprite Motion Set")]
    public sealed class SpriteMotionSet : ScriptableObject
    {
        [Header("Motion Clips")]
        [SerializeField] private SpriteMotionClip _idleClip;
        [SerializeField] private SpriteMotionClip _readyClip;
        [SerializeField] private SpriteMotionClip _runClip;
        [SerializeField] private SpriteMotionClip _attackClip;
        [SerializeField] private SpriteMotionClip _deathClip;

        [Header("Behavior")]
        [SerializeField] private bool _useReadyWhenNotRunning = true;

        public bool UseReadyWhenNotRunning => _useReadyWhenNotRunning;

        public SpriteMotionClip GetClip(SpriteMotionType motion)
        {
            switch (motion)
            {
                case SpriteMotionType.Idle:
                    return _idleClip;
                case SpriteMotionType.Ready:
                    return _readyClip != null && _readyClip.IsValid ? _readyClip : _idleClip;
                case SpriteMotionType.Run:
                    return _runClip;
                case SpriteMotionType.Attack:
                    return _attackClip;
                case SpriteMotionType.Death:
                    return _deathClip;
                default:
                    return _idleClip;
            }
        }
    }
}