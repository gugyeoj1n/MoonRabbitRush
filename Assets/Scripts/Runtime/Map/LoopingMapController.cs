using UnityEngine;

namespace MoonRabbitRush.Map
{
    public sealed class LoopingMapController : MonoBehaviour
    {
        private const string LoopAreaName = "MapLoopArea";

        [SerializeField] private Transform _player;
        [SerializeField] private SpriteRenderer[] _tiles;
        [SerializeField] private Camera _targetCamera;
        [SerializeField, Min(0.01f)] private float _tileScale = 2f;
        [SerializeField, Range(0f, 1f)] private float _wrapTriggerPadding = 0.15f;
        [SerializeField] private int _sortingOrder = -100;

        private BoxCollider2D _loopArea;
        private Camera _resolvedCamera;
        private Vector2 _tileSize;
        private Vector2 _tileLocalSize;
        private int _gridSize;
        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (!_isInitialized)
            {
                return;
            }

            WrapTiles();
        }

        private void OnValidate()
        {
            _tileScale = Mathf.Max(0.01f, _tileScale);
            _wrapTriggerPadding = Mathf.Clamp01(_wrapTriggerPadding);

            if (!Application.isPlaying)
            {
                TryApplyLayoutInEditor();
            }
        }

        private void Initialize()
        {
            if (_player == null || !TryGetGridSize(out int gridSize))
            {
                return;
            }

            _gridSize = gridSize;

            Transform loopAreaTransform = _player.Find(LoopAreaName);

            if (loopAreaTransform == null)
            {
                Debug.LogError(
                    $"'{LoopAreaName}' child was not found on player.",
                    this);
                return;
            }

            _loopArea = loopAreaTransform.GetComponent<BoxCollider2D>();

            if (_loopArea == null)
            {
                Debug.LogError(
                    $"'{LoopAreaName}' is missing {nameof(BoxCollider2D)}.",
                    loopAreaTransform);
                return;
            }

            if (!TryGetTileSprite(out Sprite tileSprite))
            {
                Debug.LogError("Map tile sprite is not assigned.", this);
                return;
            }

            _resolvedCamera = ResolveCamera();

            transform.position = new Vector3(
                _player.position.x,
                _player.position.y,
                transform.position.z);

            Vector3 spriteSize = tileSprite.bounds.size;
            _tileLocalSize = new Vector2(
                spriteSize.x * _tileScale,
                spriteSize.y * _tileScale);
            _tileSize = Vector2.Scale(
                _tileLocalSize,
                new Vector2(transform.lossyScale.x, transform.lossyScale.y));

            ApplyTileAppearance();
            ApplyTileLayout();
            ResizeLoopArea();
            _isInitialized = true;
        }

