// MonsterDerby.Domain.MasterData
// 目的:
//  - 不変のマスターデータを表現する
//  - 具体的な保存形式（ScriptableObject/JSON等）は Infrastructure 側に置く

namespace MonsterDerby.Domain.MasterData
{
    using System;
    using MonsterDerby.Domain.Monster;
    using MonsterDerby.Domain.SharedKernel;

    public sealed class SpeciesDefinition
    {
        public SpeciesId SpeciesId { get; }
        public string Name { get; }
        public string VisualId { get; }

        /// <summary>
        /// ショップ購入など「新規作成時」の成長テーブル。
        /// 交配すると子は独自の GrowthIncrements を持つ。
        /// </summary>
        public GrowthIncrements SpeciesGrowthIncrements { get; }

        /// <summary>
        /// 種が持つ標準のスキル学習計画（Lvで解放）。
        /// </summary>
        public SpeciesLearnset SpeciesLearnset { get; }

        public SpeciesDefinition(
            SpeciesId speciesId,
            string name,
            string visualId,
            GrowthIncrements speciesGrowthIncrements,
            SpeciesLearnset speciesLearnset)
        {
            SpeciesId = speciesId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            VisualId = visualId ?? throw new ArgumentNullException(nameof(visualId));
            SpeciesGrowthIncrements = speciesGrowthIncrements ?? throw new ArgumentNullException(nameof(speciesGrowthIncrements));
            SpeciesLearnset = speciesLearnset ?? throw new ArgumentNullException(nameof(speciesLearnset));
        }
    }
}
