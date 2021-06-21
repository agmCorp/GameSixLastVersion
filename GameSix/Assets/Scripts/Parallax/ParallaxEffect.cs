using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "ParallaxEffect";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField, Range(0, 1)]
    [Tooltip("The proportion of the camera's movement to move the layer by (x-axis)")]
    private float _parallaxScaleX = 0.0f;

    [SerializeField, Range(0, 1)]
    [Tooltip("The proportion of the camera's movement to move the layer by (y-axis)")]
    private float _parallaxScaleY = 0.0f;

    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition; // The position of the camera in the previous frame.
    private Vector3 _tmp;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _cameraTransform = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_VIRTUAL_CAMERA_ONE).transform;
    }

    private void Start()
    {
        _lastCameraPosition = _cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;
        _tmp = transform.position;
        _tmp.x += deltaMovement.x * _parallaxScaleX;
        _tmp.y += deltaMovement.y * _parallaxScaleY;
        transform.position = _tmp;
        _lastCameraPosition = _cameraTransform.position;
    }
    #endregion
}

