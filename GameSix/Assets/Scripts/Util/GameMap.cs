using System;
using System.Linq;
using UnityEngine;

public class GameMap
{
    #region Public Json structs (GC friendly)
    /* 
     * In C#, classes are always allocated on the heap. 
     * Structs are allocated on the stack, if a local function variable, or on the heap as 
     * part of a class if a class member.
     */
#pragma warning disable 0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [Serializable]
    public struct JsonLevelConfig
    {
        public int levelDebugId;
        public JsonPolar polarCoords;
        public string[] challengeFilenameList;
    }

    [Serializable]
    public struct JsonPolar
    {
        public float rho;
        public float phi;
    }
#pragma warning restore 0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    #endregion

    #region Private Json structs (GC friendly)
    /* 
     * In C#, classes are always allocated on the heap. 
     * Structs are allocated on the stack, if a local function variable, or on the heap as 
     * part of a class if a class member.
     */
#pragma warning disable 0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [Serializable]
    private struct JsonMapConfig
    {
        public bool loaded;
        public JsonLevelConfig[] levelConfigList;
    }
#pragma warning restore 0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    #endregion

    #region Private Constants
    private const string LOG_TAG = "GameMap";
    private const string MAP_FILENAME = "MapConfig";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    // Singleton: unique instance
    private static GameMap _instance;

    private JsonMapConfig _jsonMapConfig;
    #endregion

    #region Utils
    // Singleton: prevent instantiation from other classes
    private GameMap()
    {
    }

    private void LoadJsonMapConfigFromFile(string mapName)
    {
        _jsonMapConfig = new JsonMapConfig();
        TextAsset txt = (TextAsset)Resources.Load(mapName, typeof(TextAsset));
        if (txt != null)
        {
            _jsonMapConfig = JsonUtility.FromJson<JsonMapConfig>(txt.text);
            _jsonMapConfig.loaded = true;
        }
        else
        {
            _jsonMapConfig.loaded = false;
            _log.DebugLog(LOG_TAG, "Can't load " + mapName + ".");
        }
    }
    #endregion

    #region API
    // Singleton: retrieve instance
    public static GameMap GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GameMap();
        }
        return _instance;
    }

    public bool LoadMap()
    {
        if (!_jsonMapConfig.loaded)
        {
            LoadJsonMapConfigFromFile(MAP_FILENAME);
        }
        return _jsonMapConfig.loaded;
    }

    public int GetLevelCount()
    {
        return _jsonMapConfig.loaded ? _jsonMapConfig.levelConfigList.Count() : -1;
    }

    public JsonLevelConfig GetJsonLevelConfig(int index)
    {
        return _jsonMapConfig.levelConfigList[index];
    }

    public bool IsMapLoaded()
    {
        return _jsonMapConfig.loaded;
    }
    #endregion
}
