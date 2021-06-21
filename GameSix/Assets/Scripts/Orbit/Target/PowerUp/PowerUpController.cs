using System.Collections;
using UnityEngine;

public class PowerUpController : MonoBehaviour, ICollectible
{
    #region Public Constants
    public const string TAG_POWER_UP = "PowerUp";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "PowerUpController";

    private const string NAME_PACKAGE = "Package";
    private const string NAME_COLLECTIBLE = "Collectible";
    private const string HURT_BOX_NAME = "HurtBox";
    private const float CHANGE_ANIMATION_TIME = 3.0f;
    private const string TYPE_ANIM_PARAM = "Type";
    private const int MIN_TYPE_ANIM = 1;
    private const int MAX_TYPE_ANIM = 4;
    private const float FADE_OUT_TIME = 3.0f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private CircleCollider2D _hurtBox;
    private GameObject _package;
    private GameObject _collectible;
    private Animator _animatorPackage;
    private float _changeAnimationTime;
    private Hud _hud;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _hurtBox = transform.Find(HURT_BOX_NAME).gameObject.GetComponent<CircleCollider2D>();
        _package = transform.Find(NAME_PACKAGE).gameObject;
        _collectible = transform.Find(NAME_COLLECTIBLE).gameObject;
        _animatorPackage = _package.GetComponent<Animator>();
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
            _animatorPackage.SetInteger(TYPE_ANIM_PARAM, Random.Range(MIN_TYPE_ANIM, MAX_TYPE_ANIM + 1));
            _changeAnimationTime = 0;
        }
    }
    #endregion

    #region Utils
    private IEnumerator FadeOut(GameObject collectible, float FadeOutTime)
    {
        SpriteRenderer spriteRenderer = collectible.GetComponent<SpriteRenderer>();
        float timeSinceStarted = 0;
        float percentageCompleted;

        Color initialColor = Color.white;
        Color finalColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        do
        {
            percentageCompleted = timeSinceStarted / FadeOutTime;

            spriteRenderer.color = Color.Lerp(initialColor, finalColor, percentageCompleted);

            timeSinceStarted += Time.deltaTime;

            yield return null;
        } while (percentageCompleted < 1.0f);

        // Disables collectible and undo changes
        collectible.SetActive(false);
        spriteRenderer.color = initialColor;
    }
    #endregion

    #region API
    public void PublicInit(string powerUpName)
    {
        gameObject.name = powerUpName;
        _hurtBox.enabled = true;
        _package.SetActive(true);
        _collectible.SetActive(false);
    }

    public void Take(PlayerController playerController)
    {
        _log.DebugLog(LOG_TAG, "PowerUpController.Take", gameObject);

        _hurtBox.enabled = false;
        _package.SetActive(false);
        _collectible.SetActive(true);
        StartCoroutine(FadeOut(_collectible, FADE_OUT_TIME));
        IPowerUpCollectible powerUpCollectible = _collectible.GetComponent<IPowerUpCollectible>();
        powerUpCollectible.OnPowerUpColectibleTaken(playerController, _hud);
    }
    #endregion
}
