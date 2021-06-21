using UnityEngine;

public class Logging
{
    #region Private Constants
    private const string LOG_TAG = "****GAMESIX****";
    #endregion

    #region Private Attributes
    // Singleton: unique instance
    private static Logging _instance;
    private bool _enableDebugLog;
    #endregion

    #region Properties
    public bool EnableDebugLog
    {
        get { return _enableDebugLog; }
        set { _enableDebugLog = value; }
    }
    #endregion

    #region Utils
    // Singleton: prevent instantiation from other classes
    private Logging()
    {
        _enableDebugLog = GlobalConstants.ENABLE_DEBUG_LOG;
    }

    private string GetDebugTag(string tag)
    {
        return LOG_TAG + " " + tag + ": ";
    }
    #endregion

    #region API
    // Singleton: retrieve instance
    public static Logging GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Logging();
        }
        return _instance;
    }

    public void DebugLog(string tag, string text, GameObject obj)
    {
        if (_enableDebugLog)
        {
            Debug.Log(GetDebugTag(tag) + text, obj);
        }
    }

    public void DebugLogError(string tag, string text, GameObject obj)
    {
        if (_enableDebugLog)
        {
            Debug.LogError(GetDebugTag(tag) + text, obj);
        }
    }

    public void DebugLog(string tag, string text)
    {
        if (_enableDebugLog)
        {
            Debug.Log(GetDebugTag(tag) + text);
        }
    }

    public void DebugLogError(string tag, string text)
    {
        if (_enableDebugLog)
        {
            Debug.LogError(GetDebugTag(tag) + text);
        }
    }

    public void DebugBreak()
    {
        if (_enableDebugLog)
        {
            Debug.Break();
        }
    }
    #endregion
}