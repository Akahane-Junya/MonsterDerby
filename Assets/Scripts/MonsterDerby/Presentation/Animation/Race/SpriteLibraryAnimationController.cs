using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// Sprite Library Asset のカテゴリ/ラベルを使ってレース中モーションを再生する。
    /// </summary>
    public sealed class SpriteLibraryAnimationController : MonoBehaviour, IRaceRunnerAnimationController
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteMotionSet _motionSet;

        [Header("Behavior")]
        [SerializeField] private float _damageFlashDurationSeconds = 0.12f;
        [SerializeField] private float _damageShakeDurationSeconds = 0.18f;
        [SerializeField] private float _damageShakeAmplitudePx = 1.5f;
        [SerializeField] private float _damageShakeFrequencyHz = 35f;
        [SerializeField] private Color _damageFlashColor = new Color(1.8f, 1.8f, 1.8f, 1f);

        private UnityEngine.Object _spriteLibraryAsset;
        private readonly HashSet<string> _warnedKeys = new();

        private SpriteMotionType _baseMotion = SpriteMotionType.Run;
        private SpriteMotionType _currentMotion = SpriteMotionType.Run;
        private int _frameIndex;
        private float _frameTimer;
        private bool _damageEffectActive;
        private float _damageEffectStartSeconds;
        private float _damageFlashUntilSeconds;
        private float _damageShakeUntilSeconds;

        private bool _hasLastRaceTime;
        private float _lastRaceTime;
        private Transform _shakeTarget;
        private Vector3 _defaultShakeLocalPosition;
        private Color _defaultColor = Color.white;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _shakeTarget = _spriteRenderer != null ? _spriteRenderer.transform : null;
            if (_shakeTarget != null && _shakeTarget != transform)
            {
                _defaultShakeLocalPosition = _shakeTarget.localPosition;
            }
            if (_spriteRenderer != null)
            {
                _defaultColor = _spriteRenderer.color;
            }
        }

        /// <summary>
        /// SpriteLibraryAsset を注入し、初期モーション再生を開始。
        /// </summary>
        public void SetSpriteLibraryAsset(UnityEngine.Object asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset), "SpriteLibraryAsset が未設定です。");

            var typeName = asset.GetType().FullName;
            if (typeName != "UnityEngine.U2D.Animation.SpriteLibraryAsset")
                throw new InvalidOperationException(
                    $"asset の型が正しくありません。期待値: UnityEngine.U2D.Animation.SpriteLibraryAsset、実際: {typeName}");

            _spriteLibraryAsset = asset;
        }

        public void SetRunState(bool isRunning, float raceTimeSeconds)
        {
            _baseMotion = isRunning ? SpriteMotionType.Run : ResolveWaitingMotion();
            SwitchTo(_baseMotion, force: false);
            Tick(raceTimeSeconds);
        }

        public void Tick(float raceTimeSeconds)
        {
            float delta = 0f;
            if (_hasLastRaceTime)
            {
                delta = Mathf.Max(0f, raceTimeSeconds - _lastRaceTime);
            }

            _hasLastRaceTime = true;
            _lastRaceTime = raceTimeSeconds;

            Advance(delta);
            UpdateDamageEffect(raceTimeSeconds);
        }

        public void PlaySkillOverlay(float raceTimeSeconds)
        {
            RaceTransientEffect.SpawnAttackEffect(transform);
        }

        public void PlayDamageOverlay(float raceTimeSeconds)
        {
            _damageEffectActive = true;
            _damageEffectStartSeconds = raceTimeSeconds;
            _damageFlashUntilSeconds = raceTimeSeconds + Mathf.Max(0.01f, _damageFlashDurationSeconds);
            _damageShakeUntilSeconds = raceTimeSeconds + Mathf.Max(0.01f, _damageShakeDurationSeconds);
            Tick(raceTimeSeconds);
        }

        public void SetMotionSet(SpriteMotionSet motionSet)
        {
            _motionSet = motionSet ?? throw new ArgumentNullException(nameof(motionSet));
            if (_spriteLibraryAsset == null)
                throw new InvalidOperationException("SpriteLibraryAsset が未設定です。SetSpriteLibraryAsset を先に呼び出してください。");

            _baseMotion = SpriteMotionType.Run;
            if (!IsClipValid(_baseMotion))
            {
                _baseMotion = FindFirstValidMotion();
            }

            SwitchTo(_baseMotion, force: true);
        }

        private void Advance(float deltaSeconds)
        {
            var clip = GetClip(_currentMotion);
            if (clip == null || !clip.IsValid)
            {
                return;
            }

            if (clip.Labels.Length <= 1)
            {
                return;
            }

            float frameDuration = 1f / clip.Fps;
            _frameTimer += deltaSeconds;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;

                if (clip.Loop)
                {
                    _frameIndex = (_frameIndex + 1) % clip.Labels.Length;
                }
                else
                {
                    if (_frameIndex < clip.Labels.Length - 1)
                    {
                        _frameIndex++;
                    }
                }

                ApplyCurrentFrame(clip);

                if (!clip.Loop && _frameIndex >= clip.Labels.Length - 1)
                {
                    SwitchTo(_baseMotion, force: true);
                    break;
                }
            }
        }

        private void SwitchTo(SpriteMotionType motion, bool force)
        {
            if (!force && _currentMotion == motion)
            {
                return;
            }

            var clip = GetClip(motion);
            if (clip == null || !clip.IsValid)
            {
                WarnOnce($"clip:{motion}", $"SpriteLibraryAnimationController: MotionSet の {motion} クリップが無効です。");
                return;
            }

            _currentMotion = motion;
            _frameIndex = 0;
            _frameTimer = 0f;
            ApplyCurrentFrame(clip);
        }

        private void ApplyCurrentFrame(SpriteMotionClip clip)
        {
            if (_spriteLibraryAsset == null)
            {
                WarnOnce("asset:null", "SpriteLibraryAsset が設定されていません。");
                return;
            }

            var getSpriteMethod = _spriteLibraryAsset.GetType().GetMethod(
                "GetSprite",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(string) },
                null);

            if (getSpriteMethod == null)
            {
                WarnOnce("method:GetSprite", $"GetSprite(string, string) が見つかりません。型: {_spriteLibraryAsset.GetType().FullName}");
                return;
            }

            try
            {
                var sprite = getSpriteMethod.Invoke(_spriteLibraryAsset, new object[] { clip.Category, clip.Labels[_frameIndex] }) as Sprite;
                if (sprite == null)
                {
                    WarnOnce(
                        $"sprite:{clip.Category}:{clip.Labels[_frameIndex]}",
                        $"category='{clip.Category}', label='{clip.Labels[_frameIndex]}' のスプライトが見つかりません。");
                    return;
                }

                _spriteRenderer.sprite = sprite;
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                WarnOnce(
                    $"error:{clip.Category}:{clip.Labels[_frameIndex]}",
                    $"GetSprite({clip.Category}, {clip.Labels[_frameIndex]}) エラー: {ex.InnerException?.Message}");
            }
        }

        private SpriteMotionClip GetClip(SpriteMotionType motion)
        {
            return _motionSet.GetClip(motion);
        }

        private bool IsClipValid(SpriteMotionType motion)
        {
            var clip = GetClip(motion);
            return clip != null && clip.IsValid;
        }

        private SpriteMotionType FindFirstValidMotion()
        {
            var candidates = new[]
            {
                SpriteMotionType.Run,
                SpriteMotionType.Ready,
                SpriteMotionType.Idle,
                SpriteMotionType.Attack,
                SpriteMotionType.Death,
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                if (IsClipValid(candidates[i]))
                {
                    return candidates[i];
                }
            }

            return SpriteMotionType.Run;
        }

        private SpriteMotionType ResolveWaitingMotion()
        {
            if (IsClipValid(SpriteMotionType.Ready))
            {
                return SpriteMotionType.Ready;
            }

            if (IsClipValid(SpriteMotionType.Idle))
            {
                return SpriteMotionType.Idle;
            }

            return SpriteMotionType.Run;
        }

        private void WarnOnce(string key, string message)
        {
            if (_warnedKeys.Contains(key))
            {
                return;
            }

            _warnedKeys.Add(key);
            Debug.LogWarning(message, this);
        }

        private void UpdateDamageEffect(float raceTimeSeconds)
        {
            if (!_damageEffectActive)
            {
                return;
            }

            if (_spriteRenderer != null)
            {
                if (raceTimeSeconds <= _damageFlashUntilSeconds)
                {
                    float duration = Mathf.Max(0.0001f, _damageFlashUntilSeconds - _damageEffectStartSeconds);
                    float t = 1f - Mathf.Clamp01((raceTimeSeconds - _damageEffectStartSeconds) / duration);
                    _spriteRenderer.color = Color.Lerp(_defaultColor, _damageFlashColor, t);
                }
                else
                {
                    _spriteRenderer.color = _defaultColor;
                }
            }

            if (_shakeTarget != null && _shakeTarget != transform && raceTimeSeconds <= _damageShakeUntilSeconds)
            {
                float duration = Mathf.Max(0.0001f, _damageShakeUntilSeconds - _damageEffectStartSeconds);
                float elapsed = Mathf.Max(0f, raceTimeSeconds - _damageEffectStartSeconds);
                float decay = 1f - Mathf.Clamp01(elapsed / duration);
                float radians = elapsed * _damageShakeFrequencyHz * Mathf.PI * 2f;

                float offsetX = Mathf.Sin(radians) * _damageShakeAmplitudePx * decay;
                float offsetY = Mathf.Cos(radians * 0.73f) * _damageShakeAmplitudePx * 0.35f * decay;
                _shakeTarget.localPosition = _defaultShakeLocalPosition + new Vector3(offsetX, offsetY, 0f);
            }
            else if (_shakeTarget != null && _shakeTarget != transform)
            {
                _shakeTarget.localPosition = _defaultShakeLocalPosition;
            }

            if (raceTimeSeconds > _damageFlashUntilSeconds && raceTimeSeconds > _damageShakeUntilSeconds)
            {
                _damageEffectActive = false;
            }
        }

        private void OnDisable()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _defaultColor;
            }

            if (_shakeTarget != null && _shakeTarget != transform)
            {
                _shakeTarget.localPosition = _defaultShakeLocalPosition;
            }
            _damageEffectActive = false;
        }
    }
}