﻿using ItsGood;
using ItsGood.Builtins;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Core
{
    public class PlayerBehavior : Behavior
    {
        private AnimatedSpriteBehavior _animationBehavior;
        private PlatformerBehavior _platformerBehavior;
        private KeyboardState _previousKeyState;

        private Action<float> _currentState;

        public override void OnOwnerCreated()
        {
            _animationBehavior = Owner.GetBehavior<AnimatedSpriteBehavior>();
            _platformerBehavior = Owner.GetBehavior<PlatformerBehavior>();

            GoToIdle();
        }

        public override void Tick(GameTime gameTime)
        {
            _currentState.Invoke((float)gameTime.ElapsedGameTime.TotalSeconds);

            _previousKeyState = Keyboard.GetState();
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
            if (_platformerBehavior.IsOnGround() && keyboardState.IsKeyDown(Keys.Space) && _previousKeyState.IsKeyUp(Keys.Space))
            {
                GoToJump();
            }

            _platformerBehavior.Move(movement);

            if (movement == 0f) 
            {
                GoToIdle();
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
            if (_platformerBehavior.IsFalling()) 
            {
                GoToFall();
                return;
            }

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
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                _platformerBehavior.Jump();
            }

            _platformerBehavior.Move(movement);
        }

        private void GoToFall()
        {
            _animationBehavior.Play("Jump");
            _currentState = Fall;
        }

        private void Fall(float elapesedTime)
        {
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
        }

        private void GoToAttack() 
        {
            _animationBehavior.Play("Attack");
            _currentState = Attack;
        }

        private void Attack(float elapsedTime)
        {
            if (_animationBehavior.CurrentFrame == 2) 
            {
                var bounds = new Rectangle(
                    (int)Math.Round(Owner.Position.X - 16),
                    (int)Math.Round(Owner.Position.Y - 16),
                    32, 32);

                var overlaps = Owner.Layout.TestOverlap(bounds);

                foreach (var overlap in overlaps) 
                {
                    if (overlap == Owner)
                        continue;

                    overlap.Color = Color.Red;
                }
            }
            else if (_animationBehavior.IsFinished) 
            {
                GoToIdle();
            }
        }
    }
}
