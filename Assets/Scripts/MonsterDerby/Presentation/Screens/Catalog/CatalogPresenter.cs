
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation.Screens.Catalog
{
    /// <summary>
    /// Catalog画面のPresenter（モンスター・スキル切替タブ付き）
    /// </summary>

    using MonsterDerby.Application.Context;


    using MonsterDerby.Domain.Catalog;
    using MonsterDerby.Domain.Monster;
    using UnityEngine;

    public sealed class CatalogPresenter : IScreenPresenter
    {
        private CatalogView _view;
        private readonly INavigationContext _navigationContext;
        private readonly MonsterDefinitionRepositoryAsset _monsterRepository;
        private enum TabKind { Monster, Skill }
        private TabKind _currentTab = TabKind.Monster;

        private List<MonsterDefinitionSO> _monsterDefs;

        public CatalogPresenter(
            INavigationContext navigationContext,
            MonsterDefinitionRepositoryAsset monsterRepository)
        {
            _navigationContext = navigationContext;
            _monsterRepository = monsterRepository;
            _monsterDefs = _monsterRepository.GetAll() != null ? new List<MonsterDefinitionSO>(_monsterRepository.GetAll()) : new List<MonsterDefinitionSO>();
        }

        public void BindView(IScreenView view)
        {
            _view = view as CatalogView;
            if (_view == null) return;
            _view.OnMonsterTabClicked += () => SwitchTab(TabKind.Monster);
            _view.OnSkillTabClicked += () => SwitchTab(TabKind.Skill);
            _view.ItemList.selectionChanged += OnItemSelected;
            var closeButton = (_view.GetType().GetProperty("CloseButton")?.GetValue(_view) as Button) ?? _view.GetComponent<UIDocument>()?.rootVisualElement?.Q<Button>("closeButton");
            if (closeButton != null)
                closeButton.clicked += () => NavigationToHome();
            SwitchTab(_currentTab);
        }

        private void NavigationToHome()
        {
            _navigationContext.Navigator.NavigateTo(MonsterDerby.Domain.SharedKernel.ScreenId.Home);
        }

        public void Show() { }
        public void Hide() { }

        private void SwitchTab(TabKind tab)
        {
            _currentTab = tab;
            if (_view == null) return;
            if (tab == TabKind.Monster)
            {
                _view.ItemList.itemsSource = _monsterDefs;
                _view.ItemList.makeItem = () => new Label();
                _view.ItemList.bindItem = (e, i) =>
                {
                    var def = _monsterDefs[i];
                    // 仮: 全て未確認扱い
                    (e as Label).text = "???";
                };
            }
            else
            {
                // TODO: スキル図鑑対応
                _view.ItemList.itemsSource = new List<string>();
                _view.ItemList.makeItem = () => new Label();
                _view.ItemList.bindItem = (e, i) => { (e as Label).text = "-"; };
            }
            _view.ItemList.Rebuild();
            if (_view.ItemList.itemsSource.Count > 0)
                _view.ItemList.selectedIndex = 0;
        }

        private void OnItemSelected(IEnumerable<object> selected)
        {
            if (_currentTab == TabKind.Monster)
            {
                var def = selected?.FirstOrDefault() as MonsterDefinitionSO;
                if (def == null || _view == null) return;
                _view.DetailPanel.Clear();
                // タイトル
                var title = new Label("???") { name = "detailTitle" };
                _view.DetailPanel.Add(title);
                // 以降は全て非表示（仮実装）
                var unlock = new Label(stage == CatalogUnlockStage.Raised ? "全情報解放" : stage == CatalogUnlockStage.Encountered ? "出会った" : "???") { name = "detailUnlockStatus" };
                _view.DetailPanel.Add(unlock);
            }
            else
            {
                // TODO: スキル図鑑対応
                _view.DetailPanel.Clear();
                _view.DetailPanel.Add(new Label("未対応"));
            }
        }
    }
}
