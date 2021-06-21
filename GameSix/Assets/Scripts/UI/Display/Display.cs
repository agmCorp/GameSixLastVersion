using UnityEngine;

public class Display : MonoBehaviour
{
    /*
     * http://wiki.unity3d.com/index.php?title=FramesPerSecond
     */
    #region Private Constants
    private const string LOG_TAG = "Display";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private float deltaTime = 0.0f;
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        gameObject.SetActive(GlobalConstants.DISPLAY);
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int fontSize = Screen.height * 2 / (int)GlobalConstants.PPM;

        Rect rect = new Rect(0, Screen.height - fontSize, Screen.width, fontSize);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = fontSize;
        style.normal.textColor = Color.white;

        string debug = GlobalConstants.TURN_ON_DEBUG ? "Debug ON" : "Debug OFF";
        string log = _log.EnableDebugLog ? "Log ON" : "Log OFF";
        float fps = 1.0f / deltaTime;
        float msec = deltaTime * 1000.0f; // Milliseconds
        string text = string.Format("{0} - {1} - {2:0.} FPS - {3:0.0} ms", debug, log, fps, msec);

        GUI.Label(rect, text, style);
    }
    #endregion
}