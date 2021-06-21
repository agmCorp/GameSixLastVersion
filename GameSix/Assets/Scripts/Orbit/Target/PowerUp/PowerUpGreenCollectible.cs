using UnityEngine;

public class PowerUpGreenCollectible : MonoBehaviour, IPowerUpCollectible
{
    #region Private Constants
    private const string LOG_TAG = "PowerUpGreenCollectible";
    private const int MIN_SLOW_DOWN_COUNT = 1;
    private const int MAX_SLOW_DOWN_COUNT = 4;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion

    #region API
    public void OnPowerUpColectibleTaken(PlayerController playerController, Hud hud)
    {
        int slowDownCount = Random.Range(MIN_SLOW_DOWN_COUNT, MAX_SLOW_DOWN_COUNT + 1);
        playerController.ApplyPowerUpGreen(slowDownCount);
        hud.ShowPowerUp(ObjectPooler.POWER_UP_GREEN_KEY, slowDownCount);

        _log.DebugLog(LOG_TAG, "Applying Power Up GREEN with slowDownCount = " + slowDownCount, gameObject);
    }
    #endregion
}
