using UnityEngine;

public class PowerUpYellowCollectible : MonoBehaviour, IPowerUpCollectible
{
    #region Private Constants
    private const string LOG_TAG = "PowerUpYellowCollectible";
    private const int MIN_POTIONS = 1;
    private const int MAX_POTIONS = 4;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion

    #region API
    public void OnPowerUpColectibleTaken(PlayerController playerController, Hud hud)
    {
        int potions = Random.Range(MIN_POTIONS, MAX_POTIONS + 1);
        playerController.ApplyPowerUpYellow(potions);
        hud.ShowPowerUp(ObjectPooler.POWER_UP_YELLOW_KEY, potions);

        _log.DebugLog(LOG_TAG, "Applying Power Up YELLOW with potions = " + potions, gameObject);
    }
    #endregion
}