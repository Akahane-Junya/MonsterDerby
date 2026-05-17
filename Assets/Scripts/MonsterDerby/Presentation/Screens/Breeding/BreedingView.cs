using System;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Breeding
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class BreedingView : MonoBehaviour, IScreenView
    {
        private const int CandidateCount = 3;

        private UIDocument _uiDocument;
        private Button _backButton;
        private Label _currentMonsterLabel;
        private Label _selectHintLabel;
        private Button[] _baseMonsterButtons;
        private Button[] _eggCandidateButtons;
        private VisualElement _eggModal;
        private Button _eggModalCancelButton;
        private Label _eggModalSubTitle;

        private readonly Action[] _baseButtonHandlers = new Action[CandidateCount];
        private readonly Action[] _eggButtonHandlers = new Action[CandidateCount];

        public event Action OnBackClicked;
        public event Action<int> OnBaseMonsterSelected;
        public event Action<int> OnEggCandidateSelected;
        public event Action OnEggModalCanceled;

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
            _currentMonsterLabel = root.Q<Label>("currentMonsterLabel");
            _selectHintLabel = root.Q<Label>("selectHintLabel");
            _eggModal = root.Q<VisualElement>("eggModal");
            _eggModalCancelButton = root.Q<Button>("eggModalCancelButton");
            _eggModalSubTitle = root.Q<Label>("eggModalSubTitle");

            if (_backButton == null)
                throw new InvalidOperationException("BreedingView: 'backButton' が見つかりません。");
            if (_currentMonsterLabel == null)
                throw new InvalidOperationException("BreedingView: 'currentMonsterLabel' が見つかりません。");
            if (_selectHintLabel == null)
                throw new InvalidOperationException("BreedingView: 'selectHintLabel' が見つかりません。");
            if (_eggModal == null)
                throw new InvalidOperationException("BreedingView: 'eggModal' が見つかりません。");
            if (_eggModalCancelButton == null)
                throw new InvalidOperationException("BreedingView: 'eggModalCancelButton' が見つかりません。");
            if (_eggModalSubTitle == null)
                throw new InvalidOperationException("BreedingView: 'eggModalSubTitle' が見つかりません。");

            _baseMonsterButtons = new Button[CandidateCount];
            _eggCandidateButtons = new Button[CandidateCount];
            for (int i = 0; i < CandidateCount; i++)
            {
                _baseMonsterButtons[i] = root.Q<Button>($"baseMonsterButton{i}");
                _eggCandidateButtons[i] = root.Q<Button>($"eggCandidateButton{i}");
                if (_baseMonsterButtons[i] == null)
                    throw new InvalidOperationException($"BreedingView: 'baseMonsterButton{i}' が見つかりません。");
                if (_eggCandidateButtons[i] == null)
                    throw new InvalidOperationException($"BreedingView: 'eggCandidateButton{i}' が見つかりません。");

                var index = i;
                _baseButtonHandlers[i] = () => OnBaseMonsterSelected?.Invoke(index);
                _eggButtonHandlers[i] = () => OnEggCandidateSelected?.Invoke(index);
                _baseMonsterButtons[i].clicked += _baseButtonHandlers[i];
                _eggCandidateButtons[i].clicked += _eggButtonHandlers[i];
            }

            _backButton.clicked += HandleBackClicked;
            _eggModalCancelButton.clicked += HandleEggModalCanceled;
            HideEggModal();
        }

        private void OnDisable()
        {
            if (_backButton != null)
                _backButton.clicked -= HandleBackClicked;

            if (_eggModalCancelButton != null)
                _eggModalCancelButton.clicked -= HandleEggModalCanceled;

            if (_baseMonsterButtons != null)
            {
                for (int i = 0; i < _baseMonsterButtons.Length; i++)
                {
                    if (_baseMonsterButtons[i] != null && _baseButtonHandlers[i] != null)
                        _baseMonsterButtons[i].clicked -= _baseButtonHandlers[i];
                }
            }

            if (_eggCandidateButtons != null)
            {
                for (int i = 0; i < _eggCandidateButtons.Length; i++)
                {
                    if (_eggCandidateButtons[i] != null && _eggButtonHandlers[i] != null)
                        _eggCandidateButtons[i].clicked -= _eggButtonHandlers[i];
                }
            }
        }

        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
        }

        private void HandleEggModalCanceled()
        {
            OnEggModalCanceled?.Invoke();
        }

        public void SetCurrentMonsterSummary(string summary)
        {
            if (_currentMonsterLabel == null)
                return;

            _currentMonsterLabel.text = string.IsNullOrWhiteSpace(summary) ? "現在のモンスター: なし" : summary;
        }

        public void SetBaseMonsterOptions(string[] optionLabels)
        {
            if (_baseMonsterButtons == null)
                return;

            for (int i = 0; i < _baseMonsterButtons.Length; i++)
            {
                var label = optionLabels != null && i < optionLabels.Length ? optionLabels[i] : string.Empty;
                _baseMonsterButtons[i].text = string.IsNullOrWhiteSpace(label) ? "候補なし" : label;
                _baseMonsterButtons[i].SetEnabled(!string.IsNullOrWhiteSpace(label));
            }
        }

        public void SetBreedingHint(string message)
        {
            if (_selectHintLabel == null)
                return;

            _selectHintLabel.text = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
        }

        public void ShowEggModal(string[] candidateLabels)
        {
            if (_eggCandidateButtons == null || _eggModal == null)
                return;

            SetEggModalSubTitle("レベル1で誕生します");
            for (int i = 0; i < _eggCandidateButtons.Length; i++)
            {
                var label = candidateLabels != null && i < candidateLabels.Length ? candidateLabels[i] : string.Empty;
                _eggCandidateButtons[i].text = string.IsNullOrWhiteSpace(label) ? "候補なし" : label;
                _eggCandidateButtons[i].SetEnabled(!string.IsNullOrWhiteSpace(label));
            }

            _eggModal.style.display = DisplayStyle.Flex;
        }

        public void HideEggModal()
        {
            if (_eggModal == null)
                return;

            _eggModal.style.display = DisplayStyle.None;
        }

        private void SetEggModalSubTitle(string text)
        {
            if (_eggModalSubTitle == null)
                return;

            _eggModalSubTitle.text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
        }
    }
}