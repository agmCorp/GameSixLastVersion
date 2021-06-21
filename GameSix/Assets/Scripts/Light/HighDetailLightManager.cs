using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class HighDetailLightManager : MonoBehaviour, ILightManager
{
    #region Private Constants
    private const string LOG_TAG = "HighDetailLightManager";

    private const string NAME_SPOTLIGHT_LEFT = "SpotlightLeft";
    private const string NAME_SPOTLIGHT_RIGHT = "SpotlightRight";
    private const string NAME_SPOTLIGHT_CENTER = "SpotlightCenter";

    private const float TRANSITION_TIME = 3.0f;
    private const float LIGHT_MAX_INTENSITY = 1.0f;
    private const float SHADOW_MAX_INTENSITY = 0.1f;
    private const float CENTER_LIGHT_MAX_INTENSITY = 4.0f;
    private static readonly Color DEFAULT = Color.white;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private Light2D _spotlightLeft;
    private Light2D _spotlightRight;
    private Light2D _spotlightCenter;

    private Light2D _globalLight;

    private List<Color> _colorPallete;
    private Color _currentColorCornerSpotlights;
    private Color _currentColorCenterSpotLight;
    private float _shadowMaxIntensity;

    private bool _spotlightLeftAvailable;
    private bool _spotlightRightAvailable;
    private bool _spotlightCenterAvailable;
    private bool _globalLightAvailable;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        GameObject spotlight = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_SPOTLIGHT);
        _spotlightLeft = spotlight.transform.Find(NAME_SPOTLIGHT_LEFT).gameObject.GetComponent<Light2D>();
        _spotlightRight = spotlight.transform.Find(NAME_SPOTLIGHT_RIGHT).gameObject.GetComponent<Light2D>();
        _spotlightCenter = spotlight.transform.Find(NAME_SPOTLIGHT_CENTER).gameObject.GetComponent<Light2D>();

        _globalLight = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_GLOBAL_LIGHT).GetComponent<Light2D>();

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
    private IEnumerator TurnOnOffSpotlight(Light2D spotlight, bool turnOn, float maxIntensity, Color turnOnColor)
    {
        CoroutineBegin(spotlight);

        float timeSinceStarted = 0;
        float percentageCompleted;
        Color initialColor = spotlight.color;

        // Want to turn spotlight on
        if (turnOn)
        {
            // Spotlight is turned off
            if (spotlight.intensity == 0)
            {
                do
                {
                    percentageCompleted = timeSinceStarted / TRANSITION_TIME;

                    // Intensity, shadow and color interpolations
                    spotlight.intensity = Mathf.Lerp(0, maxIntensity, percentageCompleted);
                    spotlight.shadowIntensity = Mathf.Lerp(0, _shadowMaxIntensity, percentageCompleted);
                    spotlight.color = Color.Lerp(initialColor, turnOnColor, percentageCompleted);

                    timeSinceStarted += Time.deltaTime;

                    yield return null;
                } while (percentageCompleted < 1.0f);
            }
            else // Spotlight is turned on
            {
                do
                {
                    percentageCompleted = timeSinceStarted / TRANSITION_TIME;

                    // Color interpolations
                    spotlight.color = Color.Lerp(initialColor, turnOnColor, percentageCompleted);

                    timeSinceStarted += Time.deltaTime;

                    yield return null;
                } while (percentageCompleted < 1.0f);
            }
        }
        else // Want to turn spotlight off
        {
            // Spotlight is turned on
            if (spotlight.intensity > 0)
            {
                do
                {
                    percentageCompleted = timeSinceStarted / TRANSITION_TIME;

                    // Intensity, shadow and color interpolations
                    spotlight.intensity = Mathf.Lerp(maxIntensity, 0, percentageCompleted);
                    spotlight.shadowIntensity = Mathf.Lerp(_shadowMaxIntensity, 0, percentageCompleted);
                    spotlight.color = Color.Lerp(initialColor, DEFAULT, percentageCompleted);

                    timeSinceStarted += Time.deltaTime;

                    yield return null;
                } while (percentageCompleted < 1.0f);
            }
        }

        CoroutineEnd(spotlight);
    }

    // Sets which light is now unavailable
    private void CoroutineBegin(Light2D light)
    {
        _spotlightCenterAvailable = _spotlightCenterAvailable ? !ReferenceEquals(light, _spotlightCenter) : _spotlightCenterAvailable;
        _globalLightAvailable = _globalLightAvailable ? !ReferenceEquals(light, _globalLight) : _globalLightAvailable;
        _spotlightLeftAvailable = _spotlightLeftAvailable ? !ReferenceEquals(light, _spotlightLeft) : _spotlightLeftAvailable;
        _spotlightRightAvailable = _spotlightRightAvailable ? !ReferenceEquals(light, _spotlightRight) : _spotlightRightAvailable;
    }

    // Sets which light is now available 
    private void CoroutineEnd(Light2D light)
    {
        _spotlightCenterAvailable = _spotlightCenterAvailable ? _spotlightCenterAvailable : ReferenceEquals(light, _spotlightCenter);
        _globalLightAvailable = _globalLightAvailable ? _globalLightAvailable : ReferenceEquals(light, _globalLight);
        _spotlightLeftAvailable = _spotlightLeftAvailable ? _spotlightLeftAvailable : ReferenceEquals(light, _spotlightLeft);
        _spotlightRightAvailable = _spotlightRightAvailable ? _spotlightRightAvailable : ReferenceEquals(light, _spotlightRight);
    }

    private bool CanSwitchCornerLights()
    {
        return _spotlightLeftAvailable &&
               _spotlightRightAvailable &&
               _globalLightAvailable;
    }

    private bool CanSwitchCenterLight()
    {
        return _spotlightCenterAvailable;
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
    /*
     * Right spotlight, left spotlight and global light mutually exclude each other (only one
     * can be active at a time). The central spotlight is independent.
     * If I turn on a spotlight that is already on, it changes color.
     * 
     * Light transitions take TRANSITION_TIME seconds. While a transition is running, no other
     * transitions will run. Therefore, if TurnOnLeftSpotlight is called and TurnOnRightSpotlight is called
     * immediately, only TurnOnLeftSpotlight will run.
    */
    public void TurnOnLeftSpotlight()
    {
        if (CanSwitchCornerLights())
        {
            Color finalColor = GetFinalColor(_currentColorCornerSpotlights);
            StartCoroutine(TurnOnOffSpotlight(_globalLight, false, LIGHT_MAX_INTENSITY, DEFAULT));
            StartCoroutine(TurnOnOffSpotlight(_spotlightLeft, true, LIGHT_MAX_INTENSITY, finalColor));
            StartCoroutine(TurnOnOffSpotlight(_spotlightRight, false, LIGHT_MAX_INTENSITY, DEFAULT));
            _currentColorCornerSpotlights = finalColor;
        }
    }

    public void TurnOnRightSpotlight()
    {
        if (CanSwitchCornerLights())
        {
            Color finalColor = GetFinalColor(_currentColorCornerSpotlights);
            StartCoroutine(TurnOnOffSpotlight(_globalLight, false, LIGHT_MAX_INTENSITY, DEFAULT));
            StartCoroutine(TurnOnOffSpotlight(_spotlightLeft, false, LIGHT_MAX_INTENSITY, DEFAULT));
            StartCoroutine(TurnOnOffSpotlight(_spotlightRight, true, LIGHT_MAX_INTENSITY, finalColor));
            _currentColorCornerSpotlights = finalColor;
        }
    }

    public void TurnOnGlobalLight()
    {
        if (CanSwitchCornerLights())
        {
            StartCoroutine(TurnOnOffSpotlight(_globalLight, true, LIGHT_MAX_INTENSITY, DEFAULT));
            StartCoroutine(TurnOnOffSpotlight(_spotlightLeft, false, LIGHT_MAX_INTENSITY, DEFAULT));
            StartCoroutine(TurnOnOffSpotlight(_spotlightRight, false, LIGHT_MAX_INTENSITY, DEFAULT));
        }
    }

    public void TurnOnCenterSpotlight()
    {
        if (CanSwitchCenterLight())
        {
            Color finalColor = GetFinalColor(_currentColorCenterSpotLight);
            StartCoroutine(TurnOnOffSpotlight(_spotlightCenter, true, CENTER_LIGHT_MAX_INTENSITY, finalColor));
            _currentColorCenterSpotLight = finalColor;
        }
    }

    public void TurnOffCenterspotlight()
    {
        if (CanSwitchCenterLight())
        {
            StartCoroutine(TurnOnOffSpotlight(_spotlightCenter, false, CENTER_LIGHT_MAX_INTENSITY, DEFAULT));
        }
    }

    public void PublicInit()
    {
        StopAllCoroutines();

        // Left spot
        _spotlightLeft.intensity = 0;
        _spotlightLeft.color = DEFAULT;
        _spotlightLeft.shadowIntensity = 0;

        // Right spot
        _spotlightRight.intensity = 0;
        _spotlightRight.color = DEFAULT;
        _spotlightRight.shadowIntensity = 0;

        // Center spot
        _spotlightCenter.intensity = 0;
        _spotlightCenter.color = DEFAULT;
        _spotlightCenter.shadowIntensity = 0;

        // Global light
        _globalLight.intensity = LIGHT_MAX_INTENSITY;
        _globalLight.color = DEFAULT;
        _globalLight.shadowIntensity = 0;

        _spotlightLeftAvailable = true;
        _spotlightRightAvailable = true;
        _spotlightCenterAvailable = true;
        _globalLightAvailable = true;

        _currentColorCornerSpotlights = DEFAULT;
        _currentColorCenterSpotLight = DEFAULT;
        _shadowMaxIntensity = PlayerPrefs.GetInt(GlobalConstants.PREF_HIGH_DETAIL_ON) != 0 ? SHADOW_MAX_INTENSITY : 0.0f;
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