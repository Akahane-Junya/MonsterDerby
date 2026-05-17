using System;
using System.Reflection;
using MonsterDerby.Domain.SharedKernel;
using MonsterDerby.Infrastructure.Repositories;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Home
{
    /// <summary>
    /// ホーム表示用に見た目アセットを適用する専用オペレータ。
    /// Homeでは race 用 idle を使い回す前提で MotionSet を同じく適用する。
    /// </summary>
    public sealed class HomeMonsterVisualAssetOperator
    {
        private readonly ScriptableObjectMonsterVisualRepository _visualRepository;

        public HomeMonsterVisualAssetOperator(ScriptableObjectMonsterVisualRepository visualRepository)
        {
            _visualRepository = visualRepository ?? throw new ArgumentNullException(nameof(visualRepository));
        }

        public void Apply(GameObject target, SpeciesId speciesId)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var visual = _visualRepository.GetBySpeciesId(speciesId);
            ApplySpriteLibraryAsset(target, visual.spriteLibraryAsset);

            var controller = target.GetComponent<MonsterDerby.Presentation.Animation.Race.SpriteLibraryAnimationController>();
            if (controller != null && visual.motionSet != null)
            {
                controller.SetMotionSet(visual.motionSet);
                controller.SetRunState(false, 0f);
            }
        }

        private static void ApplySpriteLibraryAsset(GameObject target, UnityEngine.Object spriteLibraryAsset)
        {
            if (spriteLibraryAsset == null)
                return;

            var spriteLibrary = target.GetComponent("SpriteLibrary");
            if (spriteLibrary == null)
                return;

            var type = spriteLibrary.GetType();
            var property = type.GetProperty("spriteLibraryAsset", BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(spriteLibrary, spriteLibraryAsset);
                return;
            }

            var field = type.GetField("m_SpriteLibraryAsset", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? type.GetField("spriteLibraryAsset", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            field?.SetValue(spriteLibrary, spriteLibraryAsset);
        }
    }
}
