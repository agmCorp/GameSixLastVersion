using UnityEngine;

public class ParallaxInfiniteScroll : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "ParallaxInfiniteScroll";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField] private bool _enableInfiniteScrollX = false;
    [SerializeField] private bool _enableInfiniteScrollY = false;

    private SpriteRenderer _spriteRenderer;
    private GameObject _camera;
    private Transform _cameraTransform;
    private float _textureUnitSizeX;
    private float _textureUnitSizeY;
    private Vector3 _tmp;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _camera = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_VIRTUAL_CAMERA_ONE);
    }

    private void Start()
    {
        _cameraTransform = _camera.transform;
        Sprite sprite = _spriteRenderer.sprite;
        Texture2D texture = sprite.texture;
        _textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        _textureUnitSizeY = texture.height / sprite.pixelsPerUnit;
    }

    private void LateUpdate()
    {
        if (_enableInfiniteScrollX)
        {
            float diffPositionX = _cameraTransform.position.x - transform.position.x;
            if (Mathf.Abs(diffPositionX) >= _textureUnitSizeX)
            {
                float offsetPositionX = (diffPositionX) % _textureUnitSizeX;
                _tmp.x = _cameraTransform.position.x + offsetPositionX;
                _tmp.y = transform.position.y;
                transform.position = _tmp;
            }
        }

        if (_enableInfiniteScrollY)
        {
            float diffPositionY = _cameraTransform.position.y - transform.position.y;
            if (Mathf.Abs(diffPositionY) >= _textureUnitSizeY)
            {
                float offsetPositionY = (diffPositionY) % _textureUnitSizeY;
                _tmp.x = transform.position.x;
                _tmp.y = _cameraTransform.position.y + offsetPositionY;
                transform.position = _tmp;
            }
        }
    }
    #endregion
}

