using UnityEngine;

public class TransitionAnimationEvent : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "TransitionAnimationEvent";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private SceneLoader _sceneLoader;
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        _sceneLoader = SceneLoader.GetInstance();
    }
    #endregion

    #region API
    public void LoadScene()
    {
        _sceneLoader.LoadScene();
    }

    public void FinalizeTransition()
    {
        _sceneLoader.FinalizeTransition();
    }

    public void OpenCandy()
    {
        _sceneLoader.PlayOpenCandyAudio();
    }

    public void CloseCandy()
    {
        _sceneLoader.PlayCloseCandyAudio();
    }

    public void Gate()
    {
        _sceneLoader.PlayGateAudio();
    }
    #endregion
}
