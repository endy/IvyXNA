using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace Quicksilver
{
    interface IMovement
    {
        void MoveEntity(Entity entity);
    }

    class SimpleEntityMovement : IMovement
    {
        public void MoveEntity(Entity entity)
        {
            entity.Position = entity.Position + entity.Direction * entity.Speed;
        }
    }

    class ZigZagMovement : IMovement
    {
        private int tick;
        static public int ZigTick = 20;

        public ZigZagMovement()
        {
            tick = ZigTick;
        }

        public void MoveEntity(Entity entity)
        {
            if (tick <= 0)
            {
                entity.Direction = entity.Direction * new Vector2(1,-1);
                tick = ZigTick;
            }
            else
            {
                tick -= 1;
            }

            entity.Position = entity.Position + entity.Direction * entity.Speed;
        }
    }

    class CircleMovement : IMovement
    {
        private int tick;
        static public int CircleSpeed = 20;

        public CircleMovement()
        {
            tick = CircleSpeed;
        }

        public void MoveEntity(Entity entity)
        {
            if (tick <= 0)
            {
                entity.Direction = entity.Direction * new Vector2(1, -1);
                tick = CircleSpeed;
            }
            else
            {
                tick -= 1;
            }

            entity.Position = entity.Position + entity.Direction * entity.Speed;
        }
    }

    class AttackMovement : IMovement
    {
        public void MoveEntity(Entity entity)
        {
            entity.Direction = Quicksilver.Player.Position - entity.Position;
            entity.Direction.Normalize();
            entity.Position = entity.Position + entity.Direction * entity.Speed;
        }
    }

    class Collisions
    {
        public delegate void HandleEntityCollision(Entity self, Entity other);

        public static void DamageEntity(Entity self, Entity other)
        {
            other.ModifyEnergy(-10);
        }
    }

    class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public Vector2 Speed { get; set; }

        public int Energy { get; set; }
        public bool isAlive { get; set; }
        public bool takesDamage { get; set; }

        public Rectangle CollisionRect { get; private set; }

        public Texture2D Sprite { get; private set; }
        public Color Mask { get; set; }

        public IMovement Movement = null;
        public Collisions.HandleEntityCollision handleCollision = null;

        public Entity(Texture2D inSprite)
        {
            isAlive = true;
            takesDamage = false;
            Energy = 100;
            Position = new Vector2();
            Direction = new Vector2();
            Speed = new Vector2(1, 1);
            Sprite = inSprite;
            Mask = Color.White;

            CollisionRect = new Rectangle(0, 0, Sprite.Bounds.Width, Sprite.Bounds.Height);
        }

        public void Update()
        {
            if (Energy <= 0)
            {
                isAlive = false;
            }
            else if ((Quicksilver.graphics.GraphicsDevice.Viewport.Bounds.Contains(CollisionRect) == false) &&
                     (Quicksilver.graphics.GraphicsDevice.Viewport.Bounds.Intersects(CollisionRect) == false))
            {
                isAlive = false;
            }

            if (isAlive)
            {
                if (Movement != null)
                {
                    Movement.MoveEntity(this);
                }
                else
                {
                    Position = Position + Direction * Speed;
                }

                Rectangle cRect = CollisionRect;
                cRect.Offset((int)Position.X - cRect.Left, (int)Position.Y - cRect.Top);
                CollisionRect = cRect;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite,
                             new Rectangle((int)Position.X,
                                           (int)Position.Y,
                                           CollisionRect.Width,
                                           CollisionRect.Height),
                             Mask);
        }

        public void OnCollision(Entity other)
        {
            if (handleCollision != null)
            {
                handleCollision(this, other);
            }
        }

        public void ModifyEnergy(int energyAmount)
        {
            Energy += energyAmount;
        }
    };
}
