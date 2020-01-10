using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerrainGeneration;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class GameObjectFloatDict : SerializableDictionary<GameObject, float> { }

[Serializable]
public class BiomeSpawnData
{
    public TerrainGenerator.BiomeType biomeType;
    public GameObjectFloatDict spawnWeightDictionary;
    public float emptyObjectChance;
}
public class ObjectSpawner : MonoBehaviour
{
    public List<BiomeSpawnData> biomeSpawnData;
    [SerializeField] private Transform objectsParent;
    [SerializeField] private List<TileData> tileData;
    
    [Header("Spawn plants")]
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private float plantSpawnOffset = 5;
    [SerializeField] private LayerMask terrainLayer;
    
    public void SpawnObjects(List<TileData> tileData)
    {
        this.tileData = tileData;
        
        foreach (var data in tileData)
        {
            var bsd = biomeSpawnData.FirstOrDefault(d => d.biomeType == data.biome.type);

            if (bsd != null)
            {
                var newObj = GetRouletteSpinObj(bsd.spawnWeightDictionary, bsd.emptyObjectChance);

                if (newObj != null)
                {
                    Instantiate(newObj, data.worldPos, Quaternion.identity, objectsParent);
                }
            }
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnPlantsCoroutine());
    }

    IEnumerator SpawnPlantsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(plantSpawnOffset);
            SpawnPlant();
        }
    }

    public static int ToLayer ( int bitmask ) {
        int result = bitmask>0 ? 0 : 31;
        while( bitmask>1 ) {
            bitmask = bitmask>>1;
            result++;
        }
        return result;
    }

    public bool SpawnPlant()
    {
        var randomTile = tileData[Random.Range(0, tileData.Count)];

        if (randomTile.biome.type == TerrainGenerator.BiomeType.Water)
        {
            return false;
        }
        
        RaycastHit hit;
        if (Physics.Raycast(randomTile.worldPos + new Vector3(0, 30, 0), transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            //TODO shady layer number extraction
            if (hit.transform.gameObject.layer == ToLayer(terrainLayer.value))
            {
                var newPlant = Instantiate(
                    plantPrefab, 
                    new Vector3(randomTile.worldPos.x, 0 , randomTile.worldPos.z),
                    Quaternion.identity);
                newPlant.transform.localScale = Vector3.zero;
                newPlant.transform.DOScale(plantPrefab.transform.localScale, 1);

                return true;
            }
        }

        return false;
    }

    public T GetRouletteSpinObj<T>(IDictionary<T, float> dict, float emptyValChance)
    {
        float sum = 0;

        //TODO can be computed once per biome
        var valuesDict = new Dictionary<float, T>();
        foreach (var kvp in dict)
        {
            sum += kvp.Value;
            valuesDict.Add(sum, kvp.Key);
        }

        sum += emptyValChance;
        
        float randomVal = Random.Range(0, sum);

        foreach (var kvp in valuesDict)
        {
            if (randomVal <= kvp.Key)
            {
                return kvp.Value;
            }
        }

        return default(T);
    }

    public void RemoveAllObjects()
    {
        foreach (Transform t in objectsParent.Cast<Transform>().ToList())
        {
            DestroyImmediate(t.gameObject);
        }
    }
}



#if UNITY_EDITOR

[CustomEditor(typeof(ObjectSpawner))]
public class LevelScriptEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        ObjectSpawner objectSpawner = (ObjectSpawner)target;

        DrawDefaultInspector();
        
        if(GUILayout.Button("Re-generate"))
        {
            objectSpawner.RemoveAllObjects();
            TerrainGenerator tg = FindObjectOfType<TerrainGenerator>();
            objectSpawner.SpawnObjects(tg.GetAllTileData());
        }
        
        if(GUILayout.Button("Spawn Objects"))
        {
            TerrainGenerator tg = FindObjectOfType<TerrainGenerator>();
            objectSpawner.SpawnObjects(tg.GetAllTileData());
        }
        
        if(GUILayout.Button("Remove all objects"))
        {
            objectSpawner.RemoveAllObjects();
        }
    }
}

#endif
