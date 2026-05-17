using System;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Settings
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SettingsView : MonoBehaviour, IScreenView
    {
        private UIDocument _uiDocument;
        // UI Toolkit要素
        private Label _titleLabel;
        private SliderInt _bgmSlider;
        private Label _bgmLabel;
        private SliderInt _seSlider;
        private Label _seLabel;
        private Toggle _fullscreenToggle;
        private Label _fullscreenLabel;
        private DropdownField _resolutionDropdown;
        private Label _resolutionLabel;
        private DropdownField _windowModeDropdown;
        private Label _windowModeLabel;
        private DropdownField _languageDropdown;
        private Label _languageLabel;
        private Button _keyConfigButton;
        private Button _resetButton;
        private Button _applyButton;
        private Button _closeButton;

        public event Action OnBackClicked;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
                throw new InvalidOperationException("UIDocument コンポーネントが見つかりません。");
        }

        private void OnEnable()
        {
            var root = _uiDocument.rootVisualElement;

            // UI要素取得（UXML側でidを設定しておくこと）
            _titleLabel = root.Q<Label>("titleLabel");
            _bgmSlider = root.Q<SliderInt>("bgmSlider");
            _bgmLabel = root.Q<Label>("bgmLabel");
            _seSlider = root.Q<SliderInt>("seSlider");
            _seLabel = root.Q<Label>("seLabel");
            _fullscreenToggle = root.Q<Toggle>("fullscreenToggle");
            _fullscreenLabel = root.Q<Label>("fullscreenLabel");
            _resolutionDropdown = root.Q<DropdownField>("resolutionDropdown");
            _resolutionLabel = root.Q<Label>("resolutionLabel");
            _windowModeDropdown = root.Q<DropdownField>("windowModeDropdown");
            _windowModeLabel = root.Q<Label>("windowModeLabel");
            _languageDropdown = root.Q<DropdownField>("languageDropdown");
            _languageLabel = root.Q<Label>("languageLabel");
            _keyConfigButton = root.Q<Button>("keyConfigButton");
            _resetButton = root.Q<Button>("resetButton");
            _applyButton = root.Q<Button>("applyButton");
            _closeButton = root.Q<Button>("closeButton");

            // ラベルや初期値
            if (_titleLabel != null) _titleLabel.text = "設定";
            if (_bgmLabel != null) _bgmLabel.text = "BGM音量";
            if (_seLabel != null) _seLabel.text = "SE音量";
            if (_fullscreenLabel != null) _fullscreenLabel.text = "フルスクリーン";
            if (_resolutionLabel != null) _resolutionLabel.text = "解像度";
            if (_windowModeLabel != null) _windowModeLabel.text = "ウィンドウモード";
            if (_languageLabel != null) _languageLabel.text = "言語";

            // ドロップダウン例
            _resolutionDropdown?.choices?.Clear();
            _resolutionDropdown?.choices?.AddRange(new System.Collections.Generic.List<string> { "1920x1080", "1280x720" });
            _windowModeDropdown?.choices?.Clear();
            _windowModeDropdown?.choices?.AddRange(new System.Collections.Generic.List<string> { "フルスクリーン", "ウィンドウ", "ボーダレス" });
            _languageDropdown?.choices?.Clear();
            _languageDropdown?.choices?.AddRange(new System.Collections.Generic.List<string> { "日本語", "English" });

            // イベントバインド（ダミー）
            _bgmSlider?.RegisterValueChangedCallback(evt => { /* 音量変更 */ });
            _seSlider?.RegisterValueChangedCallback(evt => { /* 音量変更 */ });
            _fullscreenToggle?.RegisterValueChangedCallback(evt => { /* フルスクリーン切替 */ });
            _resolutionDropdown?.RegisterValueChangedCallback(evt => { /* 解像度変更 */ });
            _windowModeDropdown?.RegisterValueChangedCallback(evt => { /* ウィンドウモード変更 */ });
            _languageDropdown?.RegisterValueChangedCallback(evt => { /* 言語変更 */ });
            _keyConfigButton?.RegisterCallback<ClickEvent>(evt => { /* キーコンフィグ */ });
            _resetButton?.RegisterCallback<ClickEvent>(evt => { /* リセット */ });
            _applyButton?.RegisterCallback<ClickEvent>(evt => { /* 適用 */ });
            _closeButton?.RegisterCallback<ClickEvent>(evt => {
                OnBackClicked?.Invoke();
            });
        }
    }
}