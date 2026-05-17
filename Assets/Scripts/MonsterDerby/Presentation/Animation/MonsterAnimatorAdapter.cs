using System;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation
{
    [RequireComponent(typeof(Animator))]
    public sealed class MonsterAnimatorAdapter : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private MonsterMotionProfile _motionProfile;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_animator == null)
            {
                throw new InvalidOperationException("Animator が見つかりません。");
            }

            if (_motionProfile == null)
            {
                throw new InvalidOperationException("MonsterMotionProfile が設定されていません。");
            }
        }

        public MonsterMotionProfile MotionProfile => _motionProfile;

        public void PlayHomeIdle()
        {
            _animator.CrossFade(_motionProfile.HomeIdleStateName, _motionProfile.IdleFadeSeconds);
        }

        public void PlayRaceIdle()
        {
            _animator.CrossFade(_motionProfile.RaceIdleStateName, _motionProfile.IdleFadeSeconds);
        }

        public void PlayRun()
        {
            _animator.CrossFade(_motionProfile.RunStateName, _motionProfile.RunFadeSeconds);
        }

        public void PlaySkill()
        {
            _animator.CrossFade(_motionProfile.SkillStateName, _motionProfile.OverlayFadeSeconds);
        }

        public void PlayDamage()
        {
            _animator.CrossFade(_motionProfile.DamageStateName, _motionProfile.OverlayFadeSeconds);
        }
    }
}