using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioAssetManager : MonoBehaviour
{
    #region Public Constants
    public const string MASTER_MIXER_MUSIC_EXP_PARAM = "MusicVolume";
    public const string MASTER_MIXER_SFX_EXP_PARAM = "SFXVolume";
    public const string MASTER_MIXER_SFXUI_EXP_PARAM = "SFXUIVolume";
    public const string MASTER_MIXER_CROSSFADE_EXP_PARAM = "CrossfadeVolume";

    // Music
    public const string GAME_MUSIC = "Game";
    public const string GAME_OVER_MUSIC = "GameOver";
    public const string MAIN_MENU_MUSIC = "MainMenu";
    public const string GRAND_FINALE_MUSIC = "GrandFinale";

    // SFX
    public const string FALL_SFX = "Fall";
    public const string YAMMY_SFX = "Yammy";
    public const string YIPEE_SFX = "Yipee";
    public const string YAY_SFX = "Yay";
    public const string YES_SFX = "Yes";
    public const string VICTORY_SFX = "Victory";
    public const string BOING_SFX = "Boing";
    public const string CLOCK_SFX = "Clock";
    public const string CRUNCH1_SFX = "Crunch1";
    public const string CRUNCH2_SFX = "Crunch2";
    public const string CRUNCH3_SFX = "Crunch3";
    public const string CRICKETS_SFX = "Crickets";
    public const string ZIP1_SFX = "Zip1";
    public const string ZIP2_SFX = "Zip2";
    public const string ZIP3_SFX = "Zip3";
    public const string ZIP4_SFX = "Zip4";
    public const string TAKE_ORANGE_SFX = "TakeOrange";
    public const string TAKE_RED_SFX = "TakeRed";
    public const string TAKE_GREEN_SFX = "TakeGreen";
    public const string TAKE_YELLOW_SFX = "TakeYellow";
    public const string TAKE_PINK_SFX = "TakePink";
    public const string ELECTRICITY_SFX = "Electricity";
    public const string MAGIC_SFX = "Tiny";
    public const string BEEP_SFX = "Beep";
    public const string SCARY_SFX = "Scary";
    public const string OH_OH_SFX = "OhOh";
    public const string FIREWORK_01_SFX = "Firework_01";
    public const string FIREWORK_02_SFX = "Firework_02";
    public const string FIREWORK_03_SFX = "Firework_03";
    public const string FIREWORK_04_SFX = "Firework_04";
    public const string FIREWORK_05_SFX = "Firework_05";
    public const string FIREWORK_06_SFX = "Firework_06";
    public const string FIREWORK_07_SFX = "Firework_07";
    public const string FIREWORK_08_SFX = "Firework_08";
    public const string FIREWORK_09_SFX = "Firework_09";
    public const string FIREWORK_10_SFX = "Firework_10";
    public const string FIREWORK_11_SFX = "Firework_11";
    public const string FIREWORK_12_SFX = "Firework_12";
    public const string FIREWORK_13_SFX = "Firework_13";
    public const string FIREWORK_14_SFX = "Firework_14";
    public const string FIREWORK_15_SFX = "Firework_15";

    // SFXUI
    public const string CLICK_SFXUI = "Click";
    public const string CLOSE_CANDY_SFXUI = "CloseCandy";
    public const string GATE_SFXUI = "Gate";
    public const string OPEN_CANDY_SFXUI = "OpenCandy";
    public const string PANEL_IN_SFXUI = "PanelIn";
    public const string PANEL_OUT_SFXUI = "PanelOut";
    public const string HIT_SFXUI = "Hit";
    public const string RELOAD_SFXUI = "Reload";
    public const string TAP_SFXUI = "Tap";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "AudioAssetManager";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    // Singleton MonoBehaviour
    private static AudioAssetManager _instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _masterMixer = null;
    [SerializeField] private AudioMixerGroup _crossfadeGroup = null;
    [SerializeField] private AudioMixerGroup _musicGroup = null;
    [SerializeField] private AudioMixerGroup _sFXGroup = null;
    [SerializeField] private AudioMixerGroup _sFXUIGroup = null;

    private Dictionary<string, AudioClip> _musicClipDictionary;
    private Dictionary<string, AudioClip> _sFXClipDictionary;
    private Dictionary<string, AudioClip> _sFXUIClipDictionary;

    [Header("Audio Clips Music")]
    [SerializeField] private AudioClip _game = null;
    [SerializeField] private AudioClip _gameOver = null;
    [SerializeField] private AudioClip _mainMenu = null;
    [SerializeField] private AudioClip _grandFinale = null;

    [Header("Audio Clips SFX")]
    [SerializeField] private AudioClip _fall = null;
    [SerializeField] private AudioClip _yammy = null;
    [SerializeField] private AudioClip _yipee = null;
    [SerializeField] private AudioClip _yay = null;
    [SerializeField] private AudioClip _yes = null;
    [SerializeField] private AudioClip _victory = null;
    [SerializeField] private AudioClip _boing = null;
    [SerializeField] private AudioClip _clock = null;
    [SerializeField] private AudioClip _crunch1 = null;
    [SerializeField] private AudioClip _crunch2 = null;
    [SerializeField] private AudioClip _crunch3 = null;
    [SerializeField] private AudioClip _crickets = null;
    [SerializeField] private AudioClip _zip1 = null;
    [SerializeField] private AudioClip _zip2 = null;
    [SerializeField] private AudioClip _zip3 = null;
    [SerializeField] private AudioClip _zip4 = null;
    [SerializeField] private AudioClip _takeOrange = null;
    [SerializeField] private AudioClip _takeRed = null;
    [SerializeField] private AudioClip _takeGreen = null;
    [SerializeField] private AudioClip _takeYellow = null;
    [SerializeField] private AudioClip _takePink = null;
    [SerializeField] private AudioClip _electricity = null;
    [SerializeField] private AudioClip _magic = null;
    [SerializeField] private AudioClip _beep = null;
    [SerializeField] private AudioClip _scary = null;
    [SerializeField] private AudioClip _ohOh = null;
    [SerializeField] private AudioClip _firework_01 = null;
    [SerializeField] private AudioClip _firework_02 = null;
    [SerializeField] private AudioClip _firework_03 = null;
    [SerializeField] private AudioClip _firework_04 = null;
    [SerializeField] private AudioClip _firework_05 = null;
    [SerializeField] private AudioClip _firework_06 = null;
    [SerializeField] private AudioClip _firework_07 = null;
    [SerializeField] private AudioClip _firework_08 = null;
    [SerializeField] private AudioClip _firework_09 = null;
    [SerializeField] private AudioClip _firework_10 = null;
    [SerializeField] private AudioClip _firework_11 = null;
    [SerializeField] private AudioClip _firework_12 = null;
    [SerializeField] private AudioClip _firework_13 = null;
    [SerializeField] private AudioClip _firework_14 = null;
    [SerializeField] private AudioClip _firework_15 = null;

    [Header("Audio Clips SFXUI")]
    [SerializeField] private AudioClip _click = null;
    [SerializeField] private AudioClip _closeCandy = null;
    [SerializeField] private AudioClip _gate = null;
    [SerializeField] private AudioClip _openCandy = null;
    [SerializeField] private AudioClip _panelIn = null;
    [SerializeField] private AudioClip _panelOut = null;
    [SerializeField] private AudioClip _hit = null;
    [SerializeField] private AudioClip _reload = null;
    [SerializeField] private AudioClip _tap = null;
    #endregion

    #region Properties
    public AudioMixer MasterMixer
    {
        get { return _masterMixer; }
    }
    public AudioMixerGroup CrossfadeGroup
    {
        get { return _crossfadeGroup; }
    }
    public AudioMixerGroup MusicGroup
    {
        get { return _musicGroup; }
    }
    public AudioMixerGroup SFXGroup
    {
        get { return _sFXGroup; }
    }
    public AudioMixerGroup SFXUIGroup
    {
        get { return _sFXUIGroup; }
    }
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
        _musicClipDictionary = new Dictionary<string, AudioClip>
        {
            { GAME_MUSIC, _game },
            { GAME_OVER_MUSIC, _gameOver },
            { MAIN_MENU_MUSIC, _mainMenu },
            { GRAND_FINALE_MUSIC, _grandFinale }
        };

        _sFXClipDictionary = new Dictionary<string, AudioClip>
        {
            { FALL_SFX, _fall },
            { YAMMY_SFX, _yammy },      
            { YIPEE_SFX, _yipee },
            { YAY_SFX, _yay },
            { YES_SFX, _yes },
            { VICTORY_SFX, _victory },
            { BOING_SFX, _boing },
            { CLOCK_SFX, _clock },
            { CRUNCH1_SFX, _crunch1 },
            { CRUNCH2_SFX, _crunch2 },
            { CRUNCH3_SFX, _crunch3 },
            { CRICKETS_SFX, _crickets },
            { ZIP1_SFX, _zip1},
            { ZIP2_SFX, _zip2 },
            { ZIP3_SFX, _zip3 },
            { ZIP4_SFX, _zip4 },
            { TAKE_ORANGE_SFX, _takeOrange },
            { TAKE_RED_SFX, _takeRed },
            { TAKE_GREEN_SFX, _takeGreen },
            { TAKE_YELLOW_SFX, _takeYellow },
            { TAKE_PINK_SFX, _takePink },
            { ELECTRICITY_SFX, _electricity },
            { MAGIC_SFX, _magic },
            { BEEP_SFX, _beep },
            { SCARY_SFX, _scary },
            { OH_OH_SFX, _ohOh },
            { FIREWORK_01_SFX, _firework_01 },
            { FIREWORK_02_SFX, _firework_02 },
            { FIREWORK_03_SFX, _firework_03 },
            { FIREWORK_04_SFX, _firework_04 },
            { FIREWORK_05_SFX, _firework_05 },
            { FIREWORK_06_SFX, _firework_06 },
            { FIREWORK_07_SFX, _firework_07 },
            { FIREWORK_08_SFX, _firework_08 },
            { FIREWORK_09_SFX, _firework_09 },
            { FIREWORK_10_SFX, _firework_10 },
            { FIREWORK_11_SFX, _firework_11 },
            { FIREWORK_12_SFX, _firework_12 },
            { FIREWORK_13_SFX, _firework_13 },
            { FIREWORK_14_SFX, _firework_14 },
            { FIREWORK_15_SFX, _firework_15 }
        };

        _sFXUIClipDictionary = new Dictionary<string, AudioClip>
        {
            { CLICK_SFXUI, _click },
            { CLOSE_CANDY_SFXUI, _closeCandy },
            { GATE_SFXUI, _gate },
            { OPEN_CANDY_SFXUI, _openCandy },
            { PANEL_IN_SFXUI, _panelIn },
            { PANEL_OUT_SFXUI, _panelOut },
            { HIT_SFXUI, _hit },
            { RELOAD_SFXUI, _reload },
            { TAP_SFXUI, _tap }
        };
    }
    #endregion

    #region API
    // Singleton MonoBehaviour: retrieve instance
    public static AudioAssetManager GetInstance()
    {
        return _instance;
    }

    public AudioClip GetClip(string key)
    {
        AudioClip audioClip;
        if (_musicClipDictionary.ContainsKey(key))
        {
            audioClip = _musicClipDictionary[key];
        }
        else
        {
            if (_sFXClipDictionary.ContainsKey(key))
            {
                audioClip = _sFXClipDictionary[key];
            }
            else
            {
                audioClip = _sFXUIClipDictionary[key];
            }
        }

        return audioClip;
    }

    public AudioClip GetRandomSFXClip()
    {
        return _sFXClipDictionary.Values.ElementAt(Random.Range(0, _sFXClipDictionary.Count));
    }
    #endregion
}
