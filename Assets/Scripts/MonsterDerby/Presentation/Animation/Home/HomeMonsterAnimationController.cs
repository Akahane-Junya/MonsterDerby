using System;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Home
{
    /// <summary>
    /// ホーム画面専用。待機モーションのみを管理する。
    /// </summary>
    public sealed class HomeMonsterAnimationController : MonoBehaviour
    {
        [SerializeField] private MonsterAnimatorAdapter _animatorAdapter;

        private void Awake()
        {
            if (_animatorAdapter == null)
            {
                _animatorAdapter = GetComponent<MonsterAnimatorAdapter>();
            }

            if (_animatorAdapter == null)
            {
                throw new InvalidOperationException("MonsterAnimatorAdapter が見つかりません。");
            }
        }

        private void OnEnable()
        {
            _animatorAdapter.PlayHomeIdle();
        }
    }
}