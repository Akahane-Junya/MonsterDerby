using System;
using MonsterDerby.Presentation.Animation.Race;
using UnityEngine;

namespace MonsterDerby.Presentation.Screens.Status
{
    /// <summary>
    /// Status画面用のモンスター表示コントローラ。
    /// SpriteLibraryAsset + MotionSet から Ready/Idle を SpriteRenderer で再生する。
    /// </summary>
    public sealed class StatusMonsterPreviewController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private bool _showWorldSprite = false;
        [SerializeField] private string _sortingLayerName = "Default";
        [SerializeField] private int _sortingOrder = 5000;

        public event Action<Sprite> OnFrameSpriteChanged;

        private UnityEngine.Object _spriteLibraryAsset;
        private SpriteMotionClip _currentClip;
        private int _frameIndex;
        private float _frameTimer;
        private bool _isAnimating;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingLayerName = _sortingLayerName;
                _spriteRenderer.sortingOrder = _sortingOrder;
                _spriteRenderer.enabled = _showWorldSprite;
            }
        }

        public void Configure(UnityEngine.Object spriteLibraryAsset, SpriteMotionSet motionSet)
        {
            if (_spriteRenderer == null)
                throw new InvalidOperationException("SpriteRenderer が見つかりません。");
            if (spriteLibraryAsset == null)
                throw new ArgumentNullException(nameof(spriteLibraryAsset), "spriteLibraryAsset が未設定です。");
            if (motionSet == null)
                throw new ArgumentNullException(nameof(motionSet), "motionSet が未設定です。");

            var typeName = spriteLibraryAsset.GetType().FullName;
            if (typeName != "UnityEngine.U2D.Animation.SpriteLibraryAsset")
                throw new InvalidOperationException(
                    $"spriteLibraryAsset の型が正しくありません。期待値: UnityEngine.U2D.Animation.SpriteLibraryAsset、実際: {typeName}");

            _spriteLibraryAsset = spriteLibraryAsset;
            StartMotionAnimation(motionSet);
        }

        private void Update()
        {
            if (!_isAnimating || _currentClip == null)
                return;

            if (!_currentClip.IsValid || _currentClip.Labels.Length <= 1)
                return;

            var frameDuration = 1f / _currentClip.Fps;
            _frameTimer += Time.deltaTime;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;

                if (_currentClip.Loop)
                {
                    _frameIndex = (_frameIndex + 1) % _currentClip.Labels.Length;
                }
                else
                {
                    if (_frameIndex >= _currentClip.Labels.Length - 1)
                    {
                        _isAnimating = false;
                        return;
                    }

                    _frameIndex++;
                }

                ApplyFrame(_currentClip, _frameIndex);
            }
        }

        private void StartMotionAnimation(SpriteMotionSet motionSet)
        {
            var clip = motionSet.UseReadyWhenNotRunning
                ? motionSet.GetClip(SpriteMotionType.Ready)
                : motionSet.GetClip(SpriteMotionType.Idle);

            if (clip == null || !clip.IsValid || clip.Labels.Length == 0)
                throw new InvalidOperationException("Ready/Idle クリップが無効です。MotionSet を確認してください。");

            _currentClip = clip;
            _frameIndex = 0;
            _frameTimer = 0f;
            _isAnimating = true;

            ApplyFrame(_currentClip, _frameIndex);
        }

        private void ApplyFrame(SpriteMotionClip clip, int frameIndex)
        {
            if (clip == null)
                throw new ArgumentNullException(nameof(clip));
            if (frameIndex < 0 || frameIndex >= clip.Labels.Length)
                throw new ArgumentOutOfRangeException(nameof(frameIndex));
            if (_spriteLibraryAsset == null)
                throw new InvalidOperationException("SpriteLibraryAsset が設定されていません。");

            var getSpriteMethod = _spriteLibraryAsset.GetType().GetMethod(
                "GetSprite",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(string) },
                null);
            
            if (getSpriteMethod == null)
                throw new InvalidOperationException(
                    $"SpriteLibraryAsset.GetSprite(string, string) メソッドが見つかりません。型: {_spriteLibraryAsset.GetType().FullName}");

            try
            {
                var sprite = getSpriteMethod.Invoke(_spriteLibraryAsset, new object[] { clip.Category, clip.Labels[frameIndex] }) as Sprite;
                if (sprite == null)
                    throw new InvalidOperationException(
                        $"category='{clip.Category}', label='{clip.Labels[frameIndex]}' のスプライトが見つかりません。");

                OnFrameSpriteChanged?.Invoke(sprite);

                if (_showWorldSprite)
                {
                    _spriteRenderer.sprite = sprite;
                    _spriteRenderer.enabled = true;
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new InvalidOperationException(
                    $"GetSprite({clip.Category}, {clip.Labels[frameIndex]}) 実行中にエラー: {ex.InnerException?.Message}", ex.InnerException);
            }
        }
    }
}
