using System;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Race
{
    [Serializable]
    public sealed class SpriteMotionClip
    {
        [SerializeField] private string _category;
        [SerializeField] private string[] _labels;
        [SerializeField] private float _fps = 12f;
        [SerializeField] private bool _loop = true;

        public string Category => _category;
        public string[] Labels => _labels;
        public float Fps => _fps;
        public bool Loop => _loop;

        public bool IsValid =>
            !string.IsNullOrEmpty(_category)
            && _labels != null
            && _labels.Length > 0
            && _fps > 0f;
    }
}