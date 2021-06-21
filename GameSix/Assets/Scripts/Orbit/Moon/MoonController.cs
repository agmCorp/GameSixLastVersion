using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpringJoint2D))]
public class MoonController : MonoBehaviour
{
    #region State machine
    private enum State
    {
        INITIAL,
        ORBIT,
        SWING,
        RESTORE,
        FLEE,
        SHRINK,
        DISPOSE
    }
    #endregion

    #region Public Constants
    public const string TAG_MOON = "Moon";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "MoonController";
    private const float SHRINK_SCALE = 1.0f / 6.0f;
    private const string NAME_HIT_BOX = "HitBox";
    private const float CHANGE_ANIMATION_TIME = 3.0f;
    private const string COLLISION_ANIM_PARAM = "Collision";
    private const string FAINT_ANIM_PARAM = "Faint";
    private const string CRY_ANIM_PARAM = "Cry";
    private const string SLEEP_ANIM_PARAM = "Sleep";
    private const string SLEEP_OFFSET_ANIM_PARAM = "SleepRandomOffset";
    private const string TYPE_ANIM_PARAM = "Type";
    private const int MIN_TYPE_ANIM = 1;
    private const int MAX_TYPE_ANIM = 5;
    private const float SLOW_DOWN_SCALE = 0.4f; // 40% original speed
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private State _currentState;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private CircleCollider2D _hitBoxCollider;
    private GameObject _planet;
    private float _initialRadius;
    private float _minRadius;
    private float _maxRadius;
    private bool _increasing;
    private float _swingingSpeed;
    private float _deltaSwingingSpeed;
    private float _swingDuration;
    private float _pauseDuration;
    private float _initialTangentialSpeed;
    private bool _clockwise;
    private bool _accelerationEnabled;
    private float _maxTangentialSpeed;
    private float _tangentialAcceleration;
    private float _accumulator;
    private float _accSign;
    private float _dirSign;
    private bool _bounce;
    private float _fleeSpeed;
    private float _timeSinceStarted;
    private float _shrinkTime;
    private SpringJoint2D _rope;
    private bool _sleepAnimation;
    private bool _faintAnimation;
    private bool _cryAnimation;
    private float _changeAnimationTime;
    private float _sleepRandomOffset;
    private bool _slowedDown;
    private bool _simulate;
    private ShadowCaster2D _shadowCaster;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _shadowCaster = GetComponent<ShadowCaster2D>();
        _animator = GetComponent<Animator>();
        _rope = GetComponent<SpringJoint2D>();

