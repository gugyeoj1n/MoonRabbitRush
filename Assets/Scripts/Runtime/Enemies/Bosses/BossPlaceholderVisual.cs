using UnityEngine;

namespace MoonRabbitRush.Enemies.Bosses
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BossPlaceholderVisual : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _size = 3.2f;
        [SerializeField] private Color _color = Color.black;

        private static Sprite _squareSprite;

        private void Awake()
        {
            if (_squareSprite == null)
            {
                Texture2D texture = Texture2D.whiteTexture;
                _squareSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    texture.width / _size);
                _squareSprite.name = "Boss Placeholder Square";
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = _squareSprite;
            spriteRenderer.color = _color;
            spriteRenderer.sortingOrder = 9;
        }

        private void OnValidate()
        {
            _size = Mathf.Max(0.1f, _size);
        }
    }
}
