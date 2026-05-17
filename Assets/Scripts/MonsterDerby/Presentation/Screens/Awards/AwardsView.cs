using System;
using System.Collections.Generic;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Awards
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class AwardsView : MonoBehaviour, IScreenView
    {
        private UIDocument _uiDocument;
        private Button _backButton;
        private VisualElement _trophyList;
        private IReadOnlyList<AwardsTrophyRow> _pendingTrophyRows;

        public event Action OnBackClicked;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                throw new InvalidOperationException("UIDocument コンポーネントが見つかりません。");
            }
        }

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;
            _backButton = root.Q<Button>("backButton");
            _trophyList = root.Q<VisualElement>("trophyList");
            if (_backButton == null)
                throw new InvalidOperationException("AwardsView: 'backButton' が見つかりません。");
            if (_trophyList == null)
                throw new InvalidOperationException("AwardsView: 'trophyList' が見つかりません。");

            _backButton.clicked += HandleBackClicked;

            if (_pendingTrophyRows != null)
            {
                Render(_pendingTrophyRows);
                _pendingTrophyRows = null;
            }
        }

        public void Render(IReadOnlyList<AwardsTrophyRow> trophyRows)
        {
            if (_trophyList == null)
            {
                _pendingTrophyRows = trophyRows;
                return;
            }

            _trophyList.Clear();

            if (trophyRows == null || trophyRows.Count == 0)
            {
                var emptyTrophy = new Label("レース記録はまだありません。");
                emptyTrophy.style.color = new StyleColor(new Color(0.97f, 0.9f, 0.68f, 1f));
                _trophyList.Add(emptyTrophy);
            }
            else
            {
                for (int i = 0; i < trophyRows.Count; i++)
                    _trophyList.Add(CreateTrophyRow(trophyRows[i]));
            }
        }

        private static VisualElement CreateTrophyRow(AwardsTrophyRow trophyRow)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.FlexStart;
            row.style.marginBottom = 8;
            row.style.paddingLeft = 6;
            row.style.paddingTop = 5;
            row.style.paddingRight = 6;
            row.style.paddingBottom = 5;
            row.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.14f));
            row.style.borderTopLeftRadius = 5;
            row.style.borderTopRightRadius = 5;
            row.style.borderBottomLeftRadius = 5;
            row.style.borderBottomRightRadius = 5;

            if (trophyRow.MedalImage != null)
            {
                var icon = new VisualElement();
                icon.style.width = 48;
                icon.style.height = 48;
                icon.style.marginRight = 10;
                icon.style.flexShrink = 0;
                icon.style.backgroundImage = new StyleBackground(trophyRow.MedalImage);
                icon.style.unityBackgroundImageTintColor = Color.white;
                row.Add(icon);
            }

            var textBlock = new VisualElement();
            textBlock.style.flexDirection = FlexDirection.Column;
            textBlock.style.flexGrow = 1;

            var raceLabel = new Label(trophyRow.RaceLabel);
            raceLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            raceLabel.style.marginBottom = 2;
            raceLabel.style.color = new StyleColor(new Color(0.97f, 0.9f, 0.68f, 1f));

            var trophyLabel = new Label($"トロフィー: {trophyRow.TrophyDetail}");
            trophyLabel.style.whiteSpace = WhiteSpace.Normal;
            trophyLabel.style.color = new StyleColor(new Color(0.9f, 0.83f, 0.62f, 1f));
            trophyLabel.style.marginBottom = 1;

            var recordLabel = new Label($"レコード: {trophyRow.RecordDetail}");
            recordLabel.style.whiteSpace = WhiteSpace.Normal;
            recordLabel.style.color = new StyleColor(new Color(0.77f, 0.97f, 0.82f, 1f));

            textBlock.Add(raceLabel);
            textBlock.Add(trophyLabel);
            textBlock.Add(recordLabel);
            row.Add(textBlock);

            return row;
        }

        private void OnDisable()
        {
            if (_backButton != null)
            {
                _backButton.clicked -= HandleBackClicked;
            }
        }

        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
        }
    }
}