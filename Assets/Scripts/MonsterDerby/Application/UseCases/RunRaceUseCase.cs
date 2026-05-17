using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDerby.Domain.Course;
using MonsterDerby.Domain.Monster;
using MonsterDerby.Domain.Race;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Domain.Skill;

namespace MonsterDerby.Application.UseCases
{
    /// <summary>
    /// レースを実行するユースケース
    /// </summary>
    public sealed class RunRaceUseCase
    {
        private readonly RaceEngine _raceEngine;

        public RunRaceUseCase(RaceEngine raceEngine)
        {
            _raceEngine = Guard.NotNull(raceEngine, nameof(raceEngine));
        }

        /// <summary>
        /// レースを実行する
        /// </summary>
        /// <param name="courseId">コースID</param>
        /// <param name="participants">参加モンスターのスナップショット</param>
        /// <param name="createDebugEvents">デバッグイベントを生成するか</param>
        /// <returns>レースの実行結果と再生用データ</returns>
        public RaceRunOutput Execute(CourseId courseId, MonsterSnapshot[] participants, bool createDebugEvents = false)
        {
            // --- 入力検証 ---
            Guard.NotNull(participants, nameof(participants));

            if (participants.Length < 2)
                throw new ArgumentException($"参加者は最低2体必要です。現在: {participants.Length}", nameof(participants));

            if (participants.Any(p => p == null))
                throw new ArgumentException("参加者リストに null が含まれています。", nameof(participants));

            // --- RaceInput の構築 ---
            var raceInput = new RaceInput(
                courseId,
                participants,
                randomSeed: DateTime.UtcNow.Ticks, // 暫定的に現在時刻を使用
                new SimulationConfiguration(
                    sampleIntervalSeconds: 0.1f, // 100ms ごとにサンプル
                    maximumSimulationSeconds: 600f, // 最大10分
                    speedMultiplier: 1.0f // プレイヤーが操作する再生速度倍率（デフォルト1.0 = 通常速）
                )
            );

            // --- ドメインロジックの実行 ---
            return _raceEngine.RunRace(raceInput, createDebugEvents);
        }
    }
}