        // HitBox's collider (size of this gameObject)
        _hitBoxCollider = transform.Find(NAME_HIT_BOX).GetComponent<CircleCollider2D>();
    }

    // Reusing GameObject
    private void OnEnable()
    {
        // Performance
        _simulate = false;
        _rigidbody.simulated = _simulate;
        _shadowCaster.enabled = _simulate;
        _animator.enabled = _simulate;
        _hitBoxCollider.enabled = true;
        _rope.enabled = true;
        transform.localScale = Vector3.one;
        _currentState = State.INITIAL;
        InitAnimation();
    }

    private void Update()
    {
        // Calls functions that don't involve physics
        switch (_currentState)
        {
            case State.INITIAL:
                break;
            case State.ORBIT:
                break;
            case State.SWING:
                break;
            case State.RESTORE:
                break;
            case State.FLEE:
                break;
            case State.SHRINK:
                StateShrink();
                break;
            case State.DISPOSE:
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        // Calls functions involving physics
        switch (_currentState)
        {
            case State.INITIAL:
                break;
            case State.ORBIT:
                StateOrbit();
                break;
            case State.SWING:
                StateSwing();
                break;
            case State.RESTORE:
                StateRestore();
                break;
            case State.FLEE:
                StateFlee();
                break;
            case State.SHRINK:
                break;
            case State.DISPOSE:
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        _animator.SetBool(SLEEP_ANIM_PARAM, _sleepAnimation);
        _animator.SetFloat(SLEEP_OFFSET_ANIM_PARAM, _sleepRandomOffset);
        _animator.SetBool(COLLISION_ANIM_PARAM, _faintAnimation || _cryAnimation);
        _animator.SetBool(FAINT_ANIM_PARAM, _faintAnimation);
        _animator.SetBool(CRY_ANIM_PARAM, _cryAnimation);
        _changeAnimationTime += Time.deltaTime;
        if (_changeAnimationTime >= CHANGE_ANIMATION_TIME)
        {
            _animator.SetInteger(TYPE_ANIM_PARAM, Random.Range(MIN_TYPE_ANIM, MAX_TYPE_ANIM + 1));
            _changeAnimationTime = 0;
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    // Regardless of whether the renderer of ANY Moon or the renderer of the Planet became visible by the 
    // camera, the entire orbit is activated. See PlanetController.OnBecameVisible().
    private void OnBecameVisible()
    {
        // Performance
        if (!_simulate)
        {
            _planet.GetComponent<PlanetController>().SimulateOrbit();
        }
    }
    #endregion

    #region Utils
    private void ConfigRope()
    {
        _rope.connectedBody = _planet.GetComponent<Rigidbody2D>();
        _rope.distance = _initialRadius;
    }

    private void GoRound()
    {
        Vector2 centripetalForceDirection = (_planet.transform.position - transform.position).normalized;
        Vector2 tangentialVelocityDirection = Vector2.Perpendicular(centripetalForceDirection);
        _rigidbody.velocity = tangentialVelocityDirection * _initialTangentialSpeed;

        if (_accelerationEnabled)
        {
            _accumulator += _accSign * Time.fixedDeltaTime;
            float newTangentialSpeed = _initialTangentialSpeed + _tangentialAcceleration * _accumulator;
            newTangentialSpeed = Mathf.Clamp(newTangentialSpeed, _initialTangentialSpeed, _maxTangentialSpeed);
            _rigidbody.velocity = _dirSign * tangentialVelocityDirection * newTangentialSpeed;
            if (newTangentialSpeed == _maxTangentialSpeed || newTangentialSpeed == _initialTangentialSpeed)
            {
                _accSign *= -1;
                if (newTangentialSpeed == _initialTangentialSpeed)
                {
                    _dirSign = (_bounce) ? _dirSign *= -1 : _dirSign;
                }
            }
        }
        _rigidbody.velocity = _clockwise ? _rigidbody.velocity : -_rigidbody.velocity;
    }

    private void StateOrbit()
    {
        GoRound();
    }

    private void StateSwing()
    {
        GoRound();

        _deltaSwingingSpeed = _swingingSpeed * Time.deltaTime;
        if (_increasing)
        {
            _rope.distance += _deltaSwingingSpeed;
            if (_maxRadius <= _rope.distance)
            {
                _rope.distance = _maxRadius;
                _increasing = false;
            }
        }
        else
        {
            _rope.distance -= _deltaSwingingSpeed;
            if (_rope.distance <= _minRadius)
            {
                _rope.distance = _minRadius;
                _increasing = true;
            }
        }
    }

    private void StateRestore()
    {
        GoRound();

        _deltaSwingingSpeed = _swingingSpeed * Time.deltaTime;
        if (_increasing)
        {
            _rope.distance += _deltaSwingingSpeed;
            if (_initialRadius <= _rope.distance)
            {
                _rope.distance = _initialRadius;
                _currentState = State.ORBIT;
            }
        }
        else
        {
            _rope.distance -= _deltaSwingingSpeed;
            if (_rope.distance <= _initialRadius)
            {
                _rope.distance = _initialRadius;
                _currentState = State.ORBIT;
            }
        }
    }

    private void StateFlee()
    {
        _hitBoxCollider.enabled = false;
        _rope.enabled = false;
        Vector2 centripetalForceDirection = (_planet.transform.position - transform.position).normalized;
        _rigidbody.velocity = -centripetalForceDirection * _fleeSpeed;
        _timeSinceStarted = 0;
        _currentState = State.SHRINK;
        FaintAnimation();
    }

    private void StateShrink()
    {
        // Change moon's size over time
        _timeSinceStarted += Time.deltaTime;
        float _percentageCompleted = _timeSinceStarted / _shrinkTime;
        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * SHRINK_SCALE, _percentageCompleted);
        if (_percentageCompleted >= 1.0f)
        {
            _currentState = State.DISPOSE;
        }
    }

    private void RestartSwinging()
    {
        StartSwingingWithPause(_minRadius, _maxRadius, _swingingSpeed, _swingDuration, _pauseDuration);
    }

    private void InitAnimation()
    {
        _sleepAnimation = false;
        _faintAnimation = false;
        _cryAnimation = false;
        _changeAnimationTime = CHANGE_ANIMATION_TIME;
        _slowedDown = false;
    }
    #endregion

    #region API
    public void PublicInit(GameObject planet, string moonName)
    {
        _planet = planet;
        gameObject.name = moonName;
        transform.SetParent(planet.transform);
        _initialRadius = Vector2.Distance(planet.transform.position, transform.position);
        ConfigRope();
    }

    public float GetRadius()
    {
        return _hitBoxCollider.radius;
    }

    public void Orbit(float initialTangentialSpeed, bool clockwise)
    {
        _accelerationEnabled = false;
        _initialTangentialSpeed = initialTangentialSpeed;
        _clockwise = clockwise;
        _currentState = State.ORBIT;
    }

    public void Orbit(float initialTangentialSpeed, bool clockwise, float maxTangentialSpeed, float tangentialAcceleration, bool bounce)
    {
        _accelerationEnabled = true;
        _initialTangentialSpeed = initialTangentialSpeed;
        _clockwise = clockwise;
        _maxTangentialSpeed = maxTangentialSpeed;
        _tangentialAcceleration = tangentialAcceleration;
        _accumulator = 0;
        _accSign = 1;
        _dirSign = 1;
        _bounce = bounce;
        _currentState = State.ORBIT;
    }

    public void StartSwinging(float minRadius, float maxRadius, float swingingSpeed)
    {
        _minRadius = Mathf.Min(maxRadius, minRadius);
        _maxRadius = Mathf.Max(maxRadius, minRadius);
        _swingingSpeed = swingingSpeed;
        _increasing = _rope.distance <= _maxRadius;
        _currentState = State.SWING;
    }

    public void StartSwingingWithPause(float minRadius, float maxRadius, float swingingSpeed, float swingDuration, float pauseDuration)
    {
        _swingDuration = swingDuration;
        _pauseDuration = pauseDuration;
        StartSwinging(minRadius, maxRadius, swingingSpeed);
        Invoke(nameof(StopSwinging), _swingDuration);
        Invoke(nameof(RestartSwinging), _swingDuration + _pauseDuration);
    }

    public void StopSwinging()
    {
        _increasing = _rope.distance <= _initialRadius;
        _currentState = State.RESTORE;
    }

    public void StartFlee()
    {
        CancelInvoke();
        _currentState = State.FLEE;
    }

    public void SetFlee(float fleeSpeed, float shrinkTime)
    {
        _fleeSpeed = fleeSpeed;
        _shrinkTime = shrinkTime;
    }

    public void SlowDown()
    {
        _swingingSpeed *= SLOW_DOWN_SCALE;
        _initialTangentialSpeed *= SLOW_DOWN_SCALE;
        _maxTangentialSpeed *= SLOW_DOWN_SCALE;
        _tangentialAcceleration *= SLOW_DOWN_SCALE;
        _swingDuration /= SLOW_DOWN_SCALE;
        _pauseDuration /= SLOW_DOWN_SCALE;
        _slowedDown = true;
        SleepAnimation();
    }

    public void SleepAnimation()
    {
        _sleepAnimation = true;
        _sleepRandomOffset = Random.value;
    }

    public void IdleAnimation()
    {
        if (!_slowedDown)
        {
            _sleepAnimation = false;
            _changeAnimationTime = CHANGE_ANIMATION_TIME;
        }
    }

    public void FaintAnimation()
    {
        _sleepAnimation = false;
        _faintAnimation = true;
    }

    public void CryAnimation()
    {
        _cryAnimation = true;
    }

    public void Simulate()
    {
        // Performance
        _simulate = true;
        _rigidbody.simulated = _simulate;
        _shadowCaster.enabled = _simulate && (PlayerPrefs.GetInt(GlobalConstants.PREF_HIGH_DETAIL_ON) != 0);
        _animator.enabled = _simulate;
    }

    public void PauseAnimations(bool pause)
    {
        _animator.updateMode = pause ? AnimatorUpdateMode.Normal : AnimatorUpdateMode.UnscaledTime;
    }
    #endregion
}
