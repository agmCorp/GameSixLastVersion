using UnityEngine;
using System.Collections;

// Based on CFX_AutoDestructShuriken.cs
// Automatically returns to the pool an object when it has stopped emitting particles and when they have all disappeared from the screen.
// Check is performed every 0.5 seconds to not query the particle system's state every frame.

[RequireComponent(typeof(ParticleSystem))]
public class AutoReturnToPool : MonoBehaviour
{
	#region Private Constants
	private const float CHECK_INTERVAL = 0.5f;
    #endregion

    #region Private Attributes
    private ObjectPooler _objectPooler;
    #endregion

    #region MonoBehaviour
    private void OnEnable()
	{
		_objectPooler = ObjectPooler.GetInstance();
		StartCoroutine(CheckIfAlive());
	}
	#endregion

	#region Utils
	private IEnumerator CheckIfAlive ()
	{
		ParticleSystem ps = this.GetComponent<ParticleSystem>();

		while(true && ps != null)
		{
			yield return new WaitForSeconds(CHECK_INTERVAL);
			if(!ps.IsAlive(true))
			{
				_objectPooler.ReturnToPool(gameObject);
			}
		}
	}
	#endregion Utils
}
