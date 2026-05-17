using System;
using MonsterDerby.Application.UseCases;
using MonsterDerby.Presentation.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.Screens.Status
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class StatusView : MonoBehaviour, IScreenView
    {
        private static readonly Color LockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color NormalColor = Color.white;

        [Header("Status Character")]
        [SerializeField] private StatusMonsterPreviewController _monsterPreviewControllerPrefab;
        [SerializeField] private Transform _monsterPreviewParent;

        private StatusMonsterPreviewController _monsterPreviewController;

        private UIDocument _uiDocument;
        private Button _backButton;
        private VisualElement _portraitFrame;
        private VisualElement _portraitSpriteLayer;
        private Label _nicknameLabel;
        private Label _speciesLabel;
        private Label _levelLabel;
        private Label _experienceLabel;
        private Label _topSpeedLabel;
        private Label _accelerationLabel;
        private Label _staminaLabel;

        // スキルスロット (4個固定)
        private VisualElement[] _skillSlotRoots;
        private VisualElement[] _skillIcons;
        private Label[] _skillNameLabels;
        private Label[] _skillDescLabels;

        public event Action OnBackClicked;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
                throw new InvalidOperationException("UIDocument コンポーネントが見つかりません。");

            if (_monsterPreviewControllerPrefab == null)
                throw new InvalidOperationException("StatusMonsterPreviewController プレハブが設定されていません。");
            if (_monsterPreviewParent == null)
                throw new InvalidOperationException("StatusMonsterPreview の親 Transform が設定されていません。");
        }

        private void OnEnable()
        {
            EnsurePreviewInstance();

            var root = _uiDocument.rootVisualElement;
            _backButton = root.Q<Button>("backButton");
            _portraitFrame = root.Q<VisualElement>("portraitFrame");
            _portraitSpriteLayer = root.Q<VisualElement>("portraitSpriteLayer");
            _nicknameLabel = root.Q<Label>("nicknameLabel");
            _speciesLabel = root.Q<Label>("speciesLabel");
            _levelLabel = root.Q<Label>("levelLabel");
            _experienceLabel = root.Q<Label>("experienceLabel");
            _topSpeedLabel = root.Q<Label>("topSpeedLabel");
            _accelerationLabel = root.Q<Label>("accelerationLabel");
            _staminaLabel = root.Q<Label>("staminaLabel");

            _skillSlotRoots = new VisualElement[4];
            _skillIcons = new VisualElement[4];
            _skillNameLabels = new Label[4];
            _skillDescLabels = new Label[4];
            for (int i = 0; i < 4; i++)
            {
                _skillSlotRoots[i] = root.Q<VisualElement>($"skillSlot{i}");
                _skillIcons[i] = root.Q<VisualElement>($"skillIcon{i}");
                _skillNameLabels[i] = root.Q<Label>($"skillName{i}");
                _skillDescLabels[i] = root.Q<Label>($"skillDesc{i}");
            }

            if (_backButton != null)
                _backButton.clicked += HandleBackClicked;
            else
                throw new InvalidOperationException("StatusView: 'backButton' が見つかりません。");
        }

        private void OnDisable()
        {
            if (_backButton != null)
                _backButton.clicked -= HandleBackClicked;

            if (_monsterPreviewController != null)
            {
                _monsterPreviewController.OnFrameSpriteChanged -= HandlePreviewSpriteChanged;
                Destroy(_monsterPreviewController.gameObject);
                _monsterPreviewController = null;
            }
        }

        private void EnsurePreviewInstance()
        {
            if (_monsterPreviewController != null)
                return;

            var parent = ResolveRuntimePreviewParent();
            _monsterPreviewController = Instantiate(_monsterPreviewControllerPrefab, parent, false);
            _monsterPreviewController.OnFrameSpriteChanged += HandlePreviewSpriteChanged;
        }

        private Transform ResolveRuntimePreviewParent()
        {
            if (_monsterPreviewParent == null)
                throw new InvalidOperationException("StatusView: MonsterPreviewParent が未設定です。Scene上の Transform を指定してください。");

            // Prefabアセット上のTransformはSceneに属さないため、親指定に使えない。
            if (!_monsterPreviewParent.gameObject.scene.IsValid())
                throw new InvalidOperationException("StatusView: MonsterPreviewParent が Scene オブジェクトではありません。Prefabアセットではなく Scene 上の Transform を指定してください。");

            return _monsterPreviewParent;
        }

        private void HandleBackClicked() => OnBackClicked?.Invoke();

        public void SetMonsterStatus(MonsterStatusDto status)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            SetLabel(_nicknameLabel, $"名前: {status.Nickname}");
            SetLabel(_speciesLabel, $"種族: {status.SpeciesName} ({status.SpeciesId})");
            SetLabel(_levelLabel, $"レベル: {status.Level}");
            SetLabel(_experienceLabel, $"経験値: {status.Experience}");
            SetLabel(_topSpeedLabel, $"TopSpeed: {status.TopSpeed}");
            SetLabel(_accelerationLabel, $"Acceleration: {status.Acceleration}");
            SetLabel(_staminaLabel, $"Stamina: {status.Stamina}");
        }

        public void SetMonsterVisual(StatusMonsterVisualViewData visual)
        {
            if (_monsterPreviewController == null)
                throw new InvalidOperationException("StatusMonsterPreviewController が設定されていません。");

            _monsterPreviewController.Configure(visual.SpriteLibraryAsset, visual.MotionSet);
        }

        private void HandlePreviewSpriteChanged(Sprite sprite)
        {
            if (_portraitFrame == null || _portraitSpriteLayer == null)
            {
                return;
            }

            if (sprite == null)
            {
                _portraitSpriteLayer.style.backgroundImage = StyleKeyword.None;
                return;
            }

            _portraitSpriteLayer.style.backgroundImage = new StyleBackground(sprite);
            _portraitSpriteLayer.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Contain));
            _portraitSpriteLayer.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            _portraitSpriteLayer.style.scale = new StyleScale(new Scale(new Vector3(2f, 2f, 1f)));
            _portraitSpriteLayer.style.translate = new StyleTranslate(new Translate(new Length(0f, LengthUnit.Pixel), new Length(-64f, LengthUnit.Pixel), 0f));
        }

        public void SetSkillSlots(SkillSlotViewData[] slots)
        {
            if (slots == null) return;
            for (int i = 0; i < 4; i++)
            {
                if (i >= slots.Length || slots[i].IsEmpty)
                {
                    ApplyEmptySlot(i);
                    continue;
                }
                var slot = slots[i];
                if (slot.IsUnlocked)
                    ApplyUnlockedSlot(i, slot);
                else
                    ApplyLockedSlot(i, slot);
            }
        }

        private void ApplyUnlockedSlot(int i, SkillSlotViewData slot)
        {
            if (_skillIcons[i] != null)
            {
                _skillIcons[i].style.backgroundImage = slot.Icon != null
                    ? new StyleBackground(slot.Icon) : StyleKeyword.None;
                _skillIcons[i].style.unityBackgroundImageTintColor = new StyleColor(NormalColor);
            }
            SetLabel(_skillNameLabels[i], slot.SkillName, NormalColor);
            SetLabel(_skillDescLabels[i], NormalizeSkillText(slot.Description), NormalColor);
        }

        private void ApplyLockedSlot(int i, SkillSlotViewData slot)
        {
            if (_skillIcons[i] != null)
            {
                _skillIcons[i].style.backgroundImage = slot.Icon != null
                    ? new StyleBackground(slot.Icon) : StyleKeyword.None;
                _skillIcons[i].style.unityBackgroundImageTintColor = new StyleColor(LockedColor);
            }
            SetLabel(_skillNameLabels[i], slot.SkillName, LockedColor);
            SetLabel(_skillDescLabels[i], $"Lv{slot.UnlockLevel}で解放", LockedColor);
        }

        private void ApplyEmptySlot(int i)
        {
            if (_skillIcons[i] != null)
            {
                _skillIcons[i].style.backgroundImage = StyleKeyword.None;
                _skillIcons[i].style.unityBackgroundImageTintColor = StyleKeyword.None;
            }
            SetLabel(_skillNameLabels[i], string.Empty, NormalColor);
            SetLabel(_skillDescLabels[i], string.Empty, NormalColor);
        }

        private static void SetLabel(Label label, string value, Color color = default)
        {
            if (label == null) return;
            label.text = value;
            if (color != default)
                label.style.color = new StyleColor(color);
        }

        private static void SetLabel(Label label, string value)
        {
            if (label != null) label.text = value;
        }

        private static string NormalizeSkillText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var oneLine = text.Replace("\r", " ").Replace("\n", " ").Trim();
            while (oneLine.Contains("  "))
            {
                oneLine = oneLine.Replace("  ", " ");
            }

            const int maxLen = 44;
            if (oneLine.Length > maxLen)
            {
                return oneLine.Substring(0, maxLen - 1) + "…";
            }

            return oneLine;
        }
    }
}
