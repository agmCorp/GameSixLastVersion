using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NumberRenderer : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "NumberRenderer";
    private const float SEPARATOR_PIXEL_WIDTH = 8.0f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField] private List<Sprite> _spriteNumbers = null;
    private ObjectPooler _objectPooler;
    #endregion

    #region MonoBehaviour
    private void OnEnable()
    {
        _objectPooler = ObjectPooler.GetInstance();
    }
    #endregion

    #region Utils
    private GameObject GetNumberFromPool(Vector3 position)
    {
        return _objectPooler.SpawnFromPool(ObjectPooler.NUMBER_KEY, position, Quaternion.identity);
    }

    private void ReturnNumberToPool(GameObject number)
    {
        _objectPooler.ReturnToPool(number);
    }
    #endregion

    #region API
    public void RenderNumber(int num)
    {
        char[] numberArray = num.ToString().ToArray();
        GameObject number;
        Sprite sprite;
        float spriteWidth;
        float totalSizeX = 0;
        float pixelsPerUnit = _spriteNumbers[0].pixelsPerUnit;
        float separator = SEPARATOR_PIXEL_WIDTH / pixelsPerUnit;

        for (int i = 0; i < numberArray.Length; i++)
        {
            number = GetNumberFromPool(transform.position);
            number.transform.parent = transform;
            number.name = numberArray[i].ToString();

            sprite = _spriteNumbers[int.Parse(number.name)];
            spriteWidth = sprite.rect.width / pixelsPerUnit;

            number.transform.localPosition = new Vector2(totalSizeX + (spriteWidth / 2), 0);
            totalSizeX += spriteWidth + separator;

            number.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        totalSizeX -= separator;
        transform.localPosition = new Vector2(-totalSizeX / 2, 0);
    }

    public void Dispose()
    {
        while (transform.childCount > 0)
        {
            ReturnNumberToPool(transform.GetChild(0).gameObject);
        }
    }
    #endregion
}
