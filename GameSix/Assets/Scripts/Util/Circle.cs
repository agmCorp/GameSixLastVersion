using UnityEngine;

public class Circle
{
    #region Private Constants
    private const string LOG_TAG = "Circle";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private Vector2 _center;
    private float _radius;
    #endregion

    #region Properties
    public Vector2 Center
    {
        get { return _center; }
        set { _center = value; }
    }

    public float Radius
    {
        get { return _radius; }
        set { _radius = value; }
    }
    #endregion

    #region API
    public Circle(Vector2 center, float radius)
    {
        this._center = center;
        this._radius = radius;
    }

    // Returns true if this circle overlaps the other circle.
    public bool Overlaps(Circle c)
    {
        float dx = _center.x - c.Center.x;
        float dy = _center.y - c.Center.y;
        float distance = dx * dx + dy * dy;
        float radiusSum = _radius + c.Radius;
        return distance < radiusSum * radiusSum;
    }

    // Return true if this circle contains this point; false otherwise.
    public bool Contains(Vector2 point)
    {
        float dx = _center.x - point.x;
        float dy = _center.y - point.y;
        return dx * dx + dy * dy <= _radius * _radius;
    }
    #endregion
}

