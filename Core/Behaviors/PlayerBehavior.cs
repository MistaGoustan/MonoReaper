﻿using ItsGood;
using ItsGood.Builtins;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Core
{
    public class PlayerBehavior : Behavior
    {
        private AnimationBehavior _animationBehavior;
        private PlatformerBehavior _platformerBehavior;
        private KeyboardState _previousKeyState;

        private Action<float> _currentState;

        public override void OnOwnerCreated()
        {
            _animationBehavior = Owner.GetBehavior<AnimationBehavior>();
            _platformerBehavior = Owner.GetBehavior<PlatformerBehavior>();

            GoToIdle();
        }

        public override void Tick(GameTime gameTime)
        {
            _currentState.Invoke((float)gameTime.ElapsedGameTime.TotalSeconds);

            _previousKeyState = Keyboard.GetState();

            if (Owner.Position.X > 200) 
            {
                Owner.Layout.View.Zoom = MathHelper.SmoothStep(Owner.Layout.View.Zoom, 0.8f, 0.1f);
                Owner.Layout.View.Position = new Vector2(MathHelper.SmoothStep(Owner.Layout.View.Position.X, 400, 0.1f), Owner.Layout.View.Position.Y);
            }
            else
            {
                Owner.Layout.View.Zoom = MathHelper.SmoothStep(Owner.Layout.View.Zoom, 1f, 0.1f);
                Owner.Layout.View.Position = new Vector2(MathHelper.SmoothStep(Owner.Layout.View.Position.X, 100, 0.1f), Owner.Layout.View.Position.Y);
            }

            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Down)) 
            {
                Owner.Layout.View.Zoom += 0.1f;
            }
            else if (keyboardState.IsKeyDown(Keys.Up))
            {
                Owner.Layout.View.Zoom -= 0.1f;
            }
        }

        private void GoToIdle() 
        {
            _animationBehavior.Play("Idle");
            _currentState = Idle;
        }

        private void Idle(float elapesedTime) 
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.D))
            {
                GoToMove();
            }
            else if (_platformerBehavior.IsOnGround() && keyboardState.IsKeyDown(Keys.Space) && _previousKeyState.IsKeyUp(Keys.Space))
            {
                GoToJump();
            }
            else if (_platformerBehavior.IsFalling())
            {
                GoToFall();
            }
            else if (keyboardState.IsKeyDown(Keys.Left)) 
            {
                GoToAttack();
            }
        }

        private void GoToMove()
        {
            _animationBehavior.Play("Run");
            _currentState = Move;
        }

        private void Move(float elapesedTime)
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

            _platformerBehavior.Move(movement);

            if (movement == 0f) 
            {
                GoToIdle();
            }
            if (_platformerBehavior.IsOnGround() && keyboardState.IsKeyDown(Keys.Space) && _previousKeyState.IsKeyUp(Keys.Space))
            {
                GoToJump();
            }
            else if (keyboardState.IsKeyDown(Keys.Left))
            {
                GoToAttack();
            }
        }

        private void GoToJump()
        {
            _platformerBehavior.Jump();
            _animationBehavior.Play("Idle");
            _currentState = Jump;
        }

        private void Jump(float elapesedTime)
        {
            var keyboardState = Keyboard.GetState();
            float movement = 0;

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
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                _platformerBehavior.Jump();
            }

            _platformerBehavior.Move(movement);

            if (_platformerBehavior.IsFalling())
            {
                GoToFall();
            }
            else if (keyboardState.IsKeyDown(Keys.Left))
            {
                _platformerBehavior.GravityAcceleration = 0;
                _platformerBehavior.Velocity = Vector2.Zero;
                GoToAttack();
            }
        }

        private void GoToFall()
        {
            _animationBehavior.Play("Jump");
            _currentState = Fall;
        }

        private void Fall(float elapesedTime)
        {
            var keyboardState = Keyboard.GetState();

            float movement = 0;

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

            _platformerBehavior.Move(movement);

            if (_platformerBehavior.IsOnGround())
            {
                if (_platformerBehavior.IsMoving())
                {
                    GoToMove();
                }
                else
                {
                    GoToIdle();
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Left))
            {
                _platformerBehavior.GravityAcceleration = 0;
                _platformerBehavior.Velocity = Vector2.Zero;

                GoToAttack();
            }
        }

        private void GoToAttack() 
        {
            _hasCheckedForHits = false;
            _animationBehavior.Play("Attack");
            _currentState = Attack;
        }

        private bool _hasCheckedForHits;

        private void Attack(float elapsedTime)
        {
            if (_animationBehavior.CurrentFrame == 2 && !_hasCheckedForHits) 
            {
                var bounds = new Rectangle(
                    Owner.IsMirrored ? Owner.Bounds.Left - 16 : Owner.Bounds.Right + 16,
                    (int)Math.Round(Owner.Position.Y - 16),
                    16, 16);

                var overlaps = Owner.Layout.TestOverlap(bounds);

                foreach (var overlap in overlaps) 
                {
                    if (overlap == Owner)
                        continue;

                    var damageable = overlap.GetBehavior<DamageableBehavior>();

                    if (damageable == null)
                        continue;

                    damageable.Damage(1);
                }

                _hasCheckedForHits = true;
            }
            else if (_animationBehavior.IsFinished) 
            {
                _platformerBehavior.GravityAcceleration = 1400f;
                GoToIdle();
            }
        }
    }
}