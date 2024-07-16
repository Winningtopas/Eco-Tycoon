using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid;
using Newtonsoft.Json.Linq;
using System;

public class GridTile
{
    public enum BlockType { EMPTY, GRASS, WATER, LAVA, SWAMP, SAND};
    public enum StateOfMatter { SOLID, LIQUID, GAS, PLASMA};

    private Vector3Int position;
    private Vector3Int worldPosition;
    private BlockType type;
    private bool hasOccupant;

    private GridTile[] neighbours = new GridTile[6];
    private int neighbourCount;

    private GameObject tileGameObject;
    private GameObject entityPrefab;
    private GameObject occupantGameObject;

    public Vector3Int Position => position;
    public Vector3Int WorldPosition => worldPosition;
    public GridTile[] Neighbours => neighbours;
    public int Neighbourcount => neighbourCount;
    public BlockType Type => type;
    public bool HasOccupant => hasOccupant;

    public GridTile(Vector3Int position, float cellSize, BlockType type)
    {
        worldPosition = new Vector3Int((int)Mathf.Floor(position.x * cellSize), (int)Mathf.Floor(position.y * cellSize));
        this.position = position;
        this.type = type;
    }

    public void AddNeighbour(GridTile tile)
    {
        int index = -1;

        if(tile.position == new Vector3Int(position.x - 1, position.y, position.z))
            index = 0;
        if (tile.position == new Vector3Int(position.x + 1, position.y, position.z))
            index = 1;
        if (tile.position == new Vector3Int(position.x, position.y - 1, position.z))
            index = 2;
        if (tile.position == new Vector3Int(position.x, position.y + 1, position.z))
            index = 3;
        if (tile.position == new Vector3Int(position.x, position.y, position.z - 1))
            index = 4;
        if (tile.position == new Vector3Int(position.x, position.y, position.z + 1))
            index = 5;

        if (neighbours[index] == null)
            neighbourCount++;
        neighbours[index] = tile;
    }

    //public void AddNeighbours(GridTile tile)
    //{
    //    if (neighbours.Contains(tile))
    //        return;

    //    neighbours.Add(tile);
    //}

    public void RemoveNeighbours(GridTile tile)
    {
        int index = -1;

        if (tile.position == new Vector3Int(position.x - 1, position.y, position.z))
            index = 0;
        if (tile.position == new Vector3Int(position.x + 1, position.y, position.z))
            index = 1;
        if (tile.position == new Vector3Int(position.x, position.y - 1, position.z))
            index = 2;
        if (tile.position == new Vector3Int(position.x, position.y + 1, position.z))
            index = 3;
        if (tile.position == new Vector3Int(position.x, position.y, position.z - 1))
            index = 4;
        if (tile.position == new Vector3Int(position.x, position.y, position.z + 1))
            index = 5;

        if (neighbours[index] != null)
            neighbourCount--;
        neighbours[index] = null;
    }

    public void RemoveTile()
    {
        type = BlockType.EMPTY;
        foreach (GridTile tile in neighbours)
        {
            tile.RemoveNeighbours(this);
            RemoveNeighbours(tile);
        }
    }

    public void SetTileGameObject(GameObject gameObject)
    {
        this.tileGameObject = gameObject;
    }

    public GameObject GetTileGameObject()
    {
        return this.tileGameObject;
    }

    public GameObject GetEntityPrefab()
    {
        return this.entityPrefab;
    }
    public GameObject GetOccupantGameObject()
    {
        return this.tileGameObject;
    }

    public void SetEntityPrefab(GameObject prefab, float cellSize)
    {
        this.entityPrefab = prefab;
        prefab.transform.position = new Vector3(position.x * cellSize + cellSize * .5f, position.y * cellSize + cellSize * .5f, prefab.transform.position.z);
    }

    public void ChangeType(BlockType type)
    {
        this.type = type;
    }

    public void SetOccupantGameObject(GameObject gameObject)
    {
        if (hasOccupant)
            return;

        hasOccupant = true;
        tileGameObject = gameObject;
    }

    public void RemoveOccupantGameObject()
    {
        tileGameObject = null;
        hasOccupant = false;
    }
}