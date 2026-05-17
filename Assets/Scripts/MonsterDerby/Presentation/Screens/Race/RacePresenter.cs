using System;
using System.Collections.Generic;
using MonsterDerby.Application.Context;
using MonsterDerby.Application.Game;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.Race;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.MasterData;
using MonsterDerby.Infrastructure.Repositories;
using MonsterDerby.Presentation.Animation.Race;
using MonsterDerby.Presentation.Navigation;
using MonsterDerby.Presentation.Screens.Race.UI;
using System.Linq;

namespace MonsterDerby.Presentation.Screens.Race
{
    /// <summary>
    /// Race画面のPresenter
    /// 3フェーズ（CourseSelect / Racing / Result）を制御
    /// </summary>
    public sealed class RacePresenter : IScreenPresenter
    {
        private readonly IRaceContext _raceContext;
        private readonly INavigationContext _navigationContext;
        private readonly ScriptableObjectMonsterVisualRepository _monsterVisualRepository;
        private readonly MasterDataCatalog _masterDataCatalog;
        private readonly GameSession _gameSession;
        private RaceView _view;
        private Dictionary<MonsterId, string> _lastDisplayNameByMonsterId = new Dictionary<MonsterId, string>();

        public RacePresenter(
            IRaceContext raceContext,
            INavigationContext navigationContext,
            ScriptableObjectMonsterVisualRepository monsterVisualRepository,
            MasterDataCatalog masterDataCatalog,
            GameSession gameSession)
        {
            _raceContext = raceContext ?? throw new ArgumentNullException(nameof(raceContext));
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
            _monsterVisualRepository = monsterVisualRepository ?? throw new ArgumentNullException(nameof(monsterVisualRepository));
            _masterDataCatalog = masterDataCatalog ?? throw new ArgumentNullException(nameof(masterDataCatalog));
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        }

        public void BindView(IScreenView view)
        {
            _view = view as RaceView ?? throw new ArgumentException("RaceView が必要です。", nameof(view));

            // UIイベント購読
            _view.CourseSelectUI.OnCourseSelected += HandleCourseSelected;
            _view.CourseSelectUI.OnBackClicked += HandleBackFromCourseSelect;
            _view.RacingUI.OnSpeedMultiplierChanged += HandleSpeedMultiplierChanged;
            _view.ResultUI.OnBackToHome += HandleBackToHome;

            // Worldイベント購読
            _view.WorldRoot.RacingWorldView.OnRaceCompleted += HandleRaceCompleted;
            _view.WorldRoot.RacingWorldView.InitializeVisualOperator(new RaceMonsterVisualAssetOperator(_monsterVisualRepository));
        }

        public void Show()
        {
            _view.CourseSelectUI.SetOptions(BuildCourseOptions());
            TransitionToPhase(RacePhase.CourseSelect);
        }

        public void Hide()
        {
            if (_view != null)
            {
                _view.CourseSelectUI.OnCourseSelected -= HandleCourseSelected;
                _view.CourseSelectUI.OnBackClicked -= HandleBackFromCourseSelect;
                _view.RacingUI.OnSpeedMultiplierChanged -= HandleSpeedMultiplierChanged;
                _view.ResultUI.OnBackToHome -= HandleBackToHome;
                _view.WorldRoot.RacingWorldView.OnRaceCompleted -= HandleRaceCompleted;
            }
        }

        // ===== フェーズ遷移 =====

        private void TransitionToPhase(RacePhase phase)
        {
            // UI表示切替
            _view.CourseSelectUI.SetActive(phase == RacePhase.CourseSelect);
            _view.RacingUI.SetActive(phase == RacePhase.Racing);
            _view.ResultUI.SetActive(phase == RacePhase.Result);

            // World表示切替
            _view.WorldRoot.SetActivePhase(phase);
        }

        // ===== イベントハンドラ =====


