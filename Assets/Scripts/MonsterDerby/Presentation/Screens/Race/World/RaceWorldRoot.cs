using UnityEngine;

namespace MonsterDerby.Presentation.Screens.Race.World
{
    /// <summary>
    /// Race画面のWorld全体を管理
    /// </summary>
    public sealed class RaceWorldRoot : MonoBehaviour
    {
        [SerializeField] private CourseSelectWorldView _courseSelectWorldView;
        [SerializeField] private RacingWorldView _racingWorldView;
        [SerializeField] private ResultWorldView _resultWorldView;

        public RacingWorldView RacingWorldView => _racingWorldView;

        private void Awake()
        {
            if (_courseSelectWorldView == null)
                throw new System.InvalidOperationException("CourseSelectWorldView が設定されていません。");
            if (_racingWorldView == null)
                throw new System.InvalidOperationException("RacingWorldView が設定されていません。");
            if (_resultWorldView == null)
                throw new System.InvalidOperationException("ResultWorldView が設定されていません。");

            _courseSelectWorldView.gameObject.SetActive(false);
            _racingWorldView.gameObject.SetActive(false);
            _resultWorldView.gameObject.SetActive(false);
        }

        public void SetActivePhase(RacePhase phase)
        {
            _courseSelectWorldView.gameObject.SetActive(phase == RacePhase.CourseSelect);
            _racingWorldView.gameObject.SetActive(phase == RacePhase.Racing);
            _resultWorldView.gameObject.SetActive(phase == RacePhase.Result);
        }
    }
}