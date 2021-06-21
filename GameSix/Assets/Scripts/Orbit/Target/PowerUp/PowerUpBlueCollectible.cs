using UnityEngine;

public class PowerUpBlueCollectible : MonoBehaviour, IPowerUpCollectible
{
    #region Private Constants
    private const string LOG_TAG = "PowerUpBlueCollectible";
    private const int MIN_BOMB_COUNTDOWN = 3;
    private const int MAX_BOMB_COUNTDOWN = 9;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion

    #region API
    public void OnPowerUpColectibleTaken(PlayerController playerController, Hud hud)
    {
        int bombCountdown = Random.Range(MIN_BOMB_COUNTDOWN, MAX_BOMB_COUNTDOWN + 1);
        playerController.ApplyPowerUpBlue(bombCountdown);
        hud.ShowPowerUp(ObjectPooler.POWER_UP_BLUE_KEY, bombCountdown);

        _log.DebugLog(LOG_TAG, "Applying Power Up BLUE with bombCountdown = " + bombCountdown, gameObject);
    }
    #endregion
}
