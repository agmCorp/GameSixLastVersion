using UnityEngine;

public class LightManagerFactory : MonoBehaviour
{
    #region MonoBehaviour
    void Awake()
    {
        GameObject spotlight = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_SPOTLIGHT);
        GameObject spriteLight = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_SPRITE_LIGHT);

        if (PlayerPrefs.GetInt(GlobalConstants.PREF_LOW_DETAIL_ON) != 0)
        {
            spotlight.SetActive(false);
            spriteLight.SetActive(true);
            gameObject.AddComponent<LowDetailLightManager>();
        }
        else
        {
            spotlight.SetActive(true);
            spriteLight.SetActive(false);
            gameObject.AddComponent<HighDetailLightManager>();
        }
    }
    #endregion
}
