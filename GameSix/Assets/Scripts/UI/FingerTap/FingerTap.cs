using UnityEngine;

public class FingerTap : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "FingerTap";
    private const int MAX_TAPS = 3;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private int _taps;
    private Hud _hud;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _hud = GameObject.FindGameObjectWithTag(Hud.TAG_HUD).GetComponent<Hud>();
    }

    private void OnEnable()
    {
        _taps = 0;
    }
    #endregion

    #region API
    public void TapCounter()
    {
        _taps++;
        _log.DebugLog(LOG_TAG, "FingerTap.Tap() _taps: " + _taps, gameObject);

        if (_taps >= MAX_TAPS)
        {
            gameObject.SetActive(false);
        }
    }

    public void TapAudio()
    {
        _hud.TapAudio();
    }
    #endregion
}
