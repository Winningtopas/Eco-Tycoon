using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    // Singleton
    private static GridManager instance;
    public static GridManager Instance { get { return instance; } }

    [SerializeField]
    private List<Material> gridMaterials = new List<Material>();
    private GridLogic gridLogic;
    [SerializeField]
    private GridVisual gridVisual;
    private GameObject gridParent;

    private Vector3Int gridSize = new Vector3Int(100, 100, 10);
    private float tileSize = 1f;

    // Mouse selection
    [SerializeField]
    private Camera mainCamera;
    private RaycastHit hit;
    private PointerEventData pointerEventData;
    private int selectionSize = 4;

    [SerializeField]
    private bool debugNeighbours;
    [SerializeField]
    private Vector3Int debugCoordinates;

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
        gridParent.transform.position = Vector3.zero;

        CreateGrid();
    }

    private void Update()
    {
        HoverOnTile();

        if (debugNeighbours)
        {
            debugNeighbours = false;
            Debug.Log(gridLogic.GetTile(debugCoordinates).Neighbours.Count);
        }
    }

    private void HoverOnTile()
    {
        Vector2 mousePosition = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Vector3Int coordinates = Vector3Int.FloorToInt(hit.point);
            List<Vector3Int> coordinatesList = new List<Vector3Int>();

            for (int x = 0; x < selectionSize; x++)
            {
                for (int y = 0; y < selectionSize; y++)
                {
                    coordinatesList.Add(new Vector3Int(coordinates.x + x, coordinates.y + y, coordinates.z));
                }
            }

            gridVisual.HighlightTiles(coordinatesList);

            if (Input.GetMouseButtonDown(0))
                PlaceTile(coordinatesList);
            else if (Input.GetMouseButtonDown(1))
                RemoveTile(coordinates);
        }
    }

    private void PlaceTile(List<Vector3Int> coordinatesList)
    {
        for (int i = 0; i < selectionSize * selectionSize; i++)
        {
            // -1 on the z, because the tile needs to be placed with an offset.
            coordinatesList[i] = new Vector3Int(coordinatesList[i].x, coordinatesList[i].y, coordinatesList[i].z - 1);

            if (coordinatesList[i].z < 0)
            {
                Debug.LogWarning("Tile's can't be placed out of bounds!");
                break;
            }

            GridTile tile = gridLogic.GetTile(coordinatesList[i]);

            Debug.Log("tile type " + tile.Type.ToString());
            // Happens when the user clicks on a tile underneath another tile
            if (tile.Type != GridTile.BlockType.EMPTY)
                break;

            tile.ChangeType(GridTile.BlockType.NORMAL);
            gridLogic.CheckNeighbourTiles(coordinatesList[i].x, coordinatesList[i].y, coordinatesList[i].z, true);
            CreateTileGameObject(coordinatesList[i].x, coordinatesList[i].y, coordinatesList[i].z, tile, 1f, gridMaterials[1]);
            DestroyNeighbourTileGameObject(tile.Neighbours);
        }
    }

    private void RemoveTile(Vector3Int coordinates)
    {
        GridTile tile = gridLogic.GetTile(coordinates);

        tile.ChangeType(GridTile.BlockType.EMPTY);
        Destroy(tile.GetTileGameObject());

        //Now that this tile is deleted, tiles that were enclosed will be visible so they should get a gameobject.
        CreateNeighbourTileGameObject(tile.Neighbours);

        for (int i = tile.Neighbours.Count - 1; i >= 0; i--)
        {
            tile.Neighbours[i].RemoveNeighbours(tile);
            tile.RemoveNeighbours(tile.Neighbours[i]);
        }
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
                    if (tile.Neighbours.Count < 6 && tile.Type != GridTile.BlockType.EMPTY)
                        CreateTileGameObject(x, y, z, tile, tileSize, gridMaterials[1]);
                }
            }
        }
    }

    private void CreateTileGameObject(int x, int y, int z, GridTile tile, float tileSize, Material tileMaterial)
    {
        GameObject TileGameObject = new GameObject("Tile (" + x + ", " + y + ", " + z + ")");
        TileGameObject.transform.parent = gridParent.transform;
        TileGameObject.transform.position = new Vector3(x * tileSize, y * tileSize, z * tileSize);
        TileGameObject.transform.localScale = new Vector3(tileSize, tileSize, 1);
        tile.SetTileGameObject(TileGameObject);

        CreateTileMesh(TileGameObject, tile, tileMaterial);
        CreateTileCollider(TileGameObject);
    }

    //Destroys the gameobjects of tiles that are surrounded
    private void DestroyNeighbourTileGameObject(List<GridTile> tiles)
    {
        foreach (GridTile tile in tiles)
        {
            if (tile.Neighbours.Count == 6)
                Destroy(tile.GetTileGameObject());
        }
    }

    private void CreateNeighbourTileGameObject(List<GridTile> tiles)
    {
        foreach (GridTile tile in tiles)
        {
            if (tile.Neighbours.Count == 6)
                CreateTileGameObject(tile.Position.x, tile.Position.y, tile.Position.z, tile, 1f, gridMaterials[1]);
        }
    }


    private void CreateTileMesh(GameObject TileGameObject, GridTile tile, Material tileMaterial)
    {
        MeshFilter meshFilter = TileGameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = TileGameObject.AddComponent<MeshRenderer>();

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
        NeighbourCounter.transform.parent = TileGameObject.transform;
    }

    private void CreateTileCollider(GameObject TileGameObject)
    {
        TileGameObject.AddComponent<BoxCollider>();
    }
}