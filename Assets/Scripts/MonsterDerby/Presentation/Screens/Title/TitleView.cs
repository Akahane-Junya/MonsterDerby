using System;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Title
{
    /// <summary>
    /// Title画面のView
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TitleView : MonoBehaviour, IScreenView
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _startButton;

        // イベント
        public event Action OnStartClicked;
        // ※セッティング画面への遷移導線はTitleViewには設けない

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
                throw new InvalidOperationException("UIDocument が見つかりません。");
        }

        private void OnEnable()
        {
            BindUI();
        }

        private void OnDisable()
        {
            UnbindUI();
        }

        private void BindUI()
        {
            _root = _uiDocument.rootVisualElement;

            _startButton = _root.Q<Button>("startButton");
            if (_startButton == null)
                throw new InvalidOperationException("TitleView: 'startButton' が見つかりません。");

            _startButton.clicked += HandleStartClicked;
        }

        private void UnbindUI()
        {
            if (_startButton != null)
                _startButton.clicked -= HandleStartClicked;
        }

        private void HandleStartClicked()
        {
            OnStartClicked?.Invoke();
        }
    }
}