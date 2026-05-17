using System;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Home
{
    /// <summary>
    /// Home画面のView (MonoBehaviour)
    /// UI要素の取得とイベント発行を担当
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HomeView : MonoBehaviour, IScreenView
    {
        private UIDocument _uiDocument;
        private VisualElement _root;

        // UI要素
        private Button _trainingButton;
        private Button _shopButton;
        private Button _breedingButton;
        private Button _raceButton;
        private Button _statusButton;
        private Button _awardsButton;
        private Button _settingsButton;

        // イベント（Presenterが購読）
        public event Action OnTrainingClicked;
        public event Action OnShopClicked;
        public event Action OnBreedingClicked;
        public event Action OnRaceClicked;
        public event Action OnStatusClicked;
        public event Action OnAwardsClicked;
        public event Action OnSettingsClicked;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
                throw new InvalidOperationException("UIDocument コンポーネントが見つかりません。");
        }

        private void OnEnable()
        {
            BindUI();
        }

        private void OnDisable()
        {
            UnbindUI();
        }

        /// <summary>
        /// UI要素を取得してイベントをバインド
        /// </summary>
        private void BindUI()
        {
            _root = _uiDocument.rootVisualElement;

            // ボタン取得
            _trainingButton = FindButton("trainingButton");
            _shopButton = FindButton("shopButton");
            _breedingButton = FindButton("breedingButton");
            _raceButton = FindButton("raceButton");
            _statusButton = FindButton("statusButton");
            _awardsButton = FindButton("awardsButton");
            _settingsButton = FindButton("settingsButton");

            if (_trainingButton == null) throw new InvalidOperationException("HomeView: 'trainingButton/screenButton' が見つかりません。");
            if (_shopButton == null) throw new InvalidOperationException("HomeView: 'shopButton/screenButton' が見つかりません。");
            if (_breedingButton == null) throw new InvalidOperationException("HomeView: 'breedingButton/screenButton' が見つかりません。");
            if (_raceButton == null) throw new InvalidOperationException("HomeView: 'raceButton/screenButton' が見つかりません。");
            if (_statusButton == null) throw new InvalidOperationException("HomeView: 'statusButton/screenButton' が見つかりません。");
            if (_awardsButton == null) throw new InvalidOperationException("HomeView: 'awardsButton/screenButton' が見つかりません。");
            if (_settingsButton == null) throw new InvalidOperationException("HomeView: 'settingsButton/screenButton' が見つかりません。");

            // イベント登録
            _trainingButton.clicked += HandleTrainingClicked;
            _shopButton.clicked += HandleShopClicked;
            _breedingButton.clicked += HandleBreedingClicked;
            _raceButton.clicked += HandleRaceClicked;
            _statusButton.clicked += HandleStatusClicked;
            _awardsButton.clicked += HandleAwardsClicked;
            _settingsButton.clicked += HandleSettingsClicked;
        }

        /// <summary>
        /// イベントを解除
        /// </summary>
        private void UnbindUI()
        {
            if (_trainingButton != null) _trainingButton.clicked -= HandleTrainingClicked;
            if (_shopButton != null) _shopButton.clicked -= HandleShopClicked;
            if (_breedingButton != null) _breedingButton.clicked -= HandleBreedingClicked;
            if (_raceButton != null) _raceButton.clicked -= HandleRaceClicked;
            if (_statusButton != null) _statusButton.clicked -= HandleStatusClicked;
            if (_awardsButton != null) _awardsButton.clicked -= HandleAwardsClicked;
            if (_settingsButton != null) _settingsButton.clicked -= HandleSettingsClicked;
        }

        private void HandleTrainingClicked() => OnTrainingClicked?.Invoke();
        private void HandleShopClicked() => OnShopClicked?.Invoke();
        private void HandleBreedingClicked() => OnBreedingClicked?.Invoke();
        private void HandleRaceClicked() => OnRaceClicked?.Invoke();
        private void HandleStatusClicked() => OnStatusClicked?.Invoke();
        private void HandleAwardsClicked() => OnAwardsClicked?.Invoke();
        private void HandleSettingsClicked() => OnSettingsClicked?.Invoke();

        /// <summary>
        /// ボタンを検索（既存のネスト構造に対応）
        /// </summary>
        private Button FindButton(string rootName)
        {
            // まず直接 rootName という名前の要素を探す
            var element = _root.Q(rootName);
            if (element == null)
            {
                Debug.LogWarning($"HomeView: '{rootName}' が見つかりません。");
                return null;
            }

            // Button として直接見つかれば返す
            var button = element as Button;
            if (button != null)
                return button;

            // VisualElement の場合は、その中から screenButton を探す
            var buttonInside = element.Q<Button>("screenButton");
            if (buttonInside == null)
            {
                Debug.LogWarning($"HomeView: '{rootName}/screenButton' が見つかりません。");
                return null;
            }

            return buttonInside;
        }
    }
}