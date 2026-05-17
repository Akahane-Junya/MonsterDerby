using System;
using System.Collections.Generic;
using MonsterDerby.Application.Game;
using MonsterDerby.Application.Context;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.MasterData;
using MonsterDerby.Domain.Records;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;

namespace MonsterDerby.Presentation.Screens.Awards
{
    public sealed class AwardsPresenter : IScreenPresenter
    {
        private const string NoMedalLabel = "メダルなし";
        private const string NoRecordDisplay = "59:59.99 / ---";

        private readonly INavigationContext _navigationContext;
        private readonly GameSession _gameSession;
        private readonly MasterDataCatalog _masterDataCatalog;
        private AwardsView _view;

        public AwardsPresenter(
            INavigationContext navigationContext,
            GameSession gameSession,
            MasterDataCatalog masterDataCatalog)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            _masterDataCatalog = masterDataCatalog ?? throw new ArgumentNullException(nameof(masterDataCatalog));
        }

        public void BindView(IScreenView view)
        {
            _view = view as AwardsView ?? throw new ArgumentException("AwardsView が必要です。", nameof(view));
            _view.OnBackClicked += HandleBackClicked;
        }

        public void Show()
        {
            if (!_gameSession.HasWorld || _gameSession.State == null)
            {
                _view.Render(Array.Empty<AwardsTrophyRow>());
                return;
            }

            var world = _gameSession.State;
            var rows = BuildRows(world.AwardEntries);
            _view.Render(rows);
        }

        public void Hide()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBackClicked;
            }
        }

        private void HandleBackClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Home);
        }

        private List<AwardsTrophyRow> BuildRows(IReadOnlyList<RaceAwardState> awardEntries)
        {
            var rows = new List<AwardsTrophyRow>();
            var awardByRaceId = new Dictionary<string, RaceAwardState>();
            if (awardEntries != null)
            {
                for (int i = 0; i < awardEntries.Count; i++)
                {
                    var award = awardEntries[i];
                    if (award == null) continue;
                    awardByRaceId[award.RaceId] = award;
                }
            }

            for (int i = 0; i < _masterDataCatalog.Races.Count; i++)
            {
                var race = _masterDataCatalog.Races[i];
                if (race == null || string.IsNullOrWhiteSpace(race.raceId))
                    continue;

                var raceLabel = ResolveRaceLabel(race.raceId);
                if (awardByRaceId.TryGetValue(race.raceId, out var award) && award.TrophyOwnership != null)
                {
                    var owned = award.TrophyOwnership;
                    var medalLabel = ResolveMedalLabel(owned.Medal);
                    var trophyDetail = $"{medalLabel} / {owned.WinnerMonsterId.Value}";
                    var recordDetail = BuildRecordDetail(award);
                    rows.Add(new AwardsTrophyRow(
                        raceLabel,
                        trophyDetail,
                        recordDetail,
                        ResolveMedalImage(race, owned.Medal)));
                }
                else
                {
                    rows.Add(new AwardsTrophyRow(
                        raceLabel,
                        NoMedalLabel,
                        BuildRecordDetail(awardByRaceId.TryGetValue(race.raceId, out var noTrophyAward) ? noTrophyAward : null),
                        null));
                }
            }

            return rows;
        }

        private static string BuildRecordDetail(RaceAwardState award)
        {
            if (award == null || award.CourseRecord == null)
            {
                return NoRecordDisplay;
            }

            var record = award.CourseRecord;
            var clampedMs = Mathf.Clamp(record.Time.Value, 0, (59 * 60 * 1000) + (59 * 1000) + 990);
            var minutes = clampedMs / 60000;
            var seconds = (clampedMs % 60000) / 1000;
            var centiseconds = (clampedMs % 1000) / 10;

            return $"{minutes:00}:{seconds:00}.{centiseconds:00} / {record.HolderMonsterId.Value}";
        }

        private string ResolveRaceLabel(string raceId)
        {
            for (int i = 0; i < _masterDataCatalog.Races.Count; i++)
            {
                var race = _masterDataCatalog.Races[i];
                if (race == null)
                    continue;

                if (!string.Equals(race.raceId, raceId, StringComparison.Ordinal))
                    continue;

                if (!string.IsNullOrWhiteSpace(race.raceName))
                    return race.raceName;

                if (!string.IsNullOrWhiteSpace(race.raceId))
                    return race.raceId;
            }

            return raceId;
        }

        private static Sprite ResolveMedalImage(RaceDefinitionSO race, TrophyMedal medal)
        {
            if (race == null)
                return null;

            switch (medal)
            {
                case TrophyMedal.None:
                    return null;
                case TrophyMedal.Gold:
                    return race.goldMedalImage;
                case TrophyMedal.Silver:
                    return race.silverMedalImage;
                case TrophyMedal.Bronze:
                    return race.bronzeMedalImage;
                default:
                    return null;
            }
        }

        private static string ResolveMedalLabel(TrophyMedal medal)
        {
            switch (medal)
            {
                case TrophyMedal.None:
                    return NoMedalLabel;
                case TrophyMedal.Gold:
                    return "金";
                case TrophyMedal.Silver:
                    return "銀";
                case TrophyMedal.Bronze:
                    return "銅";
                default:
                    return medal.ToString();
            }
        }
    }
}