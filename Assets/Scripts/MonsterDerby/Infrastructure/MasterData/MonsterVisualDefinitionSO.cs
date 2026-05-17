using UnityEngine;
using MonsterDerby.Presentation.Animation.Race;

namespace MonsterDerby.Infrastructure.MasterData
{
    [CreateAssetMenu(menuName = "MonsterDerby/MasterData/MonsterVisual")]
    public sealed class MonsterVisualDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string visualId;

        [Header("Sprite Library")]
        public Object spriteLibraryAsset;

        [Header("Motion")]
        public SpriteMotionSet motionSet;
    }
}