        private bool TryGetTileSprite(out Sprite sprite)
        {
            sprite = null;

            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile != null && tile.sprite != null)
                {
                    sprite = tile.sprite;
                    return true;
                }
            }

            return false;
        }

        private void ApplyTileAppearance()
        {
            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile == null)
                {
                    continue;
                }

                tile.sortingOrder = _sortingOrder;
                tile.transform.localScale = new Vector3(_tileScale, _tileScale, 1f);
            }
        }

        private void ApplyTileLayout()
        {
            if (_gridSize <= 0)
            {
                return;
            }

            float startX = -((_gridSize - 1) * _tileLocalSize.x) * 0.5f;
            float startY = ((_gridSize - 1) * _tileLocalSize.y) * 0.5f;

            for (int index = 0; index < _tiles.Length; index++)
            {
                int row = index / _gridSize;
                int column = index % _gridSize;
                Vector2 position = new(
                    startX + column * _tileLocalSize.x,
                    startY - row * _tileLocalSize.y);
                SetTileLocalPosition(_tiles[index], position);
            }
        }

        private void SetTileLocalPosition(SpriteRenderer tile, Vector2 position)
        {
            if (tile == null)
            {
                return;
            }

            tile.transform.localPosition = new Vector3(position.x, position.y, 0f);
        }

        private void ResizeLoopArea()
        {
            Vector3 loopAreaScale = _loopArea.transform.lossyScale;
            float widthScale = Mathf.Approximately(loopAreaScale.x, 0f)
                ? 1f
                : loopAreaScale.x;
            float heightScale = Mathf.Approximately(loopAreaScale.y, 0f)
                ? 1f
                : loopAreaScale.y;

            _loopArea.isTrigger = true;
            _loopArea.offset = Vector2.zero;
            _loopArea.size = new Vector2(
                _tileSize.x * _gridSize / widthScale,
                _tileSize.y * _gridSize / heightScale);
        }

        private void WrapTiles()
        {
            Bounds cameraBounds = GetWrapBounds();
            float horizontalPadding = _tileSize.x * _wrapTriggerPadding;
            float verticalPadding = _tileSize.y * _wrapTriggerPadding;
            float wrapWidth = _tileSize.x * _gridSize;
            float wrapHeight = _tileSize.y * _gridSize;

            while (cameraBounds.max.x > GetMapBounds().max.x - horizontalPadding)
            {
                MoveColumnToOppositeSide(moveLeftColumn: true, wrapWidth);
            }

            while (cameraBounds.min.x < GetMapBounds().min.x + horizontalPadding)
            {
                MoveColumnToOppositeSide(moveLeftColumn: false, wrapWidth);
            }

            while (cameraBounds.max.y > GetMapBounds().max.y - verticalPadding)
            {
                MoveRowToOppositeSide(moveBottomRow: true, wrapHeight);
            }

            while (cameraBounds.min.y < GetMapBounds().min.y + verticalPadding)
            {
                MoveRowToOppositeSide(moveBottomRow: false, wrapHeight);
            }
        }

        private Bounds GetWrapBounds()
        {
            Camera wrapCamera = ResolveCamera();

            if (wrapCamera == null || !wrapCamera.orthographic)
            {
                return _loopArea.bounds;
            }

            float verticalExtent = wrapCamera.orthographicSize;
            float horizontalExtent = verticalExtent * wrapCamera.aspect;
            Vector3 cameraPosition = wrapCamera.transform.position;

            return new Bounds(
                new Vector3(cameraPosition.x, cameraPosition.y, 0f),
                new Vector3(horizontalExtent * 2f, verticalExtent * 2f, 0f));
        }

        private Camera ResolveCamera()
        {
            if (_targetCamera != null)
            {
                return _targetCamera;
            }

            if (_resolvedCamera != null)
            {
                return _resolvedCamera;
            }

            if (ManagerRoot.Instance?.CameraMaanger?.MainCamera != null)
            {
                _resolvedCamera = ManagerRoot.Instance.CameraMaanger.MainCamera;
                return _resolvedCamera;
            }

            _resolvedCamera = Camera.main;
            return _resolvedCamera;
        }

        private Bounds GetMapBounds()
        {
            Bounds mapBounds = default;
            bool hasBounds = false;

            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    mapBounds = tile.bounds;
                    hasBounds = true;
                    continue;
                }

                mapBounds.Encapsulate(tile.bounds);
            }

            return mapBounds;
        }

        private void MoveColumnToOppositeSide(bool moveLeftColumn, float wrapWidth)
        {
            float targetCenterX = moveLeftColumn ? float.MaxValue : float.MinValue;

            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile == null)
                {
                    continue;
                }

                float centerX = tile.bounds.center.x;
                targetCenterX = moveLeftColumn
                    ? Mathf.Min(targetCenterX, centerX)
                    : Mathf.Max(targetCenterX, centerX);
            }

            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile == null)
                {
                    continue;
                }

                if (Mathf.Abs(tile.bounds.center.x - targetCenterX) > 0.01f)
                {
                    continue;
                }

                Vector3 position = tile.transform.position;
                position.x += moveLeftColumn ? wrapWidth : -wrapWidth;
                tile.transform.position = position;
            }
        }

        private void MoveRowToOppositeSide(bool moveBottomRow, float wrapHeight)
        {
            float targetCenterY = moveBottomRow ? float.MaxValue : float.MinValue;

            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile == null)
                {
                    continue;
                }

                float centerY = tile.bounds.center.y;
                targetCenterY = moveBottomRow
                    ? Mathf.Min(targetCenterY, centerY)
                    : Mathf.Max(targetCenterY, centerY);
            }

            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile == null)
                {
                    continue;
                }

                if (Mathf.Abs(tile.bounds.center.y - targetCenterY) > 0.01f)
                {
                    continue;
                }

                Vector3 position = tile.transform.position;
                position.y += moveBottomRow ? wrapHeight : -wrapHeight;
                tile.transform.position = position;
            }
        }

        private void TryApplyLayoutInEditor()
        {
            if (!TryGetGridSize(out int gridSize) || !TryGetTileSprite(out Sprite tileSprite))
            {
                return;
            }

            _gridSize = gridSize;
            Vector3 spriteSize = tileSprite.bounds.size;
            _tileLocalSize = new Vector2(
                spriteSize.x * _tileScale,
                spriteSize.y * _tileScale);
            ApplyTileAppearance();
            ApplyTileLayout();
        }

        private bool TryGetGridSize(out int gridSize)
        {
            gridSize = 0;

            if (_tiles == null || _tiles.Length == 0)
            {
                return false;
            }

            float sqrt = Mathf.Sqrt(_tiles.Length);
            int rounded = Mathf.RoundToInt(sqrt);

            if (rounded * rounded != _tiles.Length)
            {
                Debug.LogError("Tile count must form a square grid.", this);
                return false;
            }

            gridSize = rounded;
            return true;
        }
    }
}
