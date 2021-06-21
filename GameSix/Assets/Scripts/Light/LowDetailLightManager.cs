using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowDetailLightManager : MonoBehaviour, ILightManager
{
    #region Private Constants
    private const string LOG_TAG = "LowDetailLightManager";
    private const string NAME_SPRITE_LIGHT_CENTER = "SpriteLightCenter";
    private const float TRANSITION_TIME = 3.0f;
    private static readonly Color DEFAULT = Color.white;
    private static readonly Color TRANSPARENT = new Color(DEFAULT.r, DEFAULT.g, DEFAULT.b, 0);
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private SpriteRenderer[] _spParallaxBackgroundCol;
    private SpriteRenderer _spCenterLight;

    private bool _backgroundLightAvailable;
    private bool _centerLightAvailable;

    private List<Color> _colorPallete;
    private Color _currentColorBackgroundLight;
    private Color _currentColorCenterLight;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        GameObject parallaxBackground = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_PARALLAX_BACKGROUND);
        _spParallaxBackgroundCol = parallaxBackground.transform.GetComponentsInChildren<SpriteRenderer>();

        GameObject spriteLight = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_SPRITE_LIGHT);
        _spCenterLight = spriteLight.transform.Find(NAME_SPRITE_LIGHT_CENTER).gameObject.GetComponent<SpriteRenderer>();

        _colorPallete = new List<Color>
        {
            Color.yellow,
            Color.green,
            Color.red,
            Color.magenta,
            Color.cyan,
            Color.gray
        };
    }

    private void Start()
    {
        PublicInit();
    }
    #endregion

    #region Utils
    private IEnumerator TurnOnOffBackgroundLight(SpriteRenderer[] spBackgroundLightCol, bool turnOn, Color turnOnColor)
    {
        _backgroundLightAvailable = false;

        float timeSinceStarted = 0;
        float percentageCompleted;
        Color currentColor = spBackgroundLightCol[0].color;
        Color finalColor = turnOn ? turnOnColor : DEFAULT;

        do
        {
            percentageCompleted = timeSinceStarted / TRANSITION_TIME;

            foreach (SpriteRenderer sp in spBackgroundLightCol)
            {
                sp.color = Color.Lerp(currentColor, finalColor, percentageCompleted);
            }

            timeSinceStarted += Time.deltaTime;

            yield return null;
        } while (percentageCompleted < 1.0f);

        _backgroundLightAvailable = true;
    }

    private IEnumerator TurnOnOffCenterLight(SpriteRenderer spCenterLightlight, bool turnOn, Color turnOnColor)
    {
        _centerLightAvailable = false;

        float timeSinceStarted = 0;
        float percentageCompleted;
        Color currentColor = spCenterLightlight.color;
        Color finalColor = turnOn ? turnOnColor : TRANSPARENT;

        do
        {
            percentageCompleted = timeSinceStarted / TRANSITION_TIME;

            spCenterLightlight.color = Color.Lerp(currentColor, finalColor, percentageCompleted);

            timeSinceStarted += Time.deltaTime;

            yield return null;
        } while (percentageCompleted < 1.0f);

        _centerLightAvailable = true;
    }

    private Color GetFinalColor(Color currentColor)
    {
        Color randomColor;
        do
        {
            randomColor = GetRandomColor();
        } while (IsEqual(currentColor, randomColor) && _colorPallete.Count > 1);
        return randomColor;
    }

    private Color GetRandomColor()
    {
        return _colorPallete[Random.Range(0, _colorPallete.Count)];
    }

    private bool IsEqual(Color c1, Color c2)
    {
        return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
    }

    private IEnumerator EndlessLightsLoop()
    {
        while (true)
        {
            Party();
            yield return new WaitForSeconds(TRANSITION_TIME);
        }
    }
    #endregion

    #region API
    public void TurnOnLeftSpotlight()
    {
        if (_backgroundLightAvailable)
        {
            Color finalColor = GetFinalColor(_currentColorBackgroundLight);
            StartCoroutine(TurnOnOffBackgroundLight(_spParallaxBackgroundCol, true, finalColor));
            _currentColorBackgroundLight = finalColor;
        }
    }

    public void TurnOnRightSpotlight()
    {
        TurnOnLeftSpotlight();
    }

    public void TurnOnGlobalLight()
    {
        if (_backgroundLightAvailable)
        {
            StartCoroutine(TurnOnOffBackgroundLight(_spParallaxBackgroundCol, false, DEFAULT));
        }
    }

    public void TurnOnCenterSpotlight()
    {
        if (_centerLightAvailable)
        {
            Color finalColor = GetFinalColor(_currentColorCenterLight);
            StartCoroutine(TurnOnOffCenterLight(_spCenterLight, true, finalColor));
            _currentColorCenterLight = finalColor;
        }
    }

    public void TurnOffCenterspotlight()
    {
        if (_centerLightAvailable)
        {
            StartCoroutine(TurnOnOffCenterLight(_spCenterLight, false, DEFAULT));
        }
    }

    public void PublicInit()
    {
        foreach (SpriteRenderer sp in _spParallaxBackgroundCol)
        {
            sp.color = DEFAULT;
        }
        _spCenterLight.color = TRANSPARENT;

        _backgroundLightAvailable = true;
        _centerLightAvailable = true;

        _currentColorBackgroundLight = DEFAULT;
        _currentColorCenterLight = DEFAULT;
    }

    public void Party()
    {
        int random = Random.Range(1, 4);
        _log.DebugLog(LOG_TAG, "Random = " + random + ".", gameObject);

        switch (random)
        {
            case 1:
                _log.DebugLog(LOG_TAG, "Left spot.", gameObject);
                TurnOnLeftSpotlight();
                break;
            case 2:
                _log.DebugLog(LOG_TAG, "Right spot.", gameObject);
                TurnOnRightSpotlight();
                break;
            case 3:
                _log.DebugLog(LOG_TAG, "Game light.", gameObject);
                TurnOnGlobalLight();
                break;
            default:
                break;
        }

        if (Random.value <= 0.5f)
        {
            _log.DebugLog(LOG_TAG, "Center light on.", gameObject);
            TurnOnCenterSpotlight();
        }
        else
        {
            _log.DebugLog(LOG_TAG, "Center light off.", gameObject);
            TurnOffCenterspotlight();
        }
    }

    public void EndlessParty()
    {
        StartCoroutine(EndlessLightsLoop());
    }
    #endregion
}
