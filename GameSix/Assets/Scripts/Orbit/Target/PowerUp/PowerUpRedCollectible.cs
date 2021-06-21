using UnityEngine;

public class PowerUpRedCollectible : MonoBehaviour, IPowerUpCollectible
{
    #region Private Constants
    private const string LOG_TAG = "PowerUpRedCollectible";
    private const int MIN_SHIELDS = 1;
    private const int MAX_SHIELDS = 4;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion

    #region API
    public void OnPowerUpColectibleTaken(PlayerController playerController, Hud hud)
    {
        int shields = Random.Range(MIN_SHIELDS, MAX_SHIELDS + 1);
        playerController.ApplyPowerUpRed(shields);
        hud.ShowPowerUp(ObjectPooler.POWER_UP_RED_KEY, shields);

        _log.DebugLog(LOG_TAG, "Applying Power Up RED with shields = " + shields, gameObject);
    }
    #endregion
}
