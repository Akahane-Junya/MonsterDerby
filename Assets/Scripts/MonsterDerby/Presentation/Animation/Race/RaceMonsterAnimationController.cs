using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// レース画面専用。本体モーションは常に run を維持し、
    /// 被弾は白フラッシュと振動、攻撃は別 GameObject のエフェクトで表現する。
    /// </summary>
    public sealed class RaceMonsterAnimationController : MonoBehaviour, IRaceRunnerAnimationController
    {
        [SerializeField] private MonsterAnimatorAdapter _animatorAdapter;
        [SerializeField] private float _damageFlashDurationSeconds = 0.12f;
        [SerializeField] private float _damageShakeDurationSeconds = 0.18f;
        [SerializeField] private float _damageShakeAmplitudePx = 1.5f;
        [SerializeField] private float _damageShakeFrequencyHz = 35f;
        [SerializeField] private Color _damageFlashColor = new Color(1.8f, 1.8f, 1.8f, 1f);

        private readonly List<SpriteRenderer> _spriteRenderers = new();
        private readonly List<Color> _defaultColors = new();

        private bool _isRunning;
        private bool _damageEffectActive;
        private float _damageEffectStartSeconds;
        private float _damageFlashUntilSeconds;
        private float _damageShakeUntilSeconds;
        private Transform _shakeTarget;
        private Vector3 _defaultShakeLocalPosition;

        private void Awake()
        {
            if (_animatorAdapter == null)
            {
                _animatorAdapter = GetComponent<MonsterAnimatorAdapter>();
            }

            if (_animatorAdapter == null)
            {
                throw new InvalidOperationException("MonsterAnimatorAdapter が見つかりません。");
            }

            _shakeTarget = _animatorAdapter.transform != transform ? _animatorAdapter.transform : null;
            if (_shakeTarget != null)
            {
                _defaultShakeLocalPosition = _shakeTarget.localPosition;
            }

            var renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            for (int i = 0; i < renderers.Length; i++)
            {
                _spriteRenderers.Add(renderers[i]);
                _defaultColors.Add(renderers[i].color);
            }
        }

        public void SetRunState(bool isRunning, float raceTimeSeconds)
        {
            if (isRunning)
            {
                if (!_isRunning)
                {
                    _animatorAdapter.PlayRun();
                    _isRunning = true;
                }
            }
            else
            {
                if (_isRunning)
                {
                    _animatorAdapter.PlayRaceIdle();
                    _isRunning = false;
                }
            }

            Tick(raceTimeSeconds);
        }

        public void Tick(float raceTimeSeconds)
        {
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

        private void UpdateDamageEffect(float raceTimeSeconds)
        {
            if (!_damageEffectActive)
            {
                return;
            }

            for (int i = 0; i < _spriteRenderers.Count; i++)
            {
                var spriteRenderer = _spriteRenderers[i];
                if (spriteRenderer == null)
                {
                    continue;
                }

                if (raceTimeSeconds <= _damageFlashUntilSeconds)
                {
                    float duration = Mathf.Max(0.0001f, _damageFlashUntilSeconds - _damageEffectStartSeconds);
                    float t = 1f - Mathf.Clamp01((raceTimeSeconds - _damageEffectStartSeconds) / duration);
                    spriteRenderer.color = Color.Lerp(_defaultColors[i], _damageFlashColor, t);
                }
                else
                {
                    spriteRenderer.color = _defaultColors[i];
                }
            }

            if (_shakeTarget != null && raceTimeSeconds <= _damageShakeUntilSeconds)
            {
                float duration = Mathf.Max(0.0001f, _damageShakeUntilSeconds - _damageEffectStartSeconds);
                float elapsed = Mathf.Max(0f, raceTimeSeconds - _damageEffectStartSeconds);
                float decay = 1f - Mathf.Clamp01(elapsed / duration);
                float radians = elapsed * _damageShakeFrequencyHz * Mathf.PI * 2f;

                float offsetX = Mathf.Sin(radians) * _damageShakeAmplitudePx * decay;
                float offsetY = Mathf.Cos(radians * 0.73f) * _damageShakeAmplitudePx * 0.35f * decay;
                _shakeTarget.localPosition = _defaultShakeLocalPosition + new Vector3(offsetX, offsetY, 0f);
            }
            else if (_shakeTarget != null)
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
            for (int i = 0; i < _spriteRenderers.Count; i++)
            {
                if (_spriteRenderers[i] != null)
                {
                    _spriteRenderers[i].color = _defaultColors[i];
                }
            }

            if (_shakeTarget != null)
            {
                _shakeTarget.localPosition = _defaultShakeLocalPosition;
            }
            _damageEffectActive = false;
            _isRunning = false;
        }
    }
}