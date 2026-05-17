using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Race.UI
{
    /// <summary>
    /// リザルト表示UI
    /// </summary>
    public sealed class ResultUI
    {
        private readonly VisualElement _root;
        private readonly Button _backHomeButton;
        private readonly Label _resultLabel;

        public event Action OnBackToHome;

        public ResultUI(VisualElement root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));

            _backHomeButton = _root.Q<Button>("BackHomeButton");
            if (_backHomeButton == null)
                throw new InvalidOperationException("BackHomeButton が見つかりません。UXML を確認してください。");

            _resultLabel = _root.Q<Label>("ResultLabel");

            _backHomeButton.clicked += () => OnBackToHome?.Invoke();
        }

        public void SetActive(bool active)
        {
            _root.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void DisplayTop3(IReadOnlyList<string> lines)
        {
            if (_resultLabel == null)
                return;

            if (lines == null || lines.Count == 0)
            {
                _resultLabel.text = "結果なし";
                return;
            }

            _resultLabel.text = string.Join("\n", lines);
        }
    }
}