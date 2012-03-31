using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Ivy
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Projectile : Entity
    {
        private AnimatedSprite m_projectileAnim;
        private AnimatedSprite m_explosionAnim;

        private AnimatedSprite m_playingAnim;

        private Entity m_weaponOwner;   /// owner of the weapon that fired this projectile

        private int Damage { get; set; }

        private int lifespan = 10000;
        private int elapsedTime = 0;

        public Projectile(Weapon weapon, Entity weaponOwner, AnimatedSprite projectileAnim, AnimatedSprite explosionAnim)
            : base(IvyGame.Get())
        {
            Position = weapon.Position;
            Direction = weapon.Direction;

            m_weaponOwner = weaponOwner;
            
            Damage = 50;
            
            m_projectileAnim = projectileAnim.Copy() as AnimatedSprite;
            m_projectileAnim.OnAnimEnd += OnAnimEnd;

            m_explosionAnim = explosionAnim.Copy() as AnimatedSprite;
            m_explosionAnim.OnAnimEnd += OnAnimEnd;

            m_playingAnim = m_projectileAnim;

            Movable = true;
            Damagable = false;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            m_playingAnim.Play();

            m_StaticCollisionRect = m_projectileAnim.GetFrameBounds();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime.Milliseconds;

            if (Alive)
            {
                if (elapsedTime > lifespan)
                {
                    Alive = false;
                }
                else
                {
                    Position = new Point(Position.X + (int)Direction.X * 4, Position.Y + (int)Direction.Y * 4);
                }
            }

            m_playingAnim.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle cameraRect = IvyGame.Get().Camera.CameraRect;

            Point projectilePos = new Point();
            projectilePos.X = (int)(((Position.X - cameraRect.X) / (float)cameraRect.Width) * 800); // screen width = 800
            projectilePos.Y = (int)(((Position.Y - cameraRect.Y) / (float)cameraRect.Height) * 600); // screen height = 600


            m_playingAnim.Draw(spriteBatch, projectilePos);
        }

        public override void ReceiveMessage(Message msg)
        {
            if (msg.Type == MessageType.CollideWithEntity)
            {
                EntityCollisionMsg collisionMsg = (EntityCollisionMsg)msg;

                if (collisionMsg.EntityHit != m_weaponOwner)
                {
                    Explode();

                    TakeDamageMsg damageMsg = new TakeDamageMsg(this, collisionMsg.EntityHit, Damage);
                    MessageDispatcher.Get().SendMessage(damageMsg);
                }
            }
            else
            {
                base.ReceiveMessage(msg);
            }
        }

        private void Explode()
        {
            Collidable = false;
            m_projectileAnim.Stop();
            m_playingAnim = m_explosionAnim;
            m_playingAnim.Reset();
            m_playingAnim.Play();

            Direction = Vector2.Zero;

        }

        public void OnAnimEnd(AnimatedSprite anim)
        {
            Alive = false;
        }        
    }
}
