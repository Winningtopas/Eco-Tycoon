using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Grid
{
    public class GridLogic
    {
        private Vector3Int dimensions;
        private float tileSize;
        private Vector3 originPosition;
        private List<Material> gridMaterials;

        private GridTile[,,] gridTiles;

        public GridLogic(Vector3Int dimensions, float tileSize, Vector3 originPosition, List<Material> gridMaterials)
        {
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
                        gridTiles[x, y, z] = new GridTile(new Vector3Int(x, y, z), tileSize, GridTile.BlockType.NORMAL);
                        CheckNeighbourTiles(x, y, z);
                    }
                }
            }
        }

        private void CheckNeighbourTiles(int x, int y, int z)
        {
            GridTile baseTile = gridTiles[x, y, z];
            GridTile targetTile;

            if (baseTile.Type == GridTile.BlockType.EMPTY)
                return;

            if (x > 0)
            {
                targetTile = gridTiles[x - 1, y, z];
                if (targetTile.Type != GridTile.BlockType.EMPTY)
                {
                    baseTile.AddNeighbours(targetTile);
                    targetTile.AddNeighbours(baseTile);
                }
            }
            if (y > 0)
            {
                targetTile = gridTiles[x, y - 1, z];
                if (targetTile.Type != GridTile.BlockType.EMPTY)
                {
                    baseTile.AddNeighbours(targetTile);
                    targetTile.AddNeighbours(baseTile);
                }
            }
            if (z > 0)
            {
                targetTile = gridTiles[x, y, z - 1];
                if (targetTile.Type != GridTile.BlockType.EMPTY)
                {
                    baseTile.AddNeighbours(targetTile);
                    targetTile.AddNeighbours(baseTile);
                }
            }
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