using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "PlayerHealth";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private ChallengeManager _challengeManager;
    private GameManager _gameManager;
    private bool _outsideTheTarget;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _challengeManager = GameObject.FindGameObjectWithTag(ChallengeManager.TAG_CHALLENGE_MANAGER).GetComponent<ChallengeManager>();
        _gameManager = GameObject.FindGameObjectWithTag(GameManager.TAG_GAME_MANAGER).GetComponent<GameManager>();
        _outsideTheTarget = true;
    }
    #endregion

    #region Utils
    private void GameOver()
    {
        if (!GlobalConstants.ENABLE_CHEAT_MODE)
        {
            _gameManager.GameOver();
        }
    }

    private void MoonCollisionNoPower(MoonController moonController)
    {
        moonController.FaintAnimation();
        GameOver();
    }

    private void MoonCollisionPowerOrange(MoonController moonController)
    {
        moonController.FaintAnimation();
    }

    private void MoonCollisionPowerRed(PlayerController playerController, MoonController moonController)
    {
        moonController.CryAnimation();
        playerController.AimBackward();
    }

    private void MoonCollisionPowerGreen(MoonController moonController)
    {
        moonController.FaintAnimation();
        GameOver();
    }

    private void MoonCollisionPowerYellow(MoonController moonController)
    {
        moonController.FaintAnimation();
        GameOver();
    }

    private void MoonCollisionPowerPink(MoonController moonController)
    {
        moonController.FaintAnimation();
        GameOver();
    }

    private void MoonCollisionPowerBlue(MoonController moonController)
    {
        moonController.FaintAnimation();
        GameOver();
    }

    private bool IsPlayerVulnerable(PlayerController playerController)
    {
        return playerController.IsJumpForward() && _outsideTheTarget;
    }
    #endregion

    #region API

    /*
     * All the collisions in the game are centralized in this class
     */

    public void MoonCollision(PlayerController playerController, MoonController moonController)
    {
        _log.DebugLog(LOG_TAG, "PlayerHealth.MoonCollision _outsideTheTarget: " + _outsideTheTarget, gameObject);
        playerController.LogState();

        if (IsPlayerVulnerable(playerController))
        {
            if (playerController.NoPower())
            {
                MoonCollisionNoPower(moonController);
            }
            else if (playerController.IsCurrentPowerOrange())
            {
                MoonCollisionPowerOrange(moonController);
            }
            else if (playerController.IsCurrentPowerRed())
            {
                MoonCollisionPowerRed(playerController, moonController);
            }
            else if (playerController.IsCurrentPowerGreen())
            {
                MoonCollisionPowerGreen(moonController);
            }
            else if (playerController.IsCurrentPowerYellow())
            {
                MoonCollisionPowerYellow(moonController);

            }
            else if (playerController.IsCurrentPowerPink())
            {
                MoonCollisionPowerPink(moonController);

            }
            else if (playerController.IsCurrentPowerBlue())
            {
                MoonCollisionPowerBlue(moonController);
            }
            else
            {
                _log.DebugLogError(LOG_TAG, "PlayerController has an unknow power!", gameObject);
            }
        }
    }

    public void PowerUpCollision(PlayerController playerController, ICollectible powerUpController)
    {
        _log.DebugLog(LOG_TAG, "PlayerHealth.PowerUpCollision _outsideTheTarget: " + _outsideTheTarget, gameObject);
        playerController.LogState();

        if (IsPlayerVulnerable(playerController) || playerController.IsIdle())
        {
            powerUpController.Take(playerController);
        }
    }

    public void CandyCollision(PlayerController playerController, ICollectible candyController)
    {
        _log.DebugLog(LOG_TAG, "PlayerHealth.CandyCollision _outsideTheTarget: " + _outsideTheTarget, gameObject);
        playerController.LogState();

        if (IsPlayerVulnerable(playerController) || playerController.IsIdle())
        {
            candyController.Take(playerController);
        }
    }

    public void TargetCollisionEnter(PlayerController playerController, TargetController targetController)
    {
        _log.DebugLog(LOG_TAG, "PlayerHealth.TargetCollisionEnter _outsideTheTarget: " + _outsideTheTarget, gameObject);
        playerController.LogState();

        if (IsPlayerVulnerable(playerController) || playerController.IsJumpBackward())
        {
            if (!playerController.IsJumpBackward())
            {
                _challengeManager.ChallengeCompleted();
            }
            playerController.Success(targetController);
            targetController.BounceAnimation();
            _outsideTheTarget = false;
        }
    }

    public void TargetCollisionExit(PlayerController playerController)
    {
        _log.DebugLog(LOG_TAG, "PlayerHealth.TargetCollisionExit _outsideTheTarget: " + _outsideTheTarget, gameObject);
        playerController.LogState();

        _outsideTheTarget = true;
    }
    #endregion
}
