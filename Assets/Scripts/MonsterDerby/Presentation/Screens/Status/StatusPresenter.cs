using System;
using MonsterDerby.Application.Context;
using MonsterDerby.Application.UseCases;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.Repositories;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;

namespace MonsterDerby.Presentation.Screens.Status
{
    public struct SkillSlotViewData
    {
        public string SkillName;
        public string Description;
        public Sprite Icon;
        public bool IsUnlocked;
        public int UnlockLevel;
        public bool IsEmpty;
    }

    public struct StatusMonsterVisualViewData
    {
        public UnityEngine.Object SpriteLibraryAsset;
        public MonsterDerby.Presentation.Animation.Race.SpriteMotionSet MotionSet;
    }

    public sealed class StatusPresenter : IScreenPresenter
    {
        private const int MaxSkillSlots = 4;

        private readonly INavigationContext _navigationContext;
        private readonly IStatusContext _statusContext;
        private readonly ScriptableObjectMonsterVisualRepository _visualRepository;
        private readonly ScriptableObjectSkillRepository _skillSORepository;
        private StatusView _view;

        public StatusPresenter(
            INavigationContext navigationContext,
            IStatusContext statusContext,
            ScriptableObjectMonsterVisualRepository visualRepository,
            ScriptableObjectSkillRepository skillSORepository)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
            _statusContext = statusContext ?? throw new ArgumentNullException(nameof(statusContext));
            _visualRepository = visualRepository ?? throw new ArgumentNullException(nameof(visualRepository));
            _skillSORepository = skillSORepository;
        }

        public void BindView(IScreenView view)
        {
            _view = view as StatusView ?? throw new ArgumentException("StatusView が必要です。", nameof(view));
            _view.OnBackClicked += HandleBackClicked;
        }

        public void Show()
        {
            var status = _statusContext.GetCurrentMonsterStatusUseCase.Execute();
            _view.SetMonsterStatus(status);

            MonsterDerby.Infrastructure.MasterData.MonsterVisualDefinitionSO visual;
            try
            {
                visual = _visualRepository.GetBySpeciesId(new SpeciesId(status.SpeciesId));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Status表示用のSpeciesId '{status.SpeciesId}' が見つからない、またはVisual解決に失敗しました。", ex);
            }

            _view.SetMonsterVisual(new StatusMonsterVisualViewData
            {
                SpriteLibraryAsset = visual.spriteLibraryAsset,
                MotionSet = visual.motionSet,
            });

            var slotViews = new SkillSlotViewData[MaxSkillSlots];
            for (int i = 0; i < MaxSkillSlots; i++)
            {
                if (status.SkillSlots == null || i >= status.SkillSlots.Length)
                {
                    slotViews[i] = new SkillSlotViewData { IsEmpty = true };
                    continue;
                }

                var slot = status.SkillSlots[i];
                if (slot.IsEmpty)
                {
                    slotViews[i] = new SkillSlotViewData { IsEmpty = true };
                    continue;
                }

                if (string.IsNullOrWhiteSpace(slot.SkillId))
                    throw new InvalidOperationException($"Status表示用スキルスロット{i}のSkillIdが空です。IsEmpty=false のデータ不整合です。");

                var so = _skillSORepository?.TryGetSO(slot.SkillId);
                if (so == null)
                    throw new InvalidOperationException($"Status表示用のSkillId '{slot.SkillId}' に対応する SkillDefinitionSO が見つかりません。");

                slotViews[i] = new SkillSlotViewData
                {
                    SkillName = slot.SkillName,
                    Description = so.description,
                    Icon = so.icon,
                    IsUnlocked = slot.IsUnlocked,
                    UnlockLevel = slot.UnlockLevel,
                    IsEmpty = slot.IsEmpty,
                };
            }
            _view.SetSkillSlots(slotViews);
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
    }
}