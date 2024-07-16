using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static GridTile;

namespace Grid
{
    public class GridLogic
    {
        private Vector3Int dimensions;
        private float tileSize;
        private Vector3 originPosition;
        private List<Material> gridMaterials;
        Dictionary<BlockType, Material> materialDictionary = new Dictionary<BlockType, Material>();

        private GridTile[,,] gridTiles;

        public GridLogic(Vector3Int dimensions, float tileSize, Vector3 originPosition, List<Material> gridMaterials)
        {
            // Start from 1 higher to skip the Empty tile type
            for (int i = 1; i < gridMaterials.Count + 1; i++)
            {
                materialDictionary.Add((BlockType)i, gridMaterials[i - 1]);
            }

            this.dimensions = dimensions;
            this.tileSize = tileSize;
            this.originPosition = originPosition;
            this.gridMaterials = gridMaterials;

            gridTiles = new GridTile[dimensions.x, dimensions.y, dimensions.z];

            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        GridTile.BlockType type = z < dimensions.z - 5 ? GridTile.BlockType.EMPTY : type = GridTile.BlockType.GRASS;

                        gridTiles[x, y, z] = new GridTile(new Vector3Int(x, y, z), tileSize, type);
                        CheckNeighbourTiles(x, y, z, false);
                    }
                }
            }
            DrawBoundingBox();
        }

        private void DrawBoundingBox()
        {
            Debug.DrawLine(GetWorldPosition(0, 0, 0), GetWorldPosition(dimensions.x, 0, 0), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(0, 0, 0), GetWorldPosition(0, dimensions.y, 0), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(dimensions.x, dimensions.y, 0), GetWorldPosition(dimensions.x, 0, 0), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(dimensions.x, dimensions.y, 0), GetWorldPosition(0, dimensions.y, 0), Color.white, Mathf.Infinity);

            Debug.DrawLine(GetWorldPosition(0, 0, dimensions.z), GetWorldPosition(dimensions.x, 0, dimensions.z), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(0, 0, dimensions.z), GetWorldPosition(0, dimensions.y, dimensions.z), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(dimensions.x, dimensions.y, dimensions.z), GetWorldPosition(dimensions.x, 0, dimensions.z), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(dimensions.x, dimensions.y, dimensions.z), GetWorldPosition(0, dimensions.y, dimensions.z), Color.white, Mathf.Infinity);

            Debug.DrawLine(GetWorldPosition(0, 0, 0), GetWorldPosition(0, 0, dimensions.z), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(dimensions.x, 0, 0), GetWorldPosition(dimensions.x, 0, dimensions.z), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(0, dimensions.y, 0), GetWorldPosition(0, dimensions.y, dimensions.z), Color.white, Mathf.Infinity);
            Debug.DrawLine(GetWorldPosition(dimensions.x, dimensions.y, 0), GetWorldPosition(dimensions.x, dimensions.y, dimensions.z), Color.white, Mathf.Infinity);
        }

        //Upon initial creation I know the order of the tile generation, with that knowledge only half the neighbours need to be searched.
        public void CheckNeighbourTiles(int x, int y, int z, bool fullSearch)
        {
            GridTile baseTile = gridTiles[x, y, z];
            GridTile targetTile;

            if (baseTile.Type == GridTile.BlockType.EMPTY)
                return;

            if (x > 0 && x < dimensions.x)
            {
                targetTile = gridTiles[x - 1, y, z];
                if (targetTile.Type != GridTile.BlockType.EMPTY)
                {
                    baseTile.AddNeighbour(targetTile);
                    targetTile.AddNeighbour(baseTile);
                }
                if (fullSearch)
                {
                    targetTile = gridTiles[x + 1, y, z];
                    if (targetTile.Type != GridTile.BlockType.EMPTY)
                    {
                        baseTile.AddNeighbour(targetTile);
                        targetTile.AddNeighbour(baseTile);
                    }
                }
            }
            if (y > 0 && y < dimensions.y)
            {
                targetTile = gridTiles[x, y - 1, z];
                if (targetTile.Type != GridTile.BlockType.EMPTY)
                {
                    baseTile.AddNeighbour(targetTile);
                    targetTile.AddNeighbour(baseTile);
                }
                if (fullSearch)
                {
                    targetTile = gridTiles[x, y + 1, z];
                    if (targetTile.Type != GridTile.BlockType.EMPTY)
                    {
                        baseTile.AddNeighbour(targetTile);
                        targetTile.AddNeighbour(baseTile);
                    }
                }
            }
            if (z > 0 && z < dimensions.z)
            {
                targetTile = gridTiles[x, y, z - 1];
                if (targetTile.Type != GridTile.BlockType.EMPTY)
                {
                    baseTile.AddNeighbour(targetTile);
                    targetTile.AddNeighbour(baseTile);
                }
                if (fullSearch)
                {
                    targetTile = gridTiles[x, y, z + 1];
                    if (targetTile.Type != GridTile.BlockType.EMPTY)
                    {
                        baseTile.AddNeighbour(targetTile);
                        targetTile.AddNeighbour(baseTile);
                    }
                }
            }
        }

        public Material GetTileMaterial(BlockType type)
        {
            Material tileMaterial = materialDictionary[type];
            return tileMaterial;
        }

        public Vector3 GetWorldPosition(int x, int y, int z)
        {
            return new Vector3(x, y, z) * tileSize + originPosition;
        }

        public GridTile GetTile(Vector3Int position)
        {
            if (position.x >= 0 && position.y >= 0 && position.z >= 0
                && position.x < dimensions.x && position.y < dimensions.y && position.z < dimensions.z)
            {
                return gridTiles[position.x, position.y, position.z];
            }
            else
            {
                return null;
            }
        }
    }
}