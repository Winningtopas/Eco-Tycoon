using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

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

    private Vector3Int gridSize = new Vector3Int(100, 100, 20);
    private float tileSize = 1f;

    // Mouse selection
    [SerializeField]
    private Camera mainCamera;
    private RaycastHit hit;
    private PointerEventData pointerEventData;
    private int selectionSize = 4;
    [SerializeField]
    private GridTile.BlockType currentBlockType = GridTile.BlockType.NORMAL;

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
        SwitchMaterial();
        HoverOnTile();

        if (debugNeighbours)
        {
            debugNeighbours = false;
            //Debug.Log(gridLogic.GetTile(debugCoordinates).Neighbours.Count);
        }
    }

    private void SwitchMaterial()
    {
        if (Input.GetKeyDown("1"))
            currentBlockType = (GridTile.BlockType)1;
        if (Input.GetKeyDown("2"))
            currentBlockType = (GridTile.BlockType)2;
        if (Input.GetKeyDown("3"))
            currentBlockType = (GridTile.BlockType)3;
        if (Input.GetKeyDown("4"))
            currentBlockType = (GridTile.BlockType)4;
        if (Input.GetKeyDown("5"))
            currentBlockType = (GridTile.BlockType)5;
    }

    private void HoverOnTile()
    {
        ModifySelectionArea();

        Vector2 mousePosition = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Vector3Int coordinates = Vector3Int.FloorToInt(hit.point);
            List<Vector3Int> coordinatesList = new List<Vector3Int>();

            int highestZPosition = -10000;
            int lowestZPosition = gridSize.z;

            for (int x = 0; x < selectionSize; x++)
            {
                for (int y = 0; y < selectionSize; y++)
                {
                    // -100 on the z so we can raycast down to find the lowest tile
                    Vector3Int newPosition = new Vector3Int(coordinates.x + x, coordinates.y + y, coordinates.z - 100);

                    RaycastHit bottomTileHit = new RaycastHit();
                    Debug.DrawRay(new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, newPosition.z), new Vector3(0.5f, 0.5f, newPosition.z), Color.red, 1f);

                    if (Physics.Raycast(new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, newPosition.z), new Vector3(0.5f, 0.5f, 1000), out bottomTileHit))
                    {
                        Vector3Int lowestCoordinates = Vector3Int.FloorToInt(bottomTileHit.point);
                        newPosition = lowestCoordinates;

                        if (lowestCoordinates.z > highestZPosition)
                            highestZPosition = lowestCoordinates.z;

                        if (lowestCoordinates.z < lowestZPosition && lowestCoordinates.z < gridSize.z)
                            lowestZPosition = lowestCoordinates.z;
                    }
                    if (newPosition.z < gridSize.z && newPosition.z >= 0) // TO DO: check if '=' is needed
                        coordinatesList.Add(newPosition);
                }
            }

            gridVisual.HighlightTiles(coordinatesList);

            // Remove all tiles that aren't on the highest floor (the bottom) from the list 
            List<Vector3Int> highestCoordinatesList = new List<Vector3Int>(coordinatesList);
            for (int i = highestCoordinatesList.Count - 1; i >= 0; i--)
            {
                if (highestCoordinatesList[i].z < highestZPosition)
                    highestCoordinatesList.RemoveAt(i);
            }

            // Remove all tiles that aren't on the lowest floor (the top) from the list 
            List<Vector3Int> lowestCoordinatesList = new List<Vector3Int>(coordinatesList);

            for (int i = lowestCoordinatesList.Count - 1; i >= 0; i--)
            {
                if (lowestCoordinatesList[i].z > lowestZPosition)
                    lowestCoordinatesList.RemoveAt(i);
            }

            if (Input.GetMouseButtonDown(0))
                PlaceTile(highestCoordinatesList);
            else if (Input.GetMouseButtonDown(1))
                RemoveTile(lowestCoordinatesList);
        }
    }

    private void ModifySelectionArea()
    {
        selectionSize += (int)Math.Round(Input.mouseScrollDelta.y);
        if (selectionSize <= 0)
            selectionSize = 1;
        else if (selectionSize >= 20)
            selectionSize = 20;
    }

    private void PlaceTile(List<Vector3Int> coordinatesList)
    {
        for (int i = 0; i < coordinatesList.Count; i++)
        {
            // -1 on the z, because the tile needs to be placed with an offset.
            coordinatesList[i] = new Vector3Int(coordinatesList[i].x, coordinatesList[i].y, coordinatesList[i].z - 1);

            if (coordinatesList[i].z < 0)
            {
                Debug.LogWarning("Tile's can't be placed out of bounds!");
                break;
            }
            else
            {
                GridTile tile = gridLogic.GetTile(coordinatesList[i]);

                // Happens when the user clicks on a tile underneath another tile
                if (tile.Type == GridTile.BlockType.EMPTY)
                {
                    tile.ChangeType(currentBlockType);
                    gridLogic.CheckNeighbourTiles(coordinatesList[i].x, coordinatesList[i].y, coordinatesList[i].z, true);

                    // Destroy and recreate neighbours so that their meshes get updated
                    DestroyNeighbourTileGameObject(tile);
                    CreateNeighbourTileGameObject(tile);

                    CreateTileGameObject(tile, 1f, gridLogic.GetTileMaterial(currentBlockType));
                }
            }
        }
    }

    private void RemoveTile(List<Vector3Int> coordinatesList)
    {
        for (int i = 0; i < coordinatesList.Count; i++)
        {
            GridTile tile = gridLogic.GetTile(coordinatesList[i]);

            tile.ChangeType(GridTile.BlockType.EMPTY);
            Destroy(tile.GetTileGameObject());

            //Now that this tile is deleted, tiles that were enclosed will be visible so they should get a gameobject.

            for (int j = tile.Neighbours.Length - 1; j >= 0; j--)
            {
                if (tile.Neighbours[j] != null)
                {
                    // Destroy neighbour tile -> Update the neighbour amount on the neighbour
                    // -> Create neighbour tile -> remove the neighbour as reference from this tile
                    if (tile.Neighbours[j].GetTileGameObject() != null)
                        Destroy(tile.Neighbours[j].GetTileGameObject());
                    tile.Neighbours[j].RemoveNeighbours(tile);
                    CreateTileGameObject(tile.Neighbours[j], 1f, gridLogic.GetTileMaterial(tile.Neighbours[j].Type));
                    tile.RemoveNeighbours(tile.Neighbours[j]);
                }
            }
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
                    //if (tile.Neighbours.Count < 6 && tile.Type != GridTile.BlockType.EMPTY)
                    if (tile.Neighbourcount < 6 && tile.Type != GridTile.BlockType.EMPTY)
                        CreateTileGameObject(tile, tileSize, gridLogic.GetTileMaterial(tile.Type));
                }
            }
        }
    }

    //Destroys the gameobjects of tiles that are surrounded
    private void DestroyNeighbourTileGameObject(GridTile tile)
    {
        foreach (GridTile neighbour in tile.Neighbours)
        {
            if (neighbour != null)
                Destroy(neighbour.GetTileGameObject());
        }
    }

    private void CreateNeighbourTileGameObject(GridTile tile)
    {
        foreach (GridTile neighbour in tile.Neighbours)
        {
            if (neighbour != null)
                CreateTileGameObject(neighbour, 1f, gridLogic.GetTileMaterial(neighbour.Type));
        }
    }

    private void CreateTileGameObject(GridTile tile, float tileSize, Material tileMaterial)
    {
        GameObject TileGameObject = new GameObject("Tile (" + tile.Position.x + ", " + tile.Position.y + ", " + tile.Position.z + ")");
        TileGameObject.transform.parent = gridParent.transform;
        TileGameObject.transform.position = new Vector3(tile.Position.x * tileSize, tile.Position.y * tileSize, tile.Position.z * tileSize);
        TileGameObject.transform.localScale = new Vector3(tileSize, tileSize, 1);
        tile.SetTileGameObject(TileGameObject);

        CreateTileMesh(TileGameObject, tile, tileMaterial);
        CreateTileCollider(TileGameObject);
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

        List<int> triangles = new List<int>();

        // only draw faces that don't have a neighbour
        if (tile.Neighbours[0] == null)
            triangles.AddRange(new int[] { 0, 7, 4, 0, 4, 3 }); // left
        if (tile.Neighbours[1] == null)
            triangles.AddRange(new int[] { 1, 2, 5, 1, 5, 6 }); // right
        if (tile.Neighbours[2] == null)
            triangles.AddRange(new int[] { 0, 6, 7, 0, 1, 6 }); // front
        if (tile.Neighbours[3] == null)
            triangles.AddRange(new int[] { 2, 3, 4, 2, 4, 5 }); // back
        if (tile.Neighbours[4] == null)
            triangles.AddRange(new int[] { 0, 2, 1, 0, 3, 2 }); // above
        if (tile.Neighbours[5] == null && tile.Position.z < gridSize.z - 1) // the lowest layer of tiles doesn't need a bottom face
            triangles.AddRange(new int[] { 5, 4, 7, 5, 7, 6 }); // below

        mesh.triangles = triangles.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshRenderer.material = tileMaterial;

        //TO DO: This is just for debugging, remove when not needed
        GameObject NeighbourCounter = new GameObject();
        NeighbourCounter.name = "neighbour amount: " + tile.Neighbourcount;
        NeighbourCounter.transform.parent = TileGameObject.transform;
    }

    private void CreateTileCollider(GameObject TileGameObject)
    {
        TileGameObject.AddComponent<BoxCollider>();
    }
}