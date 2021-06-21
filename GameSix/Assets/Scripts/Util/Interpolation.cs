using UnityEngine;

public class Interpolation
{
    #region Private Constants
    private const string LOG_TAG = "Interpolation";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    // Singleton: unique instance
    private static Interpolation _instance;
    #endregion

    #region Utils
    // Singleton: prevent instantiation from other classes
    private Interpolation()
    {
    }

    private float BackInImp(float from, float to, float time)
    {
        const float s = 1.70158f;

        to -= from;
        return to * time * time * ((s + 1f) * time - s) + from;
    }

    private float BounceOutImp(float from, float to, float time)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        to -= from;
        if (time < (1f / d1))
            return to * (n1 * time * time) + from;
        if (time < (2f / d1))
            return to * (n1 * (time -= (1.5f / d1)) * time + .75f) + from;
        if (time < (2.5f / d1))
            return to * (n1 * (time -= (2.25f / d1)) * time + .9375f) + from;
        return to * (n1 * (time -= (2.625f / d1)) * time + .984375f) + from;
    }
    #endregion

    #region API
    // Singleton: retrieve instance
    public static Interpolation GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Interpolation();
        }
        return _instance;
    }

    public float BackIn(float from, float to, float time)
    {
        return BackInImp(from, to, time);
    }

    public float BounceOut(float from, float to, float time)
    {
        return Mathf.Clamp(BounceOutImp(from, to, time), Mathf.Min(from, to), Mathf.Max(from, to));
    }
    #endregion
}