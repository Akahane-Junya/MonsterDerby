using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDerby.Application.Context;
using MonsterDerby.Application.Game;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.Repositories;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;

namespace MonsterDerby.Presentation.Screens.Shop
{
    public sealed class ShopPresenter : IScreenPresenter
    {
        private const int MaxExperience = 900;
        private const int MaxSkillCount = 4;
        private const int PriceExpSmall = 100;
        private const int PriceExpMedium = 300;
        private const int PriceExpLarge = 700;
        private const int PriceSkillRandom = 500;
        private const int PriceSkillGuaranteed = 2000;

        private readonly INavigationContext _navigationContext;
        private readonly GameSession _gameSession;
        private readonly ScriptableObjectSkillRepository _skillRepository;
        private ShopView _view;
        private SkillId _guaranteedSkillId;
        private bool _hasGuaranteedSkill;

            private int _pendingSkillPrice;
            private bool _pendingUseGuaranteed;

        public ShopPresenter(
            INavigationContext navigationContext,
            GameSession gameSession,
            ScriptableObjectSkillRepository skillRepository)
        {
            _navigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            _skillRepository = skillRepository ?? throw new ArgumentNullException(nameof(skillRepository));
        }

        public void BindView(IScreenView view)
        {
            _view = view as ShopView ?? throw new ArgumentException("ShopView が必要です。", nameof(view));
            _view.OnBackClicked += HandleBackClicked;
            _view.OnExpSmallClicked += HandleExpSmallClicked;
            _view.OnExpMediumClicked += HandleExpMediumClicked;
            _view.OnExpLargeClicked += HandleExpLargeClicked;
            _view.OnSkillRandomClicked += HandleSkillRandomClicked;
            _view.OnSkillGuaranteedClicked += HandleSkillGuaranteedClicked;
                _view.OnForgetSkillSelected += HandleForgetSkillSelected;
        }

        public void Show()
        {
            RefreshMoneyLabel();
            RefreshGuaranteedSkill();
        }

