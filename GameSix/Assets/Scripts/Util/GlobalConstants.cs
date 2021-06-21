public class GlobalConstants
{
    #region Public Constants
    // App
    public const int TARGET_APP_WIDTH_PIXELS = 480;
    public const int TARGET_APP_HEIGHT_PIXELS = 800;
    public const int TARGET_APP_PADDING_PIXELS = 150;
    public const float PPM = 100;

    // Lights
    public const string TAG_LIGHTS = "Lights";
    public const string TAG_GLOBAL_LIGHT = "GlobalLight";
    public const string TAG_PARALLAX_BACKGROUND = "ParallaxBackground";
    public const string TAG_SPOTLIGHT = "Spotlight";
    public const string TAG_SPRITE_LIGHT = "SpriteLight";

    // Preferences
    public const string PREF_HIGH_LEVEL = "HighLevel";
    public const string PREF_FROM_LEVEL = "FromLevel";
    public const string PREF_SCORE = "Score";
    public const string PREF_ATTEMPTS = "Attempts";
    public const string PREF_MUSIC_ON = "Music";
    public const string PREF_SFX_ON = "SFX";
    public const string PREF_SLIDER_MUSIC_VOLUME = "SliderMusicVolume";
    public const string PREF_SLIDER_SFX_VOLUME = "SliderSFXVolume";
    public const string PREF_LOW_DETAIL_ON = "LowDetailLights";
    public const string PREF_MEDIUM_DETAIL_ON = "MediumDetailLights";
    public const string PREF_HIGH_DETAIL_ON = "HighDetailLights";

    // URLs
    public const string URL_FLUBBIE = "https://play.google.com/store/apps/details?id=uy.com.agm.gamesix";
    public const string URL_GAMES = "https://play.google.com/store/apps/developer?id=Alvaro+G.+Morales";

    // General
    public const char NAME_SEPARATOR = '_';
    public const string TAG_VIRTUAL_CAMERA_ONE = "VCamOne";
    public const SceneLoader.Transition CURRENT_TRANSITION = SceneLoader.Transition.FLUBBIE_WIPE;
    public const int CLICKS_CHEAT_TOGGLE = 10;
    #endregion

    #region Debug Public Constants
    // Master variable: turns debug mode on or off
    public const bool TURN_ON_DEBUG = false;

    // They were declared as "static readonly" to avoid warning "CS0162 - Unreachable code detected" when using them.
    public static readonly bool DISPLAY = true && TURN_ON_DEBUG;                    // Tap on "Friends" image (MainMenu) CLICKS_CHEAT_TOGGLE times to toggle this value
    public static readonly bool ENABLE_DEBUG_LOG = true && TURN_ON_DEBUG;           // Tap on "Flubbie" title (MainMenu) CLICKS_CHEAT_TOGGLE times to toggle this value
    public static readonly bool ENABLE_STORE_CANDY_BUTTONS = true && TURN_ON_DEBUG; // Tap on "Shop" title (Hud) CLICKS_CHEAT_TOGGLE times to toggle this value
    public static readonly bool ENABLE_CHEAT_MODE = true && TURN_ON_DEBUG;
    public static readonly bool ENABLE_POWERLESS = false && TURN_ON_DEBUG;
    public static readonly bool ENABLE_CAM_FOLLOW_DEBUG = false && TURN_ON_DEBUG;
    #endregion

    #region Private Constants
    private const string LOG_TAG = "GlobalConstants";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion
}
