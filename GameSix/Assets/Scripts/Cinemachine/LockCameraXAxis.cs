using Cinemachine;
using UnityEngine;

[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
public class LockCameraXAxis : CinemachineExtension
{
    #region Private Constants
    private const string LOG_TAG = "LockCameraXAxis";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    #endregion

    /// An add-on module for Cinemachine Virtual Camera that locks the camera's X axis component
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
                                                      CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (enabled && stage == CinemachineCore.Stage.Body)
        {
            Vector3 pos = state.RawPosition;
            pos.x = 0.0f;
            state.RawPosition = pos;
        }
    }
}