        public void Hide()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBackClicked;
                _view.OnExpSmallClicked -= HandleExpSmallClicked;
                _view.OnExpMediumClicked -= HandleExpMediumClicked;
                _view.OnExpLargeClicked -= HandleExpLargeClicked;
                _view.OnSkillRandomClicked -= HandleSkillRandomClicked;
                _view.OnSkillGuaranteedClicked -= HandleSkillGuaranteedClicked;
                    _view.OnForgetSkillSelected -= HandleForgetSkillSelected;
            }
        }

        private void HandleBackClicked()
        {
            _navigationContext.Navigator.NavigateTo(ScreenId.Home);
        }

        private void HandleExpSmallClicked()
        {
            BuyExperience(PriceExpSmall, 30, "経験値アップ小");
        }

        private void HandleExpMediumClicked()
        {
            BuyExperience(PriceExpMedium, 100, "経験値アップ中");
        }

        private void HandleExpLargeClicked()
        {
            BuyExperience(PriceExpLarge, 250, "経験値アップ大");
        }

        private void BuyExperience(int price, int deltaExp, string productName)
        {
            if (!TryGetWorld(out var world))
            {
                return;
            }

            if (world.Money < price)
            {
                Debug.Log($"[Shop] 購入失敗: {productName} / 所持金不足 (必要:{price}, 所持:{world.Money})");
                return;
            }

            var monster = world.CurrentMonster;
            var before = monster.Experience.Value;
            var after = Math.Min(before + deltaExp, MaxExperience);
            var gained = after - before;

            var updatedMonster = monster.WithExperience(new Experience(after));
            var updatedWorld = world.With(world.Money - price, updatedMonster);
            _gameSession.Apply(_ => updatedWorld);

            RefreshMoneyLabel();
            Debug.Log($"[Shop] 購入成功: {productName} / 経験値 +{gained} ({before} -> {after}) / 所持金 {world.Money} -> {updatedWorld.Money}");
        }

        private void HandleSkillRandomClicked()
        {
            BuySkill(PriceSkillRandom, false);
        }

        private void HandleSkillGuaranteedClicked()
        {
            BuySkill(PriceSkillGuaranteed, true);
        }

        private void BuySkill(int price, bool useGuaranteed)
        {
            if (!TryGetWorld(out var world))
            {
                return;
            }

            if (world.Money < price)
            {
                Debug.Log($"[Shop] 購入失敗: スキル習得 / 所持金不足 (必要:{price}, 所持:{world.Money})");
                return;
            }

            var monster = world.CurrentMonster;
                // 4スキルの場合は忘却UIを表示
                if (monster.MonsterSkills.Length >= MaxSkillCount)
                {
                    ShowForgetUI(monster, price, useGuaranteed);
                    return;
                }

            var candidates = BuildUnlearnedSkillCandidates(monster);
            if (candidates.Count == 0)
            {
                Debug.Log("[Shop] 購入失敗: 未習得スキルがありません。");
                return;
            }

            SkillId selectedSkill;
            if (useGuaranteed)
            {
                if (!_hasGuaranteedSkill)
                {
                    RefreshGuaranteedSkill();
                }

                if (!_hasGuaranteedSkill || !ContainsSkillId(candidates, _guaranteedSkillId))
                {
                    Debug.Log("[Shop] 購入失敗: 確定スキルが現在選べません。画面を開き直してください。");
                    return;
                }

                selectedSkill = _guaranteedSkillId;
            }
            else
            {
                var index = UnityEngine.Random.Range(0, candidates.Count);
                selectedSkill = candidates[index];
            }

            var updatedSkills = new MonsterSkill[monster.MonsterSkills.Length + 1];
            Array.Copy(monster.MonsterSkills, updatedSkills, monster.MonsterSkills.Length);
            updatedSkills[updatedSkills.Length - 1] = new MonsterSkill(selectedSkill, monster.Level);

            var updatedMonster = new MonsterInstance(
                monster.MonsterId,
                monster.SpeciesId,
                monster.Nickname,
                monster.Experience,
                monster.GrowthIncrements,
                updatedSkills,
                monster.ParentMonsterIds);

            var updatedWorld = world.With(world.Money - price, updatedMonster);
            _gameSession.Apply(_ => updatedWorld);

            RefreshMoneyLabel();
            Debug.Log($"[Shop] 購入成功: スキル習得 / 取得:{selectedSkill.Value} / 所持金 {world.Money} -> {updatedWorld.Money}");

            RefreshGuaranteedSkill();
        }

        private void RefreshMoneyLabel()
        {
            var money = 0;
            if (_gameSession.HasWorld && _gameSession.State != null)
            {
                money = _gameSession.State.Money;
            }

            _view.SetMoney(money);
        }

        private void RefreshGuaranteedSkill()
        {
            if (!TryGetWorld(out var world))
            {
                _hasGuaranteedSkill = false;
                _view.SetGuaranteedSkillLabel("なし");
                return;
            }

            var candidates = BuildUnlearnedSkillCandidates(world.CurrentMonster);
            if (candidates.Count == 0)
            {
                _hasGuaranteedSkill = false;
                _view.SetGuaranteedSkillLabel("なし");
                return;
            }

            var index = UnityEngine.Random.Range(0, candidates.Count);
            _guaranteedSkillId = candidates[index];
            _hasGuaranteedSkill = true;
            _view.SetGuaranteedSkillLabel(_guaranteedSkillId.Value);
        }

        private List<SkillId> BuildUnlearnedSkillCandidates(MonsterInstance monster)
        {
            var learned = new HashSet<string>(monster.MonsterSkills.Select(s => s.Id.Value), StringComparer.Ordinal);
            var allSkillIds = _skillRepository.GetAllSkillIds();

            return allSkillIds
                .Where(skillId => !learned.Contains(skillId.Value))
                .ToList();
        }

        private bool TryGetWorld(out MonsterDerby.Domain.World.WorldState world)
        {
            world = null;
            if (!_gameSession.HasWorld || _gameSession.State == null || _gameSession.State.CurrentMonster == null)
            {
                Debug.Log("[Shop] ワールド状態が未初期化です。");
                return false;
            }

            world = _gameSession.State;
            return true;
        }

        private static bool ContainsSkillId(List<SkillId> candidates, SkillId target)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] == target)
                {
                    return true;
                }
            }

            return false;
        }

            private void ShowForgetUI(MonsterInstance monster, int price, bool useGuaranteed)
            {
                _pendingSkillPrice = price;
                _pendingUseGuaranteed = useGuaranteed;

                var skillNames = new string[4];
                var descriptions = new string[4];
                var levels = new int[4];

                for (int i = 0; i < 4; i++)
                {
                    var skill = monster.MonsterSkills[i];
                    var skillDef = _skillRepository.GetSkillDefinition(skill.Id);
                    var skillSO = _skillRepository.TryGetSO(skill.Id.Value);

                    skillNames[i] = skillDef.Name;
                    descriptions[i] = skillSO?.description ?? "説明なし";
                    levels[i] = skill.UnlockLevel.Value;
                }

                _view.ShowForgetModal(skillNames, descriptions, levels);
            }

            private void HandleForgetSkillSelected(int forgetIndex)
            {
                if (!TryGetWorld(out var world))
                {
                    return;
                }

                var monster = world.CurrentMonster;

                var candidates = BuildUnlearnedSkillCandidates(monster);
                if (candidates.Count == 0)
                {
                    Debug.Log("[Shop] 購入失敗: 未習得スキルがありません。");
                    return;
                }

                SkillId selectedSkill;
                if (_pendingUseGuaranteed)
                {
                    if (!_hasGuaranteedSkill)
                    {
                        RefreshGuaranteedSkill();
                    }

                    if (!_hasGuaranteedSkill || !ContainsSkillId(candidates, _guaranteedSkillId))
                    {
                        Debug.Log("[Shop] 購入失敗: 確定スキルが現在選べません。画面を開き直してください。");
                        return;
                    }

                    selectedSkill = _guaranteedSkillId;
                }
                else
                {
                    var index = UnityEngine.Random.Range(0, candidates.Count);
                    selectedSkill = candidates[index];
                }

                // 忘却スキルを削除、新スキルを追加
                var updatedSkills = new MonsterSkill[4];
                int newSkillIndex = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (i != forgetIndex)
                    {
                        updatedSkills[newSkillIndex] = monster.MonsterSkills[i];
                        newSkillIndex++;
                    }
                }
                updatedSkills[newSkillIndex] = new MonsterSkill(selectedSkill, monster.Level);

                var updatedMonster = new MonsterInstance(
                    monster.MonsterId,
                    monster.SpeciesId,
                    monster.Nickname,
                    monster.Experience,
                    monster.GrowthIncrements,
                    updatedSkills,
                    monster.ParentMonsterIds);

                var updatedWorld = world.With(world.Money - _pendingSkillPrice, updatedMonster);
                _gameSession.Apply(_ => updatedWorld);

                RefreshMoneyLabel();
                Debug.Log($"[Shop] 購入成功: スキル習得(忘却置き換え) / 忘却: {monster.MonsterSkills[forgetIndex].Id.Value} / 取得:{selectedSkill.Value} / 所持金 {world.Money} -> {updatedWorld.Money}");

                RefreshGuaranteedSkill();
            }
    }
}