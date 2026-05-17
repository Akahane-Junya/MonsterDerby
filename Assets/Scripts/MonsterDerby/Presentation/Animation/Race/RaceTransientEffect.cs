using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Race
{
    internal sealed class RaceTransientEffect : MonoBehaviour
    {
        private static Sprite s_whiteSprite;

        [SerializeField] private float _lifetimeSeconds = 0.28f;
        [SerializeField] private float _startScale = 10f;
        [SerializeField] private float _endScale = 28f;
        [SerializeField] private float _forwardDistance = 18f;
        [SerializeField] private Color _startColor = new Color(1f, 0.8f, 0.25f, 0.95f);
        [SerializeField] private Color _endColor = new Color(1f, 0.35f, 0.1f, 0f);

        private SpriteRenderer _spriteRenderer;
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _startedAt;

        public static void SpawnAttackEffect(Transform origin)
        {
            if (origin == null)
            {
                return;
            }

            var effectObject = new GameObject("RaceAttackEffect");
            effectObject.transform.position = origin.position + new Vector3(6f, 0f, -0.1f);

            var effect = effectObject.AddComponent<RaceTransientEffect>();
            effect.Initialize();
        }

        private void Initialize()
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = GetWhiteSprite();
            _spriteRenderer.color = _startColor;
            _spriteRenderer.sortingOrder = 100;

            _startPosition = transform.position;
            _endPosition = _startPosition + new Vector3(_forwardDistance, 0f, 0f);
            _startedAt = Time.time;
            transform.localScale = Vector3.one * _startScale;
            transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        }

        private void Update()
        {
            float elapsed = Time.time - _startedAt;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, _lifetimeSeconds));

            transform.position = Vector3.Lerp(_startPosition, _endPosition, t);
            float scale = Mathf.Lerp(_startScale, _endScale, t);
            transform.localScale = new Vector3(scale, scale * 0.35f, 1f);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.Lerp(_startColor, _endColor, t);
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private static Sprite GetWhiteSprite()
        {
            if (s_whiteSprite != null)
            {
                return s_whiteSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;

            s_whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return s_whiteSprite;
        }
    }
}
