using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "LevelSelector";
    private const int SPACING_X = 20;
    private const int SPACING_Y = 20;
    private const int PADDING_LEFT = 60;
    private const int PADDING_RIGHT = 60;
    private const int PADDING_TOP = 45;
    private const int PADDING_BOTTOM = 45;
    private const string PANEL_NAME = "Page";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField] private GameObject _levelHolder = null;
    [SerializeField] private GameObject _prefabLevelIcon = null;
    [SerializeField] private GameObject _canvas = null;

    private MainMenu _mainMenu;
    private int _numberOfLevels;
    private Rect _panelDimensions;
    private Rect _levelIconDimension;
    private int _amountPerPage;
    private int _currentLevelCount;
    #endregion

    #region Properties
    public int AmountPerPage
    {
        get { return _amountPerPage; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _mainMenu = GetComponent<MainMenu>();
    }
    #endregion

    #region Utils
    private void LoadPanels(int numberOfPanels)
    {
        GameObject panelClone = Instantiate(_levelHolder) as GameObject;

        PageSwiper swiper = _levelHolder.AddComponent<PageSwiper>();
        swiper.TotalPages = numberOfPanels;
        swiper.MainMenuReference = _mainMenu;

        for (int i = 1; i <= numberOfPanels; i++)
        {
            GameObject panel = Instantiate(panelClone) as GameObject;
            panel.transform.SetParent(_canvas.transform, false);
            panel.transform.SetParent(_levelHolder.transform);
            panel.name = PANEL_NAME + GlobalConstants.NAME_SEPARATOR + i;
            panel.GetComponent<RectTransform>().localPosition = new Vector2(_panelDimensions.width * (i - 1), 0);
            SetUpGrid(panel);
            bool lastPanel = i == numberOfPanels;
            int numberOfLevelIcons = lastPanel ? _numberOfLevels - _currentLevelCount : _amountPerPage;
            LoadLevelIcons(numberOfLevelIcons, panel, lastPanel);
        }
        Destroy(panelClone);
    }

    private void SetUpGrid(GameObject panel)
    {
        GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(_levelIconDimension.width, _levelIconDimension.height);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.spacing = new Vector2(SPACING_X, SPACING_Y);
        grid.padding = new RectOffset(PADDING_LEFT, PADDING_RIGHT, PADDING_TOP, PADDING_BOTTOM);
    }

    private void LoadLevelIcons(int numberOfLevelIcons, GameObject parentObject, bool lastPanel)
    {
        for (int i = 1; i <= numberOfLevelIcons; i++)
        {
            _currentLevelCount++;
            GameObject levelIcon = Instantiate(_prefabLevelIcon) as GameObject;
            levelIcon.transform.SetParent(_canvas.transform, false);
            levelIcon.transform.SetParent(parentObject.transform);
            levelIcon.GetComponent<LevelIcon>().PublicInit(_currentLevelCount, lastPanel && i == numberOfLevelIcons);

            // Must use a helper variable. Thanks Unity...
            int _currentLevel = _currentLevelCount;
            Button levelIconButton = levelIcon.GetComponent<Button>();
            levelIconButton.onClick.AddListener(() => _mainMenu.Play(_currentLevel));
            levelIconButton.onClick.AddListener(() => _mainMenu.BackLevels());
        }
    }
    #endregion

    #region API
    public void Build(int numberOfLevels)
    {
        _numberOfLevels = numberOfLevels;
        _panelDimensions = _levelHolder.GetComponent<RectTransform>().rect;
        _levelIconDimension = _prefabLevelIcon.GetComponent<RectTransform>().rect;
        int maxInARow = Mathf.FloorToInt((_panelDimensions.width + SPACING_X - (PADDING_LEFT + PADDING_RIGHT)) /
                                         (_levelIconDimension.width + SPACING_X));
        int maxInACol = Mathf.FloorToInt((_panelDimensions.height + SPACING_Y - (PADDING_TOP + PADDING_BOTTOM)) /
                                         (_levelIconDimension.height + SPACING_Y));
        _amountPerPage = maxInARow * maxInACol;
        int totalPages = Mathf.CeilToInt((float)_numberOfLevels / _amountPerPage);

        _log.DebugLog(LOG_TAG, "Build numberOfLevels: " + numberOfLevels +
                               ", _panelDimensions.width: " + _panelDimensions.width +
                               ", _panelDimensions.height: " + _panelDimensions.height +
                               ", _levelIconDimension.width: " + _levelIconDimension.width +
                               ", _levelIconDimension.height: " + _levelIconDimension.height +
                               ", maxInARow: " + maxInARow +
                               ", maxInACol: " + maxInACol +
                               ", _amountPerPage: " + _amountPerPage +
                               ", totalPages: " + totalPages,
                               gameObject);

        LoadPanels(totalPages);
    }
    #endregion
}