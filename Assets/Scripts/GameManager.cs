using System.Collections;
using System.Collections.Generic;
using TerrainGeneration;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        
        Init();
    }

    #endregion

    public static TerrainGenerator TerrainGenerator;
    public bool deathEnabled;

    void Init()
    {
        TerrainGenerator = FindObjectOfType<TerrainGenerator>();
    }
}
