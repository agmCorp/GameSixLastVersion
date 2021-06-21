using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PageSwiper : MonoBehaviour, IDragHandler, IEndDragHandler
{
    #region Private Constants
    private const string LOG_TAG = "PageSwiper";
    private const float PERCENT_THRESHOLD = 0.2f;
    private const float EASING = 0.5f;
    private const float DELAY = 1.0f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private Vector3 _panelLocation;
    private int _totalPages;
    private MainMenu _mainMenu;
    private int _currentPage;
    private bool _enableDrag;
    private bool _moving;
    #endregion

    #region Properties
    public bool IsMoving
    {
        get { return _moving; }
    }

    public bool EnableDrag
    {
        set { _enableDrag = value; }
    }

    public Vector3 PanelLocation
    {
        set { _panelLocation = value; }
    }

    public int TotalPages
    {
        set { _totalPages = value; }
    }

    public MainMenu MainMenuReference
    {
        set { _mainMenu = value; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _currentPage = 1;
    }

    public void OnDrag(PointerEventData data)
    {
        if (_enableDrag)
        {
            float difference = data.pressPosition.x - data.position.x;
            transform.position = _panelLocation - new Vector3(difference, 0, 0);
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (_enableDrag)
        {
            float percentage = (data.pressPosition.x - data.position.x) / Screen.width;
            if (Mathf.Abs(percentage) >= PERCENT_THRESHOLD)
            {
                Vector3 newLocation = _panelLocation;
                if (percentage > 0 && _currentPage < _totalPages)
                {
                    _currentPage++;
                    newLocation += new Vector3(-Screen.width, 0, 0);
                }
                else if (percentage < 0 && _currentPage > 1)
                {
                    _currentPage--;
                    newLocation += new Vector3(Screen.width, 0, 0);
                }
                StartCoroutine(SmoothMove(transform.position, newLocation, EASING));
                _panelLocation = newLocation;
            }
            else
            {
                StartCoroutine(SmoothMove(transform.position, _panelLocation, EASING));
            }
        }
    }
    #endregion

    #region Utils
    IEnumerator SmoothMove(Vector3 startpos, Vector3 endpos, float seconds)
    {
        _moving = true;

        _mainMenu.PlaySlidePanelSound();

        float timeSinceStarted = 0f;
        while (timeSinceStarted <= DELAY)
        {
            timeSinceStarted += Time.deltaTime / seconds;
            transform.position = Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0f, DELAY, timeSinceStarted));
            yield return null;
        }

        _moving = false;
    }
    #endregion

    #region API
    public void GoToPage(int page)
    {
        if (_currentPage < page)
        {
            _panelLocation += new Vector3(-Screen.width * (page - _currentPage), 0, 0);
        }
        else if (_currentPage > page)
        {
            _panelLocation += new Vector3(Screen.width * (_currentPage - page), 0, 0);
        }
        _currentPage = page;
        transform.position = _panelLocation;
    }
    #endregion
}