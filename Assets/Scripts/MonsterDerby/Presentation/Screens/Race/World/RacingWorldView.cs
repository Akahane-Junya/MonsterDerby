using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonsterDerby.Domain.Race;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Presentation.Animation.Race;

namespace MonsterDerby.Presentation.Screens.Race.World
{
    /// <summary>
    /// レース描画の本体
    /// 前提: Orthographic camera + 1 world unit = 1 pixel 相当
    /// </summary>
    public sealed class RacingWorldView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _runnerPrefab;
        [SerializeField] private Transform _runnerContainer;
        [SerializeField] private Camera _mainCamera;

        [Header("Screen")]
        [SerializeField] private int _screenHeight = 360;

        [Header("Lane")]
        [SerializeField] private float _laneSpacingPx = 32f;          // レーン間隔(px)
        [SerializeField] private float _laneYOffsetPx = 0f;           // 微調整用（必要なら）

        [Header("X Scale")]
        [SerializeField] private float _metersToPixels = 1f;         // 1m = 何px か（適宜調整）

        [Header("Playback")]
        [SerializeField] private float _playbackSpeed = 0.4f;
        private int _playbackSpeedMultiplier = 1;

        private readonly Dictionary<MonsterId, RaceRunnerRuntime> _runners = new();
        private IRaceMotionOrchestrator _motionOrchestrator;
        private RaceMonsterVisualAssetOperator _visualAssetOperator;
        private Coroutine _playbackCoroutine;

        private int _laneCount = 1;

        public event Action OnRaceCompleted;

        private void Awake()
        {
            if (_runnerPrefab == null)
                throw new InvalidOperationException("RunnerPrefab が設定されていません。");
            if (_runnerContainer == null)
                throw new InvalidOperationException("RunnerContainer が設定されていません。");
            if (_mainCamera == null)
                throw new InvalidOperationException("MainCamera が設定されていません。");

            EnsureMotionOrchestrator();

            // 1 unit = 1 px 相当に寄せる
            if (!_mainCamera.orthographic)
            {
                Debug.LogWarning("RacingWorldView: MainCamera が Perspective です。Orthographic 推奨（1 unit=px が崩れます）");
            }
            else
            {
                _mainCamera.orthographicSize = _screenHeight * 0.5f; // 360 -> 180
            }
        }

        public void Initialize(IRaceMotionOrchestrator motionOrchestrator)
        {
            _motionOrchestrator = motionOrchestrator ?? throw new ArgumentNullException(nameof(motionOrchestrator));
        }

        public void InitializeVisualOperator(RaceMonsterVisualAssetOperator visualAssetOperator)
        {
            _visualAssetOperator = visualAssetOperator ?? throw new ArgumentNullException(nameof(visualAssetOperator));
        }

        public void StartRace(RaceRunOutput output, MonsterSnapshot[] participants)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (output.RaceResult == null)
                throw new InvalidOperationException("RaceResult が null です。");
            if (participants == null)
                throw new ArgumentNullException(nameof(participants));

            var speciesByMonsterId = participants.ToDictionary(p => p.MonsterId, p => p.SpeciesId);
            var initialFramesByMonsterId = BuildInitialFrameMap(output.RaceResult.Samples);

            ClearRunners();

            // レーン数を推定（Runnersフレームの最大LaneIndex + 1）
            _laneCount = EstimateLaneCount(output.RaceResult.Samples);
            if (_laneCount <= 0) _laneCount = 1;

            // ランナー生成
            foreach (var monsterId in output.RaceResult.LaneToMonsterId)
            {
                var runnerObj = Instantiate(_runnerPrefab, _runnerContainer);

                if (_visualAssetOperator != null && speciesByMonsterId.TryGetValue(monsterId, out var speciesId))
                {
                    _visualAssetOperator.Apply(runnerObj, speciesId);
                }

                var runtime = new RaceRunnerRuntime(monsterId, runnerObj);
                _runners.Add(monsterId, runtime);

                if (initialFramesByMonsterId.TryGetValue(monsterId, out var initialFrame))
                {
                    float xPx = initialFrame.DistanceMeters * _metersToPixels;
                    float yPx = LaneIndexToYPx(initialFrame.LaneIndex, _laneCount) + _laneYOffsetPx;
                    runtime.SetPosition(xPx, yPx);
                }

                var animationController = runnerObj.GetComponent<IRaceRunnerAnimationController>();
                if (animationController != null)
                {
                    _motionOrchestrator.RegisterRunner(monsterId, animationController);
                }
                else
                {
                    Debug.LogWarning("RacingWorldView: RunnerPrefab に IRaceRunnerAnimationController 実装がありません。");
                }
            }