        private void HandleCourseSelected(CourseSelectUI.CourseOption option)
        {
            var raceDefinition = FindRaceById(option.RaceId);
            if (raceDefinition == null)
            {
                UnityEngine.Debug.LogError($"レース定義が見つかりません: {option.RaceId}");
                return;
            }

            // レベル制限チェック
            if (_gameSession.HasWorld && _gameSession.State != null && _gameSession.State.CurrentMonster != null)
            {
                var playerLevel = _gameSession.State.CurrentMonster.Level.Value;
                if (playerLevel < raceDefinition.minLevel || playerLevel > raceDefinition.maxLevel)
                {
                    UnityEngine.Debug.LogError($"このレースは参加可能レベルが {raceDefinition.minLevel}～{raceDefinition.maxLevel} です。あなたのモンスターのレベル: {playerLevel}");
                    // 必要ならUIで警告表示も追加
                    return;
                }
            }

            var participants = BuildRaceParticipants(raceDefinition, out var playerRaceMonsterId, out var displayNameByMonsterId);
            if (participants.Length == 0)
            {
                UnityEngine.Debug.LogError($"レース参加者が0体です: {raceDefinition.raceId}");
                return;
            }

            try
            {
                // UseCase実行してRaceRunOutputを取得
                var output = _raceContext.RunRaceUseCase.Execute(option.CourseId, participants);

                _lastDisplayNameByMonsterId = displayNameByMonsterId;

                _gameSession.Apply(world => world.WithRaceOutcome(
                    option.RaceId,
                    playerRaceMonsterId,
                    output.RaceResult.FinishOrder,
                    output.RaceResult.FinishTimeSecondsByMonsterId));

                _view.ResultUI.DisplayTop3(BuildTop3Lines(output.RaceResult));

                _view.RacingUI.SetSpeedMultiplier(1);
                _view.WorldRoot.RacingWorldView.SetPlaybackSpeedMultiplier(1);

                // Racingフェーズに遷移
                TransitionToPhase(RacePhase.Racing);

                // 結果と再生用データをWorldに渡す
                _view.WorldRoot.RacingWorldView.StartRace(output, participants);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"レース実行エラー: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void HandleRaceCompleted()
        {
            TransitionToPhase(RacePhase.Result);
        }

        private void HandleSpeedMultiplierChanged(int multiplier)
        {
            _view.WorldRoot.RacingWorldView.SetPlaybackSpeedMultiplier(multiplier);
        }

        private void HandleBackToHome()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Home);
        }

