using System;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Shop
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class ShopView : MonoBehaviour, IScreenView
    {
        private UIDocument _uiDocument;
        private Label _moneyLabel;
        private Button _backButton;
        private Button _expSmallButton;
        private Button _expMediumButton;
        private Button _expLargeButton;
        private Button _skillRandomButton;
        private Button _skillGuaranteedButton;

        public event Action OnBackClicked;
        public event Action OnExpSmallClicked;
        public event Action OnExpMediumClicked;
        public event Action OnExpLargeClicked;
        public event Action OnSkillRandomClicked;
        public event Action OnSkillGuaranteedClicked;

            public event Action<int> OnForgetSkillSelected;

            private VisualElement _forgetModal;
            private Button[] _forgetSkillButtons;
            private Label[] _forgetSkillNames;
            private Label[] _forgetSkillDescs;
            private Label[] _forgetSkillLevels;
            private Button _forgetCancelButton;
            private readonly Action[] _forgetSkillButtonHandlers = new Action[4];

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
            _moneyLabel = root.Q<Label>("moneyLabel");
            if (_moneyLabel == null)
                throw new InvalidOperationException("ShopView: 'moneyLabel' が見つかりません。");

            _backButton = root.Q<Button>("backButton");
            if (_backButton == null)
                throw new InvalidOperationException("ShopView: 'backButton' が見つかりません。");

            _expSmallButton = root.Q<Button>("expSmallButton");
            _expMediumButton = root.Q<Button>("expMediumButton");
            _expLargeButton = root.Q<Button>("expLargeButton");
            _skillRandomButton = root.Q<Button>("skillRandomButton");
            _skillGuaranteedButton = root.Q<Button>("skillGuaranteedButton");

            if (_expSmallButton == null)
                throw new InvalidOperationException("ShopView: 'expSmallButton' が見つかりません。");
            if (_expMediumButton == null)
                throw new InvalidOperationException("ShopView: 'expMediumButton' が見つかりません。");
            if (_expLargeButton == null)
                throw new InvalidOperationException("ShopView: 'expLargeButton' が見つかりません。");
            if (_skillRandomButton == null)
                throw new InvalidOperationException("ShopView: 'skillRandomButton' が見つかりません。");
            if (_skillGuaranteedButton == null)
                throw new InvalidOperationException("ShopView: 'skillGuaranteedButton' が見つかりません。");

            _backButton.clicked += HandleBackClicked;
            _expSmallButton.clicked += HandleExpSmallClicked;
            _expMediumButton.clicked += HandleExpMediumClicked;
            _expLargeButton.clicked += HandleExpLargeClicked;
            _skillRandomButton.clicked += HandleSkillRandomClicked;
            _skillGuaranteedButton.clicked += HandleSkillGuaranteedClicked;

                // 忘却UI要素の取得
                _forgetModal = root.Q<VisualElement>("forgetModal");
                if (_forgetModal == null)
                    throw new InvalidOperationException("ShopView: 'forgetModal' が見つかりません。");

                _forgetSkillButtons = new Button[4];
                _forgetSkillNames = new Label[4];
                _forgetSkillDescs = new Label[4];
                _forgetSkillLevels = new Label[4];

                for (int i = 0; i < 4; i++)
                {
                    _forgetSkillButtons[i] = root.Q<Button>($"forgetSkillButton{i}");
                    _forgetSkillNames[i] = root.Q<Label>($"forgetSkillName{i}");
                    _forgetSkillDescs[i] = root.Q<Label>($"forgetSkillDesc{i}");
                    _forgetSkillLevels[i] = root.Q<Label>($"forgetSkillLevel{i}");

                    if (_forgetSkillButtons[i] == null)
                        throw new InvalidOperationException($"ShopView: 'forgetSkillButton{i}' が見つかりません。");

                    int index = i;
                    _forgetSkillButtonHandlers[i] = () => HandleForgetSkillClicked(index);
                    _forgetSkillButtons[i].clicked += _forgetSkillButtonHandlers[i];
                }

                _forgetCancelButton = root.Q<Button>("forgetCancelButton");
                if (_forgetCancelButton == null)
                    throw new InvalidOperationException("ShopView: 'forgetCancelButton' が見つかりません。");

                _forgetCancelButton.clicked += HideForgetModal;
        }

        private void OnDisable()
        {
            if (_backButton != null)
            {
                _backButton.clicked -= HandleBackClicked;
            }

            if (_expSmallButton != null)
            {
                _expSmallButton.clicked -= HandleExpSmallClicked;
            }

            if (_expMediumButton != null)
            {
                _expMediumButton.clicked -= HandleExpMediumClicked;
            }

            if (_expLargeButton != null)
            {
                _expLargeButton.clicked -= HandleExpLargeClicked;
            }

            if (_skillRandomButton != null)
            {
                _skillRandomButton.clicked -= HandleSkillRandomClicked;
            }

            if (_skillGuaranteedButton != null)
            {
                _skillGuaranteedButton.clicked -= HandleSkillGuaranteedClicked;
            }

                if (_forgetCancelButton != null)
                {
                    _forgetCancelButton.clicked -= HideForgetModal;
                }

                if (_forgetSkillButtons != null)
                {
                    for (int i = 0; i < _forgetSkillButtons.Length; i++)
                    {
                        if (_forgetSkillButtons[i] != null && _forgetSkillButtonHandlers[i] != null)
                        {
                            _forgetSkillButtons[i].clicked -= _forgetSkillButtonHandlers[i];
                        }
                    }
                }
        }

        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
        }

        private void HandleExpSmallClicked()
        {
            OnExpSmallClicked?.Invoke();
        }

        private void HandleExpMediumClicked()
        {
            OnExpMediumClicked?.Invoke();
        }

        private void HandleExpLargeClicked()
        {
            OnExpLargeClicked?.Invoke();
        }

        private void HandleSkillRandomClicked()
        {
            OnSkillRandomClicked?.Invoke();
        }

        private void HandleSkillGuaranteedClicked()
        {
            OnSkillGuaranteedClicked?.Invoke();
        }

        public void SetMoney(int money)
        {
            if (_moneyLabel == null)
                return;

            _moneyLabel.text = $"所持金: {money}";
        }

        public void SetGuaranteedSkillLabel(string skillId)
        {
            if (_skillGuaranteedButton == null)
                return;

            _skillGuaranteedButton.text = $"スキル習得(確定) (価格:2000) [{skillId}]";
        }

            public void ShowForgetModal(string[] skillNames, string[] descriptions, int[] levels)
            {
                if (_forgetModal == null)
                    return;

                for (int i = 0; i < 4 && i < skillNames.Length; i++)
                {
                    _forgetSkillNames[i].text = skillNames[i];
                    _forgetSkillDescs[i].text = descriptions[i];
                    _forgetSkillLevels[i].text = $"Lv{levels[i]}で習得";
                }

                _forgetModal.style.display = DisplayStyle.Flex;
            }

            public void HideForgetModal()
            {
                if (_forgetModal == null)
                    return;

                _forgetModal.style.display = DisplayStyle.None;
            }

            private void HandleForgetSkillClicked(int index)
            {
                OnForgetSkillSelected?.Invoke(index);
                HideForgetModal();
            }
    }
}