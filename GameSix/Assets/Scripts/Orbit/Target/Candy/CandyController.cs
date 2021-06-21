using UnityEngine;

public class CandyController : MonoBehaviour, ICollectible
{
    #region Public Constants
    public const string TAG_CANDY = "Candy";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "CandyController";

    private const float CHANGE_ANIMATION_TIME = 3.0f;
    private const string TYPE_ANIM_PARAM = "Type";
    private const int MIN_TYPE_ANIM = 1;
    private const int MAX_TYPE_ANIM = 4;
    private const int CANDY_SCORE = 1;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private Animator _animator;
    private float _changeAnimationTime;
    private Hud _hud;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _hud = GameObject.FindGameObjectWithTag(Hud.TAG_HUD).GetComponent<Hud>();
        _changeAnimationTime = CHANGE_ANIMATION_TIME;
    }

    private void LateUpdate()
    {
        _changeAnimationTime += Time.deltaTime;
        if (_changeAnimationTime >= CHANGE_ANIMATION_TIME)
        {
            _animator.SetInteger(TYPE_ANIM_PARAM, Random.Range(MIN_TYPE_ANIM, MAX_TYPE_ANIM + 1));
            _changeAnimationTime = 0;
        }
    }
    #endregion

    #region API
    public void PublicInit(string powerUpName)
    {
        gameObject.name = powerUpName;
    }

    public void Take(PlayerController playerController)
    {
        playerController.TakeCandy();
        _hud.AddScoreBouncing(CANDY_SCORE);
        gameObject.SetActive(false);
    }
    #endregion
}
