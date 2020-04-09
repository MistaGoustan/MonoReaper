﻿using Microsoft.Xna.Framework;
using Reaper.Engine;

namespace Reaper.Behaviors.Common
{
    /// <summary>
    /// The layout will follow whichever world object has the scroll-to behavior.
    /// </summary>
    public class ScrollToBehavior : Behavior
    {
        public ScrollToBehavior(WorldObject owner) : base(owner) { }

        public float Smoothing { get; set; } = 0.3f;

        public override void OnLayoutStarted()
        {
            Layout.View.Position = Owner.Position;
        }

        public override void PostTick(GameTime gameTime)
        {
            Layout.View.Position = Vector2.SmoothStep(Layout.View.Position, Owner.Position, Smoothing);
        }
    }
}