using UnityEngine;

public class PowerUpPinkCollectible : MonoBehaviour, IPowerUpCollectible
{
    #region Private Constants
    private const string LOG_TAG = "PowerUpPinkCollectible";
    private const int MIN_TASTY_VALUE = 10;
    private const int MAX_TASTY_VALUE = 20;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion

    #region API
    public void OnPowerUpColectibleTaken(PlayerController playerController, Hud hud)
    {
        int tastyValue = Random.Range(MIN_TASTY_VALUE, MAX_TASTY_VALUE + 1);
        playerController.ApplyPowerUpPink(tastyValue);
        hud.ShowPowerUp(ObjectPooler.POWER_UP_PINK_KEY, tastyValue);

        _log.DebugLog(LOG_TAG, "Applying Power Up PINK with tasty value = " + tastyValue, gameObject);
    }
    #endregion
}