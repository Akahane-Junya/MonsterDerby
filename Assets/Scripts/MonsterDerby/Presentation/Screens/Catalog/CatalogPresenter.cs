
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using MonsterDerby.Presentation.Navigation;

namespace MonsterDerby.Presentation.Screens.Catalog
{
    /// <summary>
    /// Catalog画面のPresenter（モンスター・スキル切替タブ付き）
    /// </summary>
    public sealed class CatalogPresenter : IScreenPresenter
    {
        private CatalogView _view;
        private enum TabKind { Monster, Skill }
        private TabKind _currentTab = TabKind.Monster;

        // 仮データ（本来はリポジトリ/ScriptableObjectから取得）
        private class CatalogEntry { public string Name; public string Description; public bool IsUnlocked; }
        private List<CatalogEntry> _monsterEntries = new List<CatalogEntry> {
            new CatalogEntry { Name = "Slime", Description = "A basic monster.", IsUnlocked = true },
            new CatalogEntry { Name = "Dragon", Description = "A rare dragon.", IsUnlocked = false },
        };
        private List<CatalogEntry> _skillEntries = new List<CatalogEntry> {
            new CatalogEntry { Name = "Fireball", Description = "Deals fire damage.", IsUnlocked = true },
            new CatalogEntry { Name = "Heal", Description = "Restores HP.", IsUnlocked = false },
        };

        public void BindView(IScreenView view)
        {
            _view = view as CatalogView;
            if (_view == null) return;
            _view.OnMonsterTabClicked += () => SwitchTab(TabKind.Monster);
            _view.OnSkillTabClicked += () => SwitchTab(TabKind.Skill);
            _view.ItemList.selectionChanged += OnItemSelected;
            SwitchTab(_currentTab);
        }

        public void Show() { }
        public void Hide() { }

        private void SwitchTab(TabKind tab)
        {
            _currentTab = tab;
            if (_view == null) return;
            if (tab == TabKind.Monster)
            {
                _view.ItemList.itemsSource = _monsterEntries;
            }
            else
            {
                _view.ItemList.itemsSource = _skillEntries;
            }
            _view.ItemList.makeItem = () => new Label();
            _view.ItemList.bindItem = (e, i) =>
            {
                var entry = (_currentTab == TabKind.Monster ? _monsterEntries : _skillEntries)[i];
                (e as Label).text = entry.IsUnlocked ? entry.Name : "???";
            };
            _view.ItemList.Rebuild();
            if (_view.ItemList.itemsSource.Count > 0)
                _view.ItemList.selectedIndex = 0;
        }

        private void OnItemSelected(IEnumerable<object> selected)
        {
            var entry = selected?.FirstOrDefault() as CatalogEntry;
            if (entry == null || _view == null) return;
            _view.DetailPanel.Clear();
            var title = new Label(entry.IsUnlocked ? entry.Name : "???") { name = "detailTitle" };
            var desc = new Label(entry.IsUnlocked ? entry.Description : "未解禁です") { name = "detailDescription" };
            var unlock = new Label(entry.IsUnlocked ? "解禁済み" : "未解禁") { name = "detailUnlockStatus" };
            _view.DetailPanel.Add(title);
            _view.DetailPanel.Add(desc);
            _view.DetailPanel.Add(unlock);
        }
    }
}
