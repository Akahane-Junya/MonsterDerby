using System;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Training
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class TrainingView : MonoBehaviour, IScreenView
    {
        private UIDocument _uiDocument;
        private Button _backButton;

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
            if (_backButton == null)
                throw new InvalidOperationException("TrainingView: 'backButton' が見つかりません。");

            _backButton.clicked += HandleBackClicked;
        }

        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
        }
    }
}