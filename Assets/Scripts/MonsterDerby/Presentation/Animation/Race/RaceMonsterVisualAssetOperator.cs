using System;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.Repositories;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Race
{
    /// <summary>
    /// レース表示用に見た目アセットを適用する専用オペレータ。
    /// </summary>
    public sealed class RaceMonsterVisualAssetOperator
    {
        private readonly ScriptableObjectMonsterVisualRepository _visualRepository;

        public RaceMonsterVisualAssetOperator(ScriptableObjectMonsterVisualRepository visualRepository)
        {
            _visualRepository = visualRepository ?? throw new ArgumentNullException(nameof(visualRepository));
        }

        public void Apply(GameObject target, SpeciesId speciesId)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var visual = _visualRepository.GetBySpeciesId(speciesId);
            var controller = target.GetComponent<SpriteLibraryAnimationController>();
            if (controller == null)
                return;

            if (visual.spriteLibraryAsset == null)
                throw new InvalidOperationException($"MonsterVisualDefinitionSO '{speciesId.Value}': spriteLibraryAsset が未設定です。");
            if (visual.motionSet == null)
                throw new InvalidOperationException($"MonsterVisualDefinitionSO '{speciesId.Value}': motionSet が未設定です。");

            var typeName = visual.spriteLibraryAsset.GetType().FullName;
            if (typeName != "UnityEngine.U2D.Animation.SpriteLibraryAsset")
                throw new InvalidOperationException(
                    $"MonsterVisualDefinitionSO '{speciesId.Value}': spriteLibraryAsset の型が不正です。" +
                    $"期待値: UnityEngine.U2D.Animation.SpriteLibraryAsset、実際: {typeName}");

            controller.SetSpriteLibraryAsset(visual.spriteLibraryAsset);
            controller.SetMotionSet(visual.motionSet);
        }
    }
}
