using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGeneration;
using UnityEngine;
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
    
    public void SpawnObjects(List<TileData> tileData)
    {
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
