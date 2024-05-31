using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    // Singleton
    private static GridManager instance;
    public static GridManager Instance { get { return instance; } }

    [SerializeField]
    private List<Material> gridMaterials = new List<Material>();
    private GridLogic gridLogic;
    private GameObject gridParent;

    private Vector3Int gridSize = new Vector3Int(100, 100, 10);
    private float tileSize = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        gridParent = new GameObject("GridParent");
        gridParent.transform.parent = transform;
        gridParent.transform.position = Vector3.one;

        CreateGrid();
        Debug.Log(gridLogic.GetTile(new Vector3Int(4, 1, 0)).WorldPosition);
    }

    private void CreateGrid()
    {
        gridLogic = new GridLogic(gridSize, tileSize, gridParent.transform.position, gridMaterials);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    GridTile tile = gridLogic.GetTile(new Vector3Int(x, y, z));

                    // if the tiles have 6 neighbours they can't be seen, so they shouldn't have a mesh
                    if (tile.Neighbours.Count < 6)
                        CreateTileMesh(x, y, z, tile, tileSize, gridMaterials[1]);
                }
            }
        }
    }

    private void CreateTileMesh(int x, int y, int z, GridTile tile, float tileSize, Material tileMaterial)
    {
        GameObject Tile = new GameObject("Tile (" + x + ", " + y + ", " + z + ")");
        Tile.transform.parent = gridParent.transform;
        Tile.transform.position = new Vector3(x * tileSize, y * tileSize, z * tileSize);
        Tile.transform.localScale = new Vector3(tileSize, tileSize, 1);

        tile.SetTileGameObject(Tile);

        MeshFilter meshFilter = Tile.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = Tile.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh { name = "TileMesh" };

        mesh.vertices = new Vector3[] {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1)
        };

        mesh.triangles = new int[] {
            0, 2, 1, //face front
	        0, 3, 2,
            2, 3, 4, //face top
	        2, 4, 5,
            1, 2, 5, //face right
	        1, 5, 6,
            0, 7, 4, //face left
	        0, 4, 3,
            5, 4, 7, //face back
	        5, 7, 6,
            0, 6, 7, //face bottom
	        0, 1, 6
        };

        mesh.Optimize();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshRenderer.material = tileMaterial;

        //TO DO: This is just for debugging, remove when not needed
        GameObject NeighbourCounter = new GameObject();
        NeighbourCounter.name = "neighbour amount: " + tile.Neighbours.Count;
        NeighbourCounter.transform.parent = Tile.transform;
    }
}