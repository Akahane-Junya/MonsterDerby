using System;
using System.Collections.Generic;
using MonsterDerby.Application.Context;
using MonsterDerby.Application.UseCases;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation.Screens.Breeding
{
    public sealed class BreedingPresenter : IScreenPresenter
    {
        private const int CandidateCount = 3;
        private const int RequiredBreedingLevel = 10;
        private const string ReadyHintMessage = "選ぶと卵候補が表示されます";

        private readonly INavigationContext _navigationContext;
        private readonly IBreedingContext _breedingContext;
        private BreedingView _view;

        private BreedingBaseOption[] _baseOptions = Array.Empty<BreedingBaseOption>();
        private BreedingEggCandidate[] _eggCandidates = Array.Empty<BreedingEggCandidate>();

        public BreedingPresenter(
            INavigationContext navigationContext,
            IBreedingContext breedingContext)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
            _breedingContext = breedingContext ?? throw new ArgumentNullException(nameof(breedingContext));
        }

        public void BindView(IScreenView view)
        {
            _view = view as BreedingView ?? throw new ArgumentException("BreedingView が必要です。", nameof(view));
            _view.OnBackClicked += HandleBackClicked;
            _view.OnBaseMonsterSelected += HandleBaseMonsterSelected;
            _view.OnEggCandidateSelected += HandleEggCandidateSelected;
            _view.OnEggModalCanceled += HandleEggModalCanceled;
        }

        public void Show()
        {
            if (!_breedingContext.BreedingSessionUseCase.TryGetCurrentMonster(out var current))
            {
                _view.SetCurrentMonsterSummary("現在のモンスター: なし");
                _view.SetBaseMonsterOptions(Array.Empty<string>());
                _view.HideEggModal();
                return;
            }

            var currentStats = current.CurrentStats;
            var currentSummary = $"現在: {current.Nickname} ({current.SpeciesId.Value}) Lv{current.Level.Value} / SPD:{currentStats.TopSpeed} ACC:{currentStats.Accel} STA:{currentStats.Stamina}";
            _view.SetCurrentMonsterSummary(currentSummary);
            _view.SetBreedingHint(BuildBreedingHint(current.Level.Value));

            _baseOptions = ToArray(_breedingContext.PlanBreedingCandidatesUseCase.GetBaseOptions(current.SpeciesId));
            _view.SetBaseMonsterOptions(BuildBaseOptionLabels());
            _view.HideEggModal();
        }

        public void Hide()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBackClicked;
                _view.OnBaseMonsterSelected -= HandleBaseMonsterSelected;
                _view.OnEggCandidateSelected -= HandleEggCandidateSelected;
                _view.OnEggModalCanceled -= HandleEggModalCanceled;
            }
        }

        private void HandleBackClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Home);
        }

        private void HandleBaseMonsterSelected(int selectedIndex)
        {
            if (!_breedingContext.BreedingSessionUseCase.TryGetCurrentMonster(out var current))
                return;

            if (selectedIndex < 0 || selectedIndex >= _baseOptions.Length)
                return;

            if (current.Level.Value < RequiredBreedingLevel)
            {
                _eggCandidates = Array.Empty<BreedingEggCandidate>();
                return;
            }

            var baseOption = _baseOptions[selectedIndex];
            _eggCandidates = ToArray(_breedingContext.PlanBreedingCandidatesUseCase.BuildEggCandidates(current, baseOption));
            _view.ShowEggModal(BuildEggCandidateLabels());
        }

        private void HandleEggCandidateSelected(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _eggCandidates.Length)
                return;

            var selectedChild = _eggCandidates[selectedIndex].Child;
            _breedingContext.BreedingSessionUseCase.ApplySelectedChild(selectedChild);

            _view.HideEggModal();
            Show();
        }

        private void HandleEggModalCanceled()
        {
            _view.HideEggModal();
        }

        private string[] BuildBaseOptionLabels()
        {
            var labels = new string[CandidateCount];
            for (int i = 0; i < CandidateCount; i++)
            {
                if (i >= _baseOptions.Length)
                {
                    labels[i] = string.Empty;
                    continue;
                }

                var option = _baseOptions[i];
                var stats = option.Growth.CalculateStats(new Experience(0));
                labels[i] = $"{option.SpeciesName}\nID:{option.SpeciesId.Value}\nSPD:{stats.TopSpeed} ACC:{stats.Accel} STA:{stats.Stamina}";
            }

            return labels;
        }

        private string[] BuildEggCandidateLabels()
        {
            var labels = new string[CandidateCount];
            for (int i = 0; i < CandidateCount; i++)
            {
                labels[i] = i < _eggCandidates.Length ? BuildEggLabel(_eggCandidates[i]) : string.Empty;
            }

            return labels;
        }

        private static string BuildEggLabel(BreedingEggCandidate candidate)
        {
            var child = candidate.Child;
            var stats = child.CurrentStats;
            return $"{candidate.KindLabel}\n種族:{child.SpeciesId.Value} Lv{child.Level.Value}\nSPD:{stats.TopSpeed} ACC:{stats.Accel} STA:{stats.Stamina}\nSkill:{child.MonsterSkills.Length} 親:{candidate.ParentAName}/{candidate.ParentBName}";
        }

        private static string BuildBreedingHint(int currentLevel)
        {
            return currentLevel < RequiredBreedingLevel
                ? $"Lv{RequiredBreedingLevel}で解禁（現在Lv{currentLevel}）"
                : ReadyHintMessage;
        }

        private static T[] ToArray<T>(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
                return Array.Empty<T>();

            var result = new T[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                result[i] = source[i];
            }

            return result;
        }
    }
}