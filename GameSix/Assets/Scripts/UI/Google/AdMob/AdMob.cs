using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine;

public class AdMob : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "AdMob";
    private const string AD_UNIT_ID_ANDROID = "ca-app-pub-3296591416050248/4192288073";
    private const string AD_UNIT_ID_IOS = "ca-app-pub-3296591416050248/3206160725";
    private const string TEST_DEVICE_ANDROID_SAMSUNG = "153383FBE5FF1603C9ADAFC4F7E93D8C";
    private const string TEST_DEVICE_ANDROID_XIAOMI = "2B486FDACB747632CC2E54CDC85ED86B";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    // Singleton MonoBehaviour
    private static AdMob _instance;

    private InterstitialAd interstitialAd;
    private Action _actionOnCloseAd;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        // Singleton MonoBehaviour
        if (_instance == null)
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
            AwakeSingletonMonoBehaviour();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void AwakeSingletonMonoBehaviour()
    {
        // Initializes the Google Mobile Ads SDK (must be called in Awake or Start).
        MobileAds.Initialize(HandleInitCompleteAction);
    }
    #endregion

    #region Utils
    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        // Callbacks from GoogleMobileAds are not guaranteed to be called on main thread.
        // We use MobileAdsEventExecutor to schedule these calls on the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            _log.DebugLog(LOG_TAG, "AdMob Initialization complete", gameObject);

            // Loads the first interstitital
            RequestInterstitial();
        });
    }

    private void RequestInterstitial()
    {
        _log.DebugLog(LOG_TAG, "RequestInterstitial starts", gameObject);

        string adUnitId = "";
#if UNITY_ANDROID
        adUnitId = (Application.platform == RuntimePlatform.Android) ? AD_UNIT_ID_ANDROID : "*** GAMESIX: unexpected platform ***";
#endif
#if UNITY_IOS
        adUnitId = (Application.platform == RuntimePlatform.Android) ? AD_UNIT_ID_IOS : "*** GAMESIX: unexpected platform ***";
#endif

        // Cleans up interstitialAd before using it
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }

        // Initializes an InterstitialAd.
        // On iOS, InterstitialAd objects are one time use objects. That means once an interstitial is shown, the InterstitialAd object can't be 
        // used to load another ad. To request another interstitial, a new InterstitialAd object must be created.
        interstitialAd = new InterstitialAd(adUnitId);

        // Called when the ad is closed.
        interstitialAd.OnAdClosed += HandleOnAdClosed;

        // Creates an empty ad request.
        AdRequest request = null;

#if UNITY_ANDROID
        request = new AdRequest.Builder()
                      .AddTestDevice(TEST_DEVICE_ANDROID_XIAOMI)
                      .AddTestDevice(TEST_DEVICE_ANDROID_SAMSUNG)
                      .Build();
#endif
#if UNITY_IOS
        request = new AdRequest.Builder()
                      .Build();
#endif

        // Load the interstitial with the request.
        interstitialAd.LoadAd(request);

        _log.DebugLog(LOG_TAG, "RequestInterstitial ends", gameObject);
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        // Loads next interstitital
        RequestInterstitial();

        // Callback
        if (_actionOnCloseAd != null)
        {
            _log.DebugLog(LOG_TAG, "Executing callback Action", gameObject);
            MobileAdsEventExecutor.ExecuteInUpdate(_actionOnCloseAd);
        }
    }
#endregion

#region API
    // Singleton MonoBehaviour: retrieve instance
    public static AdMob GetInstance()
    {
        return _instance;
    }

    public void ShowInterstitialAd(Action callBackOnCloseAd)
    {
        if (interstitialAd.IsLoaded())
        {
            _log.DebugLog(LOG_TAG, "The interstitial ad is displayed", gameObject);
            _actionOnCloseAd = callBackOnCloseAd;
            interstitialAd.Show();
        }
        else
        {
            _log.DebugLog(LOG_TAG, "The interstitial ad wasn't loaded yet", gameObject);
            callBackOnCloseAd.Invoke();
        }
    }

    public void Dispose()
    {
        interstitialAd.Destroy();
    }
#endregion
}