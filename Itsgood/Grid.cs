﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItsGood
{
    internal class Grid
    {
        struct Cell
        {
            public List<WorldObject> WorldObjects;
        }

        private readonly int _cellSize;
        private readonly int _width;
        private readonly int _height;
        private readonly Cell[,] _cells;

        public Grid(int cellSize, int width, int height)
        {
            _cellSize = cellSize;
            _width = width;
            _height = height;
            _cells = new Cell[width, height];

            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; i < _height; i++)
                {
                    _cells[i, j].WorldObjects = new List<WorldObject>();
                }
            }
        }

        public void Add(WorldObject worldObject)
        {
            GetCellPosition(worldObject.Position, out int cellX, out int cellY);

            _cells[cellX, cellY].WorldObjects.Add(worldObject);
        }

        public void Remove(WorldObject worldObject)
        {
            GetCellPosition(worldObject.Position, out int cellX, out int cellY);

            _cells[cellX, cellY].WorldObjects.Remove(worldObject);
        }

        public void Update(WorldObject worldObject)
        {
            GetCellPosition(worldObject.PreviousPosition, out int previousCellX, out int previousCellY);
            GetCellPosition(worldObject.Position, out int cellX, out int cellY);

            if (previousCellX == cellX && previousCellY == cellY)
                return;

            _cells[previousCellX, previousCellY].WorldObjects.Remove(worldObject);
            _cells[cellX, cellY].WorldObjects.Add(worldObject);
        }

        public bool TestOverlapOffset(WorldObject worldObject, float xOffset, float yOffset)
        {
            var testedPosition = worldObject.Position + new Vector2(xOffset, yOffset);
            int width = worldObject.Source.Width;
            int height = worldObject.Source.Height;
            var bounds = new Rectangle((int)Math.Round(testedPosition.X - width * 0.5f), (int)Math.Round(testedPosition.Y - height * 0.5f), width, height);

            GetCellPosition(testedPosition, out int cellX, out int cellY);

            return _cells[cellX, cellY].WorldObjects.Any(other => other != worldObject && other.IsSolid && other.Bounds.Intersects(bounds));
        }

        public bool TestOverlapOffset(WorldObject worldObject, float xOffset, float yOffset, out WorldObject overlappedWorldObject)
        {
            var testedPosition = worldObject.Position + new Vector2(xOffset, yOffset);
            int width = worldObject.Source.Width;
            int height = worldObject.Source.Height;
            var bounds = new Rectangle((int)Math.Round(testedPosition.X - width * 0.5f), (int)Math.Round(testedPosition.Y - height * 0.5f), width, height);

            GetCellPosition(testedPosition, out int cellX, out int cellY);

            overlappedWorldObject = _cells[cellX, cellY].WorldObjects.FirstOrDefault(other => other != worldObject && other.IsSolid && other.Bounds.Intersects(bounds));
            return overlappedWorldObject != null;
        }

        public IEnumerable<WorldObject> TestOverlap(Rectangle bounds)
        {
            GetCellPosition(bounds.Center.ToVector2(), out int cellX, out int cellY);

            return _cells[cellX, cellY].WorldObjects.Where(other => other.Bounds.Intersects(bounds));
        }

        private void GetCellPosition(Vector2 position, out int column, out int row)
        {
            column = (int)(position.X / _cellSize);
            row = (int)(position.Y / _cellSize);
        }
    }
}
