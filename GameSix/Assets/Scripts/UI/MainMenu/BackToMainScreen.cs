using UnityEngine;
using UnityEngine.UI;

public class BackToMainScreen : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "BackToMainScreen";
    private const string BUTTON_CANCEL = "Cancel";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField] private Button _backButton = null;
    #endregion

    #region MonoBehaviour
    private void Update()
    {
        if (Input.GetButtonDown(BUTTON_CANCEL))
        {
            if (_backButton.interactable)
            {
                _backButton.onClick.Invoke();
            }
        }
    }
    #endregion
}
