using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using UnityEngine;

public class GPGS
{
    #region Private Constants
    private const string LOG_TAG = "GPGS";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    // Singleton: unique instance
    private static GPGS _instance;
    #endregion

    #region Utils
    // Singleton: prevent instantiation from other classes
    private GPGS()
    {
#if UNITY_ANDROID
        _log.DebugLog(LOG_TAG, "Activation starts");

        // Debugging:
        PlayGamesPlatform.DebugLogEnabled = GlobalConstants.ENABLE_DEBUG_LOG;

        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();

        _log.DebugLog(LOG_TAG, "Activation ends");
#else
        _log.DebugLog(LOG_TAG, "Platform not supported");
#endif
    }
    #endregion

    #region API
    // Singleton: retrieve instance
    public static GPGS GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GPGS();
        }
        return _instance;
    }

    public void AuthenticateOnStart(Action<bool> callback = null)
    {
#if UNITY_ANDROID
        _log.DebugLog(LOG_TAG, "AuthenticateOnStart starts");

        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>
        {
            // handle results
            _log.DebugLog(LOG_TAG, "Authentication result: " + result);
            callback?.Invoke(result == SignInStatus.Success);
        });

        _log.DebugLog(LOG_TAG, "AuthenticateOnStart ends");
#else
        _log.DebugLog(LOG_TAG, "Platform not supported");
#endif
    }

    public void AuthenticateByButton(Action<bool> callback = null)
    {
#if UNITY_ANDROID
        _log.DebugLog(LOG_TAG, "AuthenticateByButton starts");

        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (result) =>
        {
            // handle results
            _log.DebugLog(LOG_TAG, "Authentication result: " + result);
            callback?.Invoke(result == SignInStatus.Success);
        });

        _log.DebugLog(LOG_TAG, "AuthenticateByButton ends");
#else
        _log.DebugLog(LOG_TAG, "Platform not supported");
#endif
    }

    public bool IsAuthenticated()
    {
#if UNITY_ANDROID
        bool isAuthenticated = Social.localUser.authenticated;
        _log.DebugLog(LOG_TAG, "IsAuthenticated: " + isAuthenticated);
        return isAuthenticated;
#else
        _log.DebugLog(LOG_TAG, "Platform not supported");
        return false;
#endif
    }

    public void ShowLeaderboard()
    {
#if UNITY_ANDROID
        _log.DebugLog(LOG_TAG, "ShowLeaderboard starts");

        PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_leaderboard);

        _log.DebugLog(LOG_TAG, "ShowLeaderboard ends");
#else
        _log.DebugLog(LOG_TAG, "Platform not supported");
#endif
    }

    public void PostHighLevel(int highLevel)
    {
#if UNITY_ANDROID
        Social.ReportScore(highLevel, GPGSIds.leaderboard_leaderboard, (bool success) =>
        {
            _log.DebugLog(LOG_TAG, "PostHighLevel highLevel: " + highLevel + ", success: " + success);
        });
#else
        _log.DebugLog(LOG_TAG, "Platform not supported");
#endif
    }

    public void SignOut()
    {
#if UNITY_ANDROID
        _log.DebugLog(LOG_TAG, "SignOut starts");

        PlayGamesPlatform.Instance.SignOut();

        _log.DebugLog(LOG_TAG, "SignOut ends");
#else
        _log.DebugLog(LOG_TAG, "Platform not supported");
#endif
    }
    #endregion
}