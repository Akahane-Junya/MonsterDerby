using System;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Race.UI
{
    /// <summary>
    /// レース中のUI制御
    /// </summary>
    public sealed class RacingUI
    {
        private readonly VisualElement _root;
        private readonly Button _speedDownButton;
        private readonly Button _speedUpButton;
        private readonly Label _speedLabel;
        private int _speedMultiplier;

        public event Action<int> OnSpeedMultiplierChanged;

        public RacingUI(VisualElement root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));

            _speedDownButton = _root.Q<Button>("speedDownButton");
            _speedUpButton = _root.Q<Button>("speedUpButton");
            _speedLabel = _root.Q<Label>("speedLabel");
            if (_speedDownButton == null)
                throw new InvalidOperationException("RacingUI: 'speedDownButton' が見つかりません。");
            if (_speedUpButton == null)
                throw new InvalidOperationException("RacingUI: 'speedUpButton' が見つかりません。");
            if (_speedLabel == null)
                throw new InvalidOperationException("RacingUI: 'speedLabel' が見つかりません。");

            _speedDownButton.clicked += HandleSpeedDownClicked;
            _speedUpButton.clicked += HandleSpeedUpClicked;

            SetSpeedMultiplier(1);
        }

        public void SetActive(bool active)
        {
            _root.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetSpeedMultiplier(int multiplier)
        {
            _speedMultiplier = ClampMultiplier(multiplier);
            UpdateLabel();
            UpdateButtonState();
        }

        private void HandleSpeedDownClicked()
        {
            if (_speedMultiplier <= 1)
                return;

            _speedMultiplier--;
            UpdateLabel();
            UpdateButtonState();
            OnSpeedMultiplierChanged?.Invoke(_speedMultiplier);
        }

        private void HandleSpeedUpClicked()
        {
            if (_speedMultiplier >= 3)
                return;

            _speedMultiplier++;
            UpdateLabel();
            UpdateButtonState();
            OnSpeedMultiplierChanged?.Invoke(_speedMultiplier);
        }

        private void UpdateLabel()
        {
            _speedLabel.text = $"SPEED: <<×{_speedMultiplier}>>";
        }

        private void UpdateButtonState()
        {
            _speedDownButton.SetEnabled(_speedMultiplier > 1);
            _speedUpButton.SetEnabled(_speedMultiplier < 3);
        }

        private static int ClampMultiplier(int multiplier)
        {
            if (multiplier < 1)
                return 1;
            if (multiplier > 3)
                return 3;
            return multiplier;
        }
    }
}