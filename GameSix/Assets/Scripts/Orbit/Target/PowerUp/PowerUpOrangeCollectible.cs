using UnityEngine;

public class PowerUpOrangeCollectible : MonoBehaviour, IPowerUpCollectible
{
    #region Private Constants
    private const string LOG_TAG = "PowerUpOrangeCollectible";
    private const int MIN_CONSECUTIVE_TARGETS = 1;
    private const int MAX_CONSECUTIVE_TARGETS = 6;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion

    #region API
    public void OnPowerUpColectibleTaken(PlayerController playerController, Hud hud)
    {
        int targetsInARow = Random.Range(MIN_CONSECUTIVE_TARGETS, MAX_CONSECUTIVE_TARGETS + 1);
        playerController.ApplyPowerUpOrange(targetsInARow);
        hud.ShowPowerUp(ObjectPooler.POWER_UP_ORANGE_KEY, targetsInARow);

        _log.DebugLog(LOG_TAG, "Applying Power Up ORANGE with targetsInARow = " + targetsInARow, gameObject);
    }
    #endregion
}
