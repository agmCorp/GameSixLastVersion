using UnityEngine;

public class ParallaxJustMove : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "ParallaxJustMove";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField] private Vector3 _speed = Vector3.zero;
    #endregion

    #region MonoBehaviour
    private void LateUpdate()
    {
        transform.position += _speed * Time.deltaTime;
    }
    #endregion
}