            for (int lane = 0; lane < output.RaceResult.LaneToMonsterId.Length; lane++)
            {
                _motionOrchestrator.BindLane(lane, output.RaceResult.LaneToMonsterId[lane]);
            }

            ApplyInitialRunnerPositions(output.RaceResult.Samples);

            if (_playbackCoroutine != null)
                StopCoroutine(_playbackCoroutine);

            _playbackCoroutine = StartCoroutine(PlaybackRace(output.RaceResult.Samples));
        }

        private static Dictionary<MonsterId, RunnerFrame> BuildInitialFrameMap(IReadOnlyList<RaceSample> samples)
        {
            var map = new Dictionary<MonsterId, RunnerFrame>();
            if (samples == null || samples.Count == 0)
            {
                return map;
            }

            var initialSample = samples[0];
            foreach (var frame in initialSample.Runners)
            {
                map[frame.MonsterId] = frame;
            }

            return map;
        }

        public void SetPlaybackSpeedMultiplier(int multiplier)
        {
            if (multiplier < 1)
            {
                _playbackSpeedMultiplier = 1;
                return;
            }

            if (multiplier > 3)
            {
                _playbackSpeedMultiplier = 3;
                return;
            }

            _playbackSpeedMultiplier = multiplier;
        }

        private void ApplyInitialRunnerPositions(IReadOnlyList<RaceSample> samples)
        {
            if (samples == null || samples.Count == 0)
            {
                return;
            }

            var initialSample = samples[0];
            foreach (var frame in initialSample.Runners)
            {
                if (_runners.TryGetValue(frame.MonsterId, out var runner))
                {
                    float xPx = frame.DistanceMeters * _metersToPixels;
                    float yPx = LaneIndexToYPx(frame.LaneIndex, _laneCount) + _laneYOffsetPx;
                    runner.SetPosition(xPx, yPx);
                }
            }

            UpdateCamera(initialSample);
        }

        private static int EstimateLaneCount(IReadOnlyList<RaceSample> samples)
        {
            int maxLane = 0;
            for (int i = 0; i < samples.Count; i++)
            {
                var runners = samples[i].Runners;
                for (int r = 0; r < runners.Length; r++)
                {
                    if (runners[r].LaneIndex > maxLane)
                        maxLane = runners[r].LaneIndex;
                }
            }
            return maxLane + 1;
        }

        private IEnumerator PlaybackRace(IReadOnlyList<RaceSample> samples)
        {
            if (samples == null || samples.Count == 0)
            {
                Debug.LogWarning("Samples が空です。");
                OnRaceCompleted?.Invoke();
                yield break;
            }

            float raceTime = 0f;
            int nextSampleIndex = 0;
            int lastAppliedSampleIndex = -1;

            while (nextSampleIndex < samples.Count || lastAppliedSampleIndex < samples.Count - 1)
            {
                while (nextSampleIndex < samples.Count && samples[nextSampleIndex].TimeSeconds <= raceTime)
                {
                    var sampled = samples[nextSampleIndex];
                    _motionOrchestrator.ApplySample(sampled, sampled.TimeSeconds);
                    lastAppliedSampleIndex = nextSampleIndex;
                    nextSampleIndex++;
                }

                var renderSampleIndex = lastAppliedSampleIndex >= 0 ? lastAppliedSampleIndex : 0;
                var current = samples[renderSampleIndex];
                var next = nextSampleIndex < samples.Count ? samples[nextSampleIndex] : current;
                var interpolation = ComputeInterpolationFactor(current, next, raceTime);

                // サンプル間を補間して、スキル発動タイミング付近の位置ジャンプを抑える。
                foreach (var frame in current.Runners)
                {
                    if (_runners.TryGetValue(frame.MonsterId, out var runner))
                    {
                        float distanceMeters = frame.DistanceMeters;
                        if (TryGetRunnerFrame(next, frame.MonsterId, out var nextFrame))
                        {
                            distanceMeters = Mathf.Lerp(frame.DistanceMeters, nextFrame.DistanceMeters, interpolation);
                        }

                        float xPx = distanceMeters * _metersToPixels;
                        float yPx = LaneIndexToYPx(frame.LaneIndex, _laneCount) + _laneYOffsetPx;

                        runner.SetPosition(xPx, yPx);
                    }
                }

                UpdateCamera(current, next, interpolation);

                raceTime += Time.deltaTime * _playbackSpeed * _playbackSpeedMultiplier;
                _motionOrchestrator.Update(raceTime);

                yield return null;
            }

            OnRaceCompleted?.Invoke();
        }

        /// <summary>
        /// laneIndex(0..laneCount-1) を「中心揃え」したY(px)に変換する
        /// 例) laneCount=5 -> [-64, -32, 0, 32, 64]
        /// </summary>
        private float LaneIndexToYPx(int laneIndex, int laneCount)
        {
            if (laneCount <= 1) return 0f;

            float center = (laneCount - 1) * 0.5f;
            return (laneIndex - center) * _laneSpacingPx;
        }

        private void UpdateCamera(RaceSample sample)
        {
            UpdateCamera(sample, sample, 0f);
        }

        private void UpdateCamera(RaceSample current, RaceSample next, float interpolation)
        {
            if (current.Runners == null || current.Runners.Length == 0) return;

            float maxDistancePx = float.MinValue;
            foreach (var runnerFrame in current.Runners)
            {
                float distanceMeters = runnerFrame.DistanceMeters;
                if (TryGetRunnerFrame(next, runnerFrame.MonsterId, out var nextFrame))
                {
                    distanceMeters = Mathf.Lerp(runnerFrame.DistanceMeters, nextFrame.DistanceMeters, interpolation);
                }

                float xPx = distanceMeters * _metersToPixels;
                if (xPx > maxDistancePx)
                    maxDistancePx = xPx;
            }

            var cameraPos = _mainCamera.transform.position;

            // 先頭に追従：画面中央に先頭が来るようにする（必要なら調整）
            _mainCamera.transform.position = new Vector3(maxDistancePx, cameraPos.y, cameraPos.z);
        }

        private static float ComputeInterpolationFactor(RaceSample current, RaceSample next, float raceTime)
        {
            if (current == null || next == null)
            {
                return 0f;
            }

            float duration = next.TimeSeconds - current.TimeSeconds;
            if (duration <= Mathf.Epsilon)
            {
                return 0f;
            }

            return Mathf.Clamp01((raceTime - current.TimeSeconds) / duration);
        }

        private static bool TryGetRunnerFrame(RaceSample sample, MonsterId monsterId, out RunnerFrame frame)
        {
            if (sample != null)
            {
                foreach (var runnerFrame in sample.Runners)
                {
                    if (runnerFrame.MonsterId == monsterId)
                    {
                        frame = runnerFrame;
                        return true;
                    }
                }
            }

            frame = null;
            return false;
        }

        private void ClearRunners()
        {
            foreach (var runner in _runners.Values)
            {
                runner.Destroy();
            }
            _runners.Clear();

            if (_motionOrchestrator != null)
            {
                _motionOrchestrator.Clear();
            }
        }

        private void OnDisable()
        {
            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
                _playbackCoroutine = null;
            }
        }

        private void EnsureMotionOrchestrator()
        {
            if (_motionOrchestrator == null)
            {
                // 既定構成。必要なら Initialize で差し替える。
                _motionOrchestrator = new RaceMotionOrchestrator(new RaceMotionPolicy());
            }
        }
    }
}