using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlanetController : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "PlanetController";
    private const string SMALL_GAP_KEY = "SMALL_GAP";
    private const float SMALL_GAP_RADIUS = 0.2f;
    private const string MEDIUM_GAP_KEY = "MEDIUM_GAP";
    private const float MEDIUM_GAP_RADIUS = 0.3f;
    private const string BIG_GAP_KEY = "BIG_GAP";
    private const float BIG_GAP_RADIUS = 0.4f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private ObjectPooler _objectPooler;
    private float _radius;
    private Queue<string> _moonKeyQueue;
    private string _moonName = "";
    private int _moonCount;
    private bool _simulateOrbit;
    #endregion

    #region Properties
    public int MoonCount
    {
        get { return _moonCount; }
        set { _moonCount = value; }
    }
    #endregion

    #region MonoBehaviour
    // Reusing GameObject
    private void OnEnable()
    {
        _objectPooler = ObjectPooler.GetInstance();

        // Performance
        _simulateOrbit = false;
    }

    // Regardless of whether the renderer of ANY Moon or the renderer of the Planet became visible by the 
    // camera, the entire orbit is activated. See MoonController.OnBecameVisible().
    private void OnBecameVisible()
    {
        SimulateOrbit();
    }
    #endregion

    #region Utils
    private float GetChord(GameObject moon)
    {
        // There is a mathematical reason for this, trust me.
        return moon.GetComponent<MoonController>().GetRadius() * 2;
    }

    private float GetChord(string gapKey)
    {
        float radius;
        switch (gapKey)
        {
            case BIG_GAP_KEY:
                radius = BIG_GAP_RADIUS;
                break;
            case MEDIUM_GAP_KEY:
                radius = MEDIUM_GAP_RADIUS;
                break;
            case SMALL_GAP_KEY:
                radius = SMALL_GAP_RADIUS;
                break;
            default:
                radius = 0;
                break;
        }

        // There is a mathematical reason for this, trust me.
        return radius * 2;
    }

    private double GetCentralAngleRadians(GameObject moon)
    {
        return 2 * Mathf.Asin(GetChord(moon) / (2 * _radius));
    }

    private double GetCentralAngleRadians(string gapKey)
    {
        return 2 * Mathf.Asin(GetChord(gapKey) / (2 * _radius));
    }

    private GameObject GetMoonFromPool(string moonKey)
    {
        Vector3 pos = new Vector3(transform.position.x + _radius, transform.position.y, transform.position.z);
        GameObject moon = _objectPooler.SpawnFromPool(moonKey, pos, Quaternion.identity);
        moon.GetComponent<MoonController>().PublicInit(gameObject, _moonName + _moonCount);
        _moonCount++;
        return moon;
    }

    private void ReturnMoonToPool(GameObject moon)
    {
        _objectPooler.ReturnToPool(moon);
    }

    private bool IsGap(string moonKey)
    {
        return moonKey.Equals(BIG_GAP_KEY) ||
               moonKey.Equals(MEDIUM_GAP_KEY) ||
               moonKey.Equals(SMALL_GAP_KEY);
    }

    private void CreateOrbit(int fromSortingOrder)
    {
        GameObject moon;
        double actualCentralAngle;
        double previousCentralAngle = 0;
        double rotateAngle = 0;
        double totalAngleRadians = 0;
        bool exit = false;
        string moonKey;

        while (_moonKeyQueue.Count > 0 && !exit)
        {
            moonKey = _moonKeyQueue.Dequeue();
            if (IsGap(moonKey))
            {
                moon = null;
                actualCentralAngle = GetCentralAngleRadians(moonKey);
            }
            else
            {
                moon = GetMoonFromPool(moonKey);
                actualCentralAngle = GetCentralAngleRadians(moon);
            }
            totalAngleRadians += actualCentralAngle;

            if (totalAngleRadians <= 2 * Mathf.PI)
            {
                if (previousCentralAngle > 0)
                {
                    rotateAngle += previousCentralAngle / 2 + actualCentralAngle / 2;
                }
                previousCentralAngle = actualCentralAngle;

                if (moon != null)
                {
                    moon.transform.RotateAround(transform.position, Vector3.back, (float)(Mathf.Rad2Deg * rotateAngle));
                    moon.transform.rotation = Quaternion.identity;
                    moon.GetComponent<SpriteRenderer>().sortingOrder = fromSortingOrder;
                    fromSortingOrder--;
                }
            }
            else
            {
                if (moon != null)
                {
                    ReturnMoonToPool(moon);
                }
                exit = true;
            }
        }
    }
    #endregion

    #region API
    public void PublicInit(float radius, Queue<string> moonKeyQueue, string planetName, string moonName, int fromSortingOrder)
    {
        _radius = radius;
        _moonKeyQueue = moonKeyQueue;
        gameObject.name = planetName;
        _moonName = moonName;
        _moonCount = 0;
        CreateOrbit(fromSortingOrder);
    }

    public void StartOrbiting(float initialSpeed, bool clockwise)
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().Orbit(initialSpeed, clockwise);
        }
    }

    public void StartOrbiting(float initialSpeed, bool clockwise, float maxSpeed, float acceleration, bool bounce)
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().Orbit(initialSpeed, clockwise, maxSpeed, acceleration, bounce);
        }
    }

    public void StartSwinging(float minRadius, float maxRadius, float swingingSpeed)
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().StartSwinging(minRadius, maxRadius, swingingSpeed);
        }
    }

    public void StartSwingingWithPause(float minRadius, float maxRadius, float swingingSpeed, float swingDuration, float pauseDuration)
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().StartSwingingWithPause(minRadius, maxRadius, swingingSpeed, swingDuration, pauseDuration);
        }
    }

    public void StopSwinging()
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().StopSwinging();
        }
    }

    public void SetFlee(float fleeSpeed, float shrinkTime)
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().SetFlee(fleeSpeed, shrinkTime);
        }
    }

    public void StartFlee()
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().StartFlee();
        }
    }

    public void SlowDown()
    {
        MoonController moonController;
        foreach (Transform transformMoon in transform)
        {
            moonController = transformMoon.gameObject.GetComponent<MoonController>();
            moonController.SlowDown();
        }
    }

    public void Dispose()
    {
        while (transform.childCount > 0)
        {
            ReturnMoonToPool(transform.GetChild(0).gameObject);
        }
    }

    public void SleepAnimation()
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().SleepAnimation();
        }
    }

    public void IdleAnimation()
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().IdleAnimation();
        }
    }

    public void PauseAnimations(bool pause)
    {
        foreach (Transform transformMoon in transform)
        {
            transformMoon.gameObject.GetComponent<MoonController>().PauseAnimations(pause);
        }
    }

    public void SimulateOrbit()
    {
        // Performance
        if (!_simulateOrbit)
        {
            _simulateOrbit = true;
            foreach (Transform transformMoon in transform)
            {
                transformMoon.gameObject.GetComponent<MoonController>().Simulate();
            }
        }
    }
    #endregion
}
