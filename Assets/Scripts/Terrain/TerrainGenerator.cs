﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration {

    [Serializable]
    public class TileData
    {
        public TerrainGenerator.Biome biome;
        public Vector3 worldPos;
    }
    
    [ExecuteInEditMode]
    public class TerrainGenerator : MonoBehaviour {

        const string meshHolderName = "Terrain Mesh";

        public bool autoUpdate = true;

        public bool centralize = true;
        public int worldSize = 20;
        public float waterDepth = .2f;
        public float edgeDepth = .2f;

        public NoiseSettings terrainNoise;
        public Material mat;

        public Biome water;
        public Biome sand;
        public Biome grass;

        [Header ("Info")]
        public int numTiles;
        public int numLandTiles;
        public int numWaterTiles;
        public float waterPercent;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;

        bool needsUpdate;

        List<Color> colors = new List<Color>();
        Color[] startCols;
        Color[] endCols;

        [Header("Test")] 
        public int xd;
        public int yd;
        
        float[,] map;
        private TerrainData terrainData;
         
        void Update () {
//            if (Input.GetKeyDown("n"))
//            {
//                foreach (var tileData in GetAllTileData())
//                {
//                    Instantiate(new GameObject(), tileData.worldPos, Quaternion.identity);    
//                }
//            }
            
            if (needsUpdate && autoUpdate) {
                needsUpdate = false;
                terrainData = Generate ();
            } else {
                if (!Application.isPlaying) {
                    UpdateColours();
                }
            }
        }

        private void Start()
        {
            terrainData = Generate();
            if (Application.isPlaying)
            {
                //FindObjectOfType<ObjectSpawner>().SpawnObjects(GetAllTileData());
            }
        }

        public TileData GetTileDataAt(int x, int y)
        {
            if (!InGrid(x, y))
            {
                return null;
            }

            TileData td = new TileData()
            {
                biome = GetBiomeAt(x, y),
                worldPos = terrainData.tileCentres[x, y]
            };

            return td;
        }

        bool InGrid(int x, int y)
        {
            return x >= 0 && x < Mathf.CeilToInt(worldSize) &&
                   y >= 0 && y < Mathf.CeilToInt(worldSize);
        }

        public List<TileData> GetAllTileData()
        {
            List<TileData> tileDataSet = new List<TileData>();
            
            for (int i = 0; i < worldSize; i++)
            {
                for (int j = 0; j < worldSize; j++)
                {
                    tileDataSet.Add(GetTileDataAt(i, j));
                }    
            }

            return tileDataSet;
        }
        
        public TerrainData Generate () {
            //Init colors
            colors.Clear();
            startCols = new Color[] {water.startCol, sand.startCol, grass.startCol};
            endCols = new Color[] {water.endCol, sand.endCol, grass.endCol};

            CreateMeshComponents ();

            int numTilesPerLine = Mathf.CeilToInt (worldSize);
            float min = (centralize) ? -numTilesPerLine / 2f : 0;
            map = HeightmapGenerator.GenerateHeightmap (terrainNoise, numTilesPerLine);

            var vertices = new List<Vector3> ();
            var triangles = new List<int> ();
            var uvs = new List<Vector2> ();
            var normals = new List<Vector3> ();

            // Some convenience stuff:
            var biomes = new Biome[] { water, sand, grass };
            Vector3[] upVectorX4 = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            Coord[] nswe = { Coord.up, Coord.down, Coord.left, Coord.right };
            int[][] sideVertIndexByDir = { new int[] { 0, 1 }, new int[] { 3, 2 }, new int[] { 2, 0 }, new int[] { 1, 3 } };
            Vector3[] sideNormalsByDir = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            // Terrain data:
            var terrainData = new TerrainData (numTilesPerLine);
            numLandTiles = 0;
            numWaterTiles = 0;

            for (int y = 0; y < numTilesPerLine; y++) {
                for (int x = 0; x < numTilesPerLine; x++) {
                    Vector2 uv = GetBiomeInfo (map[x, y], biomes);

                    var color = Color.Lerp(startCols[(int)uv.x], endCols[(int)uv.x], uv.y);
                    colors.AddRange(new[] { color, color, color, color });
                    //uvs.AddRange (new Vector2[] { uv, uv, uv, uv });

                    bool isWaterTile = uv.x == 0f;
                    bool isLandTile = !isWaterTile;
                    if (isWaterTile) {
                        numWaterTiles++;
                    } else {
                        numLandTiles++;
                    }

                    // Vertices
                    int vertIndex = vertices.Count;
                    float height = (isWaterTile) ? -waterDepth : 0;

                    Vector3 nw = new Vector3 (min + x, height, min + y + 1);
                    Vector3 ne = nw + Vector3.right;
                    Vector3 sw = nw - Vector3.forward;
                    Vector3 se = sw + Vector3.right;
                    Vector3[] tileVertices = { nw, ne, sw, se };
                    vertices.AddRange (tileVertices);
                    normals.AddRange (upVectorX4);

                    // Add triangles
                    triangles.Add (vertIndex);
                    triangles.Add (vertIndex + 1);
                    triangles.Add (vertIndex + 2);
                    triangles.Add (vertIndex + 1);
                    triangles.Add (vertIndex + 3);
                    triangles.Add (vertIndex + 2);

                    // Bridge gaps between water and land tiles, and also fill in sides of map
                    bool isEdgeTile = x == 0 || x == numTilesPerLine - 1 || y == 0 || y == numTilesPerLine - 1;
                    if (isLandTile || isEdgeTile) {
                        for (int i = 0; i < nswe.Length; i++) {
                            int neighbourX = x + nswe[i].x;
                            int neighbourY = y + nswe[i].y;
                            bool neighbourIsOutOfBounds = neighbourX < 0 || neighbourX >= numTilesPerLine || neighbourY < 0 || neighbourY >= numTilesPerLine;
                            bool neighbourIsWater = false;
                            if (!neighbourIsOutOfBounds) {
                                float neighbourHeight = map[neighbourX, neighbourY];
                                neighbourIsWater = neighbourHeight <= biomes[0].height;
                                if (neighbourIsWater) {
                                    terrainData.shore[neighbourX, neighbourY] = true;
                                }
                            }

                            if (neighbourIsOutOfBounds || (isLandTile && neighbourIsWater)) {
                                float depth = waterDepth;
                                if (neighbourIsOutOfBounds) {
                                    depth = (isWaterTile) ? edgeDepth : edgeDepth + waterDepth;
                                }
                                vertIndex = vertices.Count;
                                int edgeVertIndexA = sideVertIndexByDir[i][0];
                                int edgeVertIndexB = sideVertIndexByDir[i][1];
                                vertices.Add (tileVertices[edgeVertIndexA]);
                                vertices.Add (tileVertices[edgeVertIndexA] + Vector3.down * depth);
                                vertices.Add (tileVertices[edgeVertIndexB]);
                                vertices.Add (tileVertices[edgeVertIndexB] + Vector3.down * depth);

                                //uvs.AddRange (new Vector2[] { uv, uv, uv, uv });
                                colors.AddRange(new[] { color, color, color, color });
                                int[] sideTriIndices = { vertIndex, vertIndex + 1, vertIndex + 2, vertIndex + 1, vertIndex + 3, vertIndex + 2 };
                                triangles.AddRange (sideTriIndices);
                                normals.AddRange (new Vector3[] { sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i] });
                            }
                        }
                    }

                    // Terrain data:
                    terrainData.tileCentres[x, y] = nw + new Vector3 (0.5f, 0, -0.5f);
                    terrainData.walkable[x, y] = isLandTile;
                }
            }

            // Update mesh:
            mesh.SetVertices (vertices);
            mesh.SetTriangles (triangles, 0, true);
            mesh.SetUVs (0, uvs);
            mesh.SetNormals (normals);
            mesh.SetColors(colors);

            meshRenderer.sharedMaterial = mat;
            //UpdateColours ();

            numTiles = numLandTiles + numWaterTiles;
            waterPercent = numWaterTiles / (float) numTiles;
            return terrainData;
        }

        void UpdateColours () {
            if (mat != null) {
                Color[] startCols = { water.startCol, sand.startCol, grass.startCol };
                Color[] endCols = { water.endCol, sand.endCol, grass.endCol };
                //mat.SetColorArray ("_StartCols", startCols);
                mat.SetColorArray ("_EndCols", endCols);
            }
        }

        Vector2 GetBiomeInfo (float height, Biome[] biomes) {
            // Find current biome
            int biomeIndex = 0;
            float biomeStartHeight = 0;
            for (int i = 0; i < biomes.Length; i++) {
                if (height <= biomes[i].height) {
                    biomeIndex = i;
                    break;
                }
                biomeStartHeight = biomes[i].height;
            }

            Biome biome = biomes[biomeIndex];
            float sampleT = Mathf.InverseLerp (biomeStartHeight, biome.height, height);
            sampleT = (int) (sampleT * biome.numSteps) / (float) Mathf.Max (biome.numSteps, 1);

            // UV stores x: biomeIndex and y: val between 0 and 1 for how close to prev/next biome
            Vector2 uv = new Vector2 (biomeIndex, sampleT);
            return uv;
        }

        
        Biome GetBiomeAt(int x, int y)
        {
            if (!InGrid(x, y))
            {
                return null;
            }

            Biome[] globalBiomes = new Biome[] { water, sand, grass };
            return GetBiome(map[x, y], globalBiomes);
        }

        Biome GetBiome(float height, Biome[] biomes)
        {
            int biomeIndex = 0;
            float biomeStartHeight = 0;
            for (int i = 0; i < biomes.Length; i++) {
                if (height <= biomes[i].height) {
                    biomeIndex = i;
                    break;
                }
                biomeStartHeight = biomes[i].height;
            }

            return biomes[biomeIndex];
        }

        void CreateMeshComponents () {
            GameObject holder = null;

            if (meshFilter == null) {
                if (GameObject.Find (meshHolderName)) {
                    holder = GameObject.Find (meshHolderName);
                } else {
                    holder = new GameObject (meshHolderName);
                    holder.AddComponent<MeshRenderer> ();
                    holder.AddComponent<MeshFilter> ();
                }
                meshFilter = holder.GetComponent<MeshFilter> ();
                meshRenderer = holder.GetComponent<MeshRenderer> ();
            }

            if (meshFilter.sharedMesh == null) {
                mesh = new Mesh ();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilter.sharedMesh = mesh;
            } else {
                mesh = meshFilter.sharedMesh;
                mesh.Clear ();
            }

            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void OnValidate () {
            needsUpdate = true;
        }

        public enum BiomeType
        {
            Water,
            Grass,
            Sand
        }
        
        [System.Serializable]
        public class Biome
        {
            [Range(0, 1)] 
            public float height;
            public Color startCol;
            public Color endCol;
            public int numSteps;
            public BiomeType type;
        }

        public class TerrainData {
            public int size;
            public Vector3[, ] tileCentres;
            public bool[, ] walkable;
            public bool[, ] shore;

            public TerrainData (int size) {
                this.size = size;
                tileCentres = new Vector3[size, size];
                walkable = new bool[size, size];
                shore = new bool[size, size];
            }
        }
    }
}