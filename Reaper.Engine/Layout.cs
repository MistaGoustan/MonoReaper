﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Reaper.Engine.Tools;
using System;
using System.Collections.Generic;

namespace Reaper.Engine
{
    /// <summary>
    /// Layouts can range from boss battles, levels, to menu screens.
    /// 
    /// - Layouts contain world objects.
    /// - Layouts are mostly a pass through for their components (the view, spatial grid, and object lists, etc...).
    /// </summary>
    public sealed class Layout
    {
        private readonly ContentManager _content;
        private readonly LayoutGrid _grid;
        private readonly WorldObjectList _worldObjectList;

        public Layout(MainGame game, int cellSize, int width, int height)
        {
            Game = game;
            Width = width;
            Height = height;

            _content = new ContentManager(game.Services, "Content");
            View = new LayoutView(game, this);
            _grid = new LayoutGrid(cellSize, (int)Math.Ceiling((double)width / cellSize), (int)Math.Ceiling((double)height / cellSize));
            _worldObjectList = new WorldObjectList(this, _content);
        }

        public LayoutView View { get; }
        public IGame Game { get; }

        public int Width { get; }
        public int Height { get; }

        public WorldObject Spawn(WorldObjectDefinition definition, Vector2 position)
        {
            var worldObject = _worldObjectList.Create(definition, position);
            worldObject.UpdateBBox();

            _grid.Add(worldObject);

            return worldObject;
        }

        public T GetWorldObjectAsBehavior<T>() where T : Behavior
        {
            return _worldObjectList.GetWorldObjectAsBehavior<T>();
        }

        public bool TestOverlapSolidOffset(WorldObject worldObject, float xOffset, float yOffset) 
        {
            return _grid.TestSolidOverlapOffset(worldObject, xOffset, yOffset);
        }

        public bool TestOverlapSolidOffset(WorldObject worldObject, float xOffset, float yOffset, out WorldObject overlappedWorldObject)
        {
            return _grid.TestSolidOverlapOffset(worldObject, xOffset, yOffset, out overlappedWorldObject);
        }

        public IEnumerable<WorldObject> QueryBounds(Rectangle bounds) 
        {
            return _grid.QueryBounds(bounds);
        }

        internal void UpdateGridCell(WorldObject worldObject)
        {
            _grid.Update(worldObject);
        }

        internal void Destroy(WorldObject worldObject)
        {
            _grid.Remove(worldObject);
            _worldObjectList.DestroyObject(worldObject);
        }

        internal void Tick(GameTime gameTime)
        {
            _worldObjectList.Tick(gameTime);
        }

        internal void PostTick(GameTime gameTime)
        {
            _worldObjectList.PostTick(gameTime);
        }

        internal void Draw()
        {
            View.BeginDraw();
            _worldObjectList.Draw(View);

            DebugTools.Draw(View.SpriteBatch, _worldObjectList.WorldObjects);

            View.EndDraw();
        }

        internal void Unload() 
        {
            View.Unload();
            _content.Unload();
        }
    }
}
