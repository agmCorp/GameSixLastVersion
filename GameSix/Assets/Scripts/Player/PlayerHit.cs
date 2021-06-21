using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "PlayerHit";
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
        _log.DebugLog(LOG_TAG, "PlayerHit.OnTriggerEnter2D", gameObject);

        GameObject obj = collision.transform.parent.gameObject;
        if (obj.CompareTag(TargetController.TAG_TARGET))
        {
            _playerHealth.TargetCollisionEnter(_playerController, obj.GetComponent<TargetController>());
        }
        else if (obj.CompareTag(PowerUpController.TAG_POWER_UP))
        {
            _playerHealth.PowerUpCollision(_playerController, obj.GetComponent<ICollectible>());
        }
        else if (obj.CompareTag(CandyController.TAG_CANDY))
        {
            _playerHealth.CandyCollision(_playerController, obj.GetComponent<ICollectible>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _log.DebugLog(LOG_TAG, "PlayerHit.OnTriggerExit2D", gameObject);

        GameObject obj = collision.transform.parent.gameObject;
        if (obj.CompareTag(TargetController.TAG_TARGET))
        {
            _playerHealth.TargetCollisionExit(_playerController);
        }
    }
    #endregion
}
