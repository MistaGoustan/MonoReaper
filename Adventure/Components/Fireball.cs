﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Engine;
using Engine.Collision;
using Engine.Graphics;

using static Adventure.Constants;

namespace Adventure.Components
{
    public sealed class Fireball : Component
    {
        private Body _body;
        private Vector2 _velocity;

        public Fireball(Vector2 velocity)
        {
            _velocity = velocity;
        }

        public override void OnLoad(ContentManager content)
        {
            Entity.AddComponent(_body = new Body(4, 4, EntityLayers.PlayerProjectile));
            Entity.AddComponent(new Particles(SharedContent.Graphics.Fire, new Rectangle(8, 8, 8, 8))
            {
                MaxParticles = 100,
                Velocity = new Vector2(25f),
                AngularVelocity = 10f,
                MinColor = Color.White,
                MaxColor = Color.White * 0.1f,
                MaxTime = 0.25f,
                ZOrder = 5
            });
        }

        public override void OnSpawn()
        {
            SharedContent.Sounds.Shoot.Play();
        }

        public override void OnUpdate(GameTime gameTime)
        {
            _body.MoveAndCollide(ref _velocity, EntityLayers.Enemy | EntityLayers.Solid, HandleCollision);
        }

        private Vector2 HandleCollision(Collision collision)
        {
            Level.Destroy(Entity);
            Level.Spawn(new Explosion(), Entity.Position);

            if (collision.Box.Entity.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.Damage(1);

                if (damageable.Flammable)
                {
                    collision.Box.Entity.AddComponent(new OnFire());
                }
            }

            return collision.Ignore();
        }
    }
}
