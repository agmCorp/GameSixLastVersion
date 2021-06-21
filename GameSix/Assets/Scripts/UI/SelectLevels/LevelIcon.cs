using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelIcon : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "LevelIcon";
    private const string LEVEL_ICON_NAME = "Level";
    private readonly Color32 GRAY = new Color32(200, 200, 200, 255);
    private const string LABEL_GOAL = "GOAL";
    private const int FONT_SIZE_GOAL = 15;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private Button _button;
    [SerializeField] private GameObject _padlock = null;
    [SerializeField] private TextMeshProUGUI _number = null;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    #endregion

    #region API
    public void PublicInit(int level, bool lastLevel)
    {
        name = LEVEL_ICON_NAME + GlobalConstants.NAME_SEPARATOR + level;

        bool levelEnable = level <= PlayerPrefs.GetInt(GlobalConstants.PREF_HIGH_LEVEL);
        _button.interactable = levelEnable;
        _padlock.SetActive(!levelEnable);

        if (lastLevel)
        {
            _number.SetText(LABEL_GOAL);
            _number.fontSize = FONT_SIZE_GOAL;
        }
        else
        {
            _number.SetText(level.ToString());
        }

        if (!levelEnable)
        {
            _number.color = GRAY;
        }
    }
    #endregion
}
