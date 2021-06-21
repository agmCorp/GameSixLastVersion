using UnityEngine;

public class PlayerHurt : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "PlayerHurt";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private PlayerHealth _playerHealth;
    private PlayerController _playerController;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(PlayerController.TAG_PLAYER);
        _playerHealth = player.GetComponent<PlayerHealth>();
        _playerController = player.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _log.DebugLog(LOG_TAG, "PlayerHurt.OnTriggerEnter2D", gameObject);

        GameObject obj = collision.transform.parent.gameObject;
        if (obj.CompareTag(MoonController.TAG_MOON))
        {
            _playerHealth.MoonCollision(_playerController, obj.GetComponent<MoonController>());
        }
    }
    #endregion
}
