
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStore : MonoBehaviour
{
    #region Public Constants
    public const int MIN_PRICE = 15;
    #endregion

    #region Private Attributes
    [SerializeField] private int _price = 0;
    [SerializeField] private int _amount = 0;
    [SerializeField] private TextMeshProUGUI _label = null;
    [SerializeField] private Sprite _buyEnabled = null;
    [SerializeField] private Sprite _buyDisabled = null;

    private Hud _hud;
    private Button _button = null;
    private Image _image = null;
    private GameManager _gameManager;
    #endregion

    #region Properties
    public int Price
    {
        get { return _price; }
    }
    public int Amount
    {
        get { return _amount; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _hud = GameObject.FindGameObjectWithTag(Hud.TAG_HUD).GetComponent<Hud>();
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _gameManager = GameObject.FindGameObjectWithTag(GameManager.TAG_GAME_MANAGER).GetComponent<GameManager>();
    }

    private void Start()
    {
        _label.text = _price.ToString();
    }

    private void Update()
    {
        if (_hud.Score < _price || _gameManager.IsGameOver())
        {
            _button.interactable = false;
            _image.sprite = _buyDisabled;
        } else
        {
            _button.interactable = true;
            _image.sprite = _buyEnabled;
        }
    }
    #endregion
}
