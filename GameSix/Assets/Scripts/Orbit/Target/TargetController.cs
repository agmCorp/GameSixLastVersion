using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    #region Public Constants
    public const string TAG_TARGET = "Target";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "TargetController";
    private const string BOUNCE_ANIM_PARAM = "Bounce";
    private const string POWER_UP_NAME = "PowerUp";
    private const string CANDY_NAME = "Candy";
    private const float POWER_UP_PROBABILITY = 0.2f;    // Probability of 20% that a PowerUp turns up.
    private const float CANDY_PROBABILITY = 0.4f;       // Probability of 40% that a Candy turns up.
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private ObjectPooler _objectPooler;
    private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;
    private NumberRenderer _numberRenderer;
    private Animator _animator;
    private GameObject _collectible;
    private List<string> _powerUpList;
    private bool _isLevel;
    [SerializeField] private Sprite _challengeSprite = null;
    [SerializeField] private Sprite _levelSprite = null;
    [SerializeField] private Sprite _goalSprite = null;

    private bool _bounceAnimation;
    #endregion

    #region Properties
    public bool IsLevel
    {
        get { return _isLevel; }
        set { _isLevel = value; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _numberRenderer = GetComponentInChildren<NumberRenderer>();
        _animator = GetComponentInChildren<Animator>();
        _powerUpList = new List<string>
        {
            ObjectPooler.POWER_UP_RED_KEY,
            ObjectPooler.POWER_UP_ORANGE_KEY,
            ObjectPooler.POWER_UP_GREEN_KEY,
            ObjectPooler.POWER_UP_YELLOW_KEY,
            ObjectPooler.POWER_UP_PINK_KEY,
            ObjectPooler.POWER_UP_BLUE_KEY
        };
    }

    private void OnEnable()
    {
        _objectPooler = ObjectPooler.GetInstance();
    }

    private void LateUpdate()
    {
        _animator.SetBool(BOUNCE_ANIM_PARAM, _bounceAnimation);
        _bounceAnimation = false;
    }
    #endregion

    #region Utils
    private void SetCollectible()
    {
        float rnd = Random.value;
        if (rnd <= POWER_UP_PROBABILITY)
        {
            SetPowerUp();
        }
        else if (rnd <= POWER_UP_PROBABILITY + CANDY_PROBABILITY)
        {
            SetCandy();
        }
    }

    private void SetPowerUp()
    {
        if (!GlobalConstants.ENABLE_POWERLESS)
        {
            string randomKey = _powerUpList[Random.Range(0, _powerUpList.Count)];
            _collectible = GetCollectibleFromPool(randomKey, POWER_UP_NAME);
        }
    }

    private void SetCandy()
    {
        _collectible = GetCollectibleFromPool(ObjectPooler.CANDY_KEY, CANDY_NAME);
    }

    private GameObject GetCollectibleFromPool(string collectibleKey, string collectibleName)
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        GameObject collectible = _objectPooler.SpawnFromPool(collectibleKey, pos, Quaternion.identity);
        collectible.transform.parent = gameObject.transform;
        collectible.GetComponent<ICollectible>().PublicInit(collectibleName);
        return collectible;
    }

    private void ReturnCollectibleToPool(GameObject collectible)
    {
        _objectPooler.ReturnToPool(collectible);
    }
    #endregion

    #region API
    public void PublicInit(string targetName, Vector2 pole, bool isLevel, int levelNumber, int maxLevel)
    {
        gameObject.name = targetName;
        _isLevel = isLevel;

        // Renders a dotted line
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, pole);
        _lineRenderer.SetPosition(1, transform.position);

        // Renders a Level Number
        if (_isLevel)
        {
            if (levelNumber < maxLevel)
            {
                _spriteRenderer.sprite = _levelSprite;
                _numberRenderer.RenderNumber(levelNumber);
            } else
            {
                _spriteRenderer.sprite = _goalSprite;
            }
        }
        else
        {
            _spriteRenderer.sprite = _challengeSprite;
            SetCollectible();
        }
    }

    public void RemoveDottedLines()
    {
        _lineRenderer.positionCount = 0;
    }

    public void BounceAnimation()
    {
        _bounceAnimation = true;
    }

    public void Dispose()
    {
        /* 
         * WA: If Player moves too fast, this Target can be returned to the pool before its animation ends.
         * As a consequence, Circle's scale remains at a random value (see TargetBouncing clip).
         * Since the scale is controlled by Animator, Unity doesn't allow it to be changed, so we must temporarily 
         * disable the Animator.
         */
        _animator.enabled = false;
        _spriteRenderer.gameObject.transform.localScale = Vector3.one;

        _numberRenderer.Dispose();
        if (_collectible != null)
        {
            ReturnCollectibleToPool(_collectible);
            _collectible = null;
        }

        _bounceAnimation = false;
        _animator.enabled = true;
    }
    #endregion
}