        private void HandleBackFromCourseSelect()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Home);
        }

        private CourseSelectUI.CourseOption[] BuildCourseOptions()
        {
            if (_masterDataCatalog.Races == null || _masterDataCatalog.Races.Count == 0)
            {
                return Array.Empty<CourseSelectUI.CourseOption>();
            }

            var options = new List<CourseSelectUI.CourseOption>(_masterDataCatalog.Races.Count);
            for (var i = 0; i < _masterDataCatalog.Races.Count; i++)
            {
                var race = _masterDataCatalog.Races[i];
                if (race == null || race.course == null)
                {
                    continue;
                }

                var label = string.IsNullOrWhiteSpace(race.raceName)
                    ? race.raceId
                    : race.raceName;
                options.Add(new CourseSelectUI.CourseOption(race.raceId, new CourseId(race.course.courseId), label));
            }

            return options.ToArray();
        }

        private RaceDefinitionSO FindRaceById(string raceId)
        {
            for (var i = 0; i < _masterDataCatalog.Races.Count; i++)
            {
                var race = _masterDataCatalog.Races[i];
                if (race == null)
                {
                    continue;
                }

                if (string.Equals(race.raceId, raceId, StringComparison.Ordinal))
                {
                    return race;
                }
            }

            return null;
        }

        private MonsterSnapshot[] BuildRaceParticipants(
            RaceDefinitionSO raceDefinition,
            out MonsterId playerRaceMonsterId,
            out Dictionary<MonsterId, string> displayNameByMonsterId)
        {
            var list = new List<MonsterSnapshot>();
            displayNameByMonsterId = new Dictionary<MonsterId, string>();
            playerRaceMonsterId = default;

            var registered = raceDefinition.BuildParticipants();
            for (var i = 0; i < registered.Length; i++)
            {
                var snapshot = ToSnapshot(registered[i]);
                list.Add(snapshot);

                var displayName = string.IsNullOrWhiteSpace(registered[i].Nickname)
                    ? registered[i].MonsterId.Value
                    : registered[i].Nickname;
                displayNameByMonsterId[snapshot.MonsterId] = displayName;
            }

            if (_gameSession.HasWorld && _gameSession.State != null && _gameSession.State.CurrentMonster != null)
            {
                var player = _gameSession.State.CurrentMonster;
                var playerId = player.MonsterId;
                var duplicated = false;

                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].MonsterId == playerId)
                    {
                        duplicated = true;
                        break;
                    }
                }

                if (duplicated)
                {
                    playerId = new MonsterId(player.MonsterId.Value + "_player");
                }

                list.Add(ToSnapshot(player, playerId));
                playerRaceMonsterId = playerId;

                var playerDisplayName = string.IsNullOrWhiteSpace(player.Nickname)
                    ? $"{player.MonsterId.Value} (Player)"
                    : $"{player.Nickname} (Player)";
                displayNameByMonsterId[playerId] = playerDisplayName;
            }

            return list.ToArray();
        }

        private string[] BuildTop3Lines(RaceResult result)
        {
            if (result == null || result.FinishOrder == null || result.FinishOrder.Length == 0)
            {
                return new[]
                {
                    "1位: -",
                    "2位: -",
                    "3位: -"
                };
            }

            var ranking = BuildDisplayRankingTop3(result);
            var lines = new[]
            {
                "1位: -",
                "2位: -",
                "3位: -"
            };

            var count = Math.Min(3, ranking.Count);
            for (var i = 0; i < count; i++)
            {
                var monsterId = ranking[i];
                if (_lastDisplayNameByMonsterId.TryGetValue(monsterId, out var displayName))
                {
                    lines[i] = $"{i + 1}位: {displayName}";
                }
                else
                {
                    lines[i] = $"{i + 1}位: {monsterId.Value}";
                }
            }

            return lines;
        }

        private static List<MonsterId> BuildDisplayRankingTop3(RaceResult result)
        {
            var ranking = new List<MonsterId>(capacity: 3);
            var used = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < result.FinishOrder.Length && ranking.Count < 3; i++)
            {
                var id = result.FinishOrder[i];
                if (id == null)
                    continue;
                if (used.Add(id.Value))
                    ranking.Add(id);
            }

            if (ranking.Count >= 3)
                return ranking;

            if (result.Samples == null || result.Samples.Length == 0)
                return ranking;

            var lastSample = result.Samples[result.Samples.Length - 1];
            if (lastSample == null || lastSample.Runners == null)
                return ranking;

            var orderedByDistance = lastSample.Runners
                .OrderByDescending(r => r.DistanceMeters)
                .ThenBy(r => r.LaneIndex);

            foreach (var runner in orderedByDistance)
            {
                if (runner?.MonsterId == null)
                    continue;

                if (used.Add(runner.MonsterId.Value))
                {
                    ranking.Add(runner.MonsterId);
                    if (ranking.Count >= 3)
                        break;
                }
            }

            return ranking;
        }

        private static MonsterSnapshot ToSnapshot(MonsterInstance monster)
        {
            return ToSnapshot(monster, monster.MonsterId);
        }

        private static MonsterSnapshot ToSnapshot(MonsterInstance monster, MonsterId monsterId)
        {
            var stats = monster.CurrentStats;
            var skillIds = new SkillId[monster.MonsterSkills.Length];
            for (var i = 0; i < monster.MonsterSkills.Length; i++)
            {
                skillIds[i] = monster.MonsterSkills[i].Id;
            }

            return new MonsterSnapshot(
                monsterId,
                monster.SpeciesId,
                new RaceStats(stats.TopSpeed, stats.Accel, stats.Stamina),
                skillIds);
        }
    }

    public enum RacePhase
    {
        CourseSelect,
        Racing,
        Result
    }
}