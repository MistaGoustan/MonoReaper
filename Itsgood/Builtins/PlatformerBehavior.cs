﻿using ItsGood.Utils.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace ItsGood.Builtins
{
    public class PlatformerBehavior : Behavior
    {
        public interface IAnimationCallbacks
        {
            void OnMoved();
            void OnStopped();
            void OnJumped();
            void OnLanded();
        }

        private const float MOVE_ACCELERATION = 1500f;
        private const float MAX_MOVE_SPEED = 400f;
        private const float DRAG = 0.8f;
        private const float MAX_JUMP_TIME = 0.35f;
        private const float JUMP_VELOCITY = -1500f;
        private const float JUMP_CONTROL = 0.14f;
        private const float GRAVITY_ACCELERATION = 1200f;
        private const float MAX_FALL_SPEED = 250f;

        private IAnimationCallbacks _receiver;
        private Vector2 _velocity;
        private float _jumpTime;
        private bool _isColliding;

        private Action<GameTime> _currentAction;

        public override void Initialize()
        {
            _currentAction = Stopped;
        }

        public void SetAnimationCallbacks(IAnimationCallbacks receiver)
        {
            _receiver = receiver;
        }

        public override void Tick(GameTime gameTime)
        {
            _currentAction.Invoke(gameTime);

            Owner.Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Owner.UpdateBBox();
            HandleCollisions();
        }

        private void Stopped(GameTime gameTime) 
        {
            _velocity = Vector2.Zero;

            if (GetMovement() != 0) 
            {
                _receiver.OnMoved();
                _currentAction = Moving;
            }
            else if (ShouldJump())
            {
                _receiver.OnJumped();
                _currentAction = Jumping;
            }
            else if (!IsOnGround())
            {
                _currentAction = Falling;
            }
            else 
            {
                ApplyMovement(gameTime);
            }
        }

        private void Moving(GameTime gameTime) 
        {
            if (GetMovement() == 0)
            {
                _receiver.OnStopped();
                _currentAction = Stopped;
            }
            else if (ShouldJump())
            {
                _receiver.OnJumped();
                _currentAction = Jumping;
            }
            else if (!IsOnGround())
            {
                _currentAction = Falling;
            }
            else 
            {
                ApplyMovement(gameTime);
            }
        }

        private void Jumping(GameTime gameTime)
        {
            ApplyMovement(gameTime);

            _jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if ( _jumpTime <= MAX_JUMP_TIME)
            {
                // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                _velocity.Y = JUMP_VELOCITY * (1.0f - (float)Math.Pow(_jumpTime / MAX_JUMP_TIME, JUMP_CONTROL));
                return;
            }

            _jumpTime = 0.0f;
            _currentAction = Falling;
        }

        private void Falling(GameTime gameTime)
        {
            ApplyGravity(gameTime);
            ApplyMovement(gameTime);

            if (IsOnGround()) 
            {
                _velocity.Y = 0;
                Owner.Position = new Vector2(Owner.Position.X, Owner.Layout.Game.ViewportHeight - (Owner.Bounds.Height * 0.5f));

                _receiver.OnStopped();
                _currentAction = Stopped;
            }
        }

        private float GetMovement()
        {
            float movement = 0;

            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.A))
            {
                movement += -1;
                Owner.IsMirrored = true;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                movement += 1;
                Owner.IsMirrored = false;
            }

            return movement;
        }

        private void ApplyMovement(GameTime gameTime)
        {
            if (_isColliding)
                return;

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float movement = GetMovement();

            _velocity.X += MOVE_ACCELERATION * movement * elapsedTime;
            _velocity.X *= DRAG;
            _velocity.X = MathHelper.Clamp(_velocity.X, -MAX_MOVE_SPEED, MAX_MOVE_SPEED);
        }

        private void ApplyGravity(GameTime gameTime) 
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _velocity.Y = MathHelper.Clamp(_velocity.Y + GRAVITY_ACCELERATION * elapsedTime, -MAX_FALL_SPEED, MAX_FALL_SPEED);
        }

        private bool ShouldJump()
        {
            var keyboardState = Keyboard.GetState();
            return keyboardState.IsKeyDown(Keys.Space);
        }

        private bool IsOnGround()
        {
            return Owner.Bounds.Bottom >= Owner.Layout.Game.ViewportHeight;
        }

        private void HandleCollisions() 
        {
            var grid = Owner.Layout.Grid;
            var collisions = grid.QueryCollisions(Owner);

            _isColliding = collisions.Any();

            foreach (var collision in collisions)
            {
                Vector2 depth = Owner.Bounds.GetIntersectionDepth(collision.Bounds);

                Console.WriteLine(depth.ToString());

                if (depth != Vector2.Zero)
                {
                    float absDepthX = Math.Abs(depth.X);
                    float absDepthY = Math.Abs(depth.Y);

                    if (absDepthY < absDepthX)
                    {
                        Owner.Position = new Vector2(Owner.Position.X, (int)(Owner.Position.Y + depth.Y));
                    }
                    else
                    {
                        Owner.Position = new Vector2((int)(Owner.Position.X + depth.X), Owner.Position.Y);
                    }

                    Owner.UpdateBBox();
                }
            }
        }
    }
}
