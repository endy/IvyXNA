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
    public class Weapon : Microsoft.Xna.Framework.GameComponent, Ivy.IMessageSender
    {
        private Entity m_owner;     /// owner of the weapon
        private Point m_mountOffset;

        private Vector2 m_direction;

        Texture2D m_weaponMap;

        Rectangle m_projectileRect = new Rectangle(0, 405, 20, 10);
        Rectangle m_explosionRect = new Rectangle(0, 387, 108, 18);

        AnimatedSprite m_projectileAnim;
        AnimatedSprite m_explosionAnim;

        SoundEffect m_fireEffect;

        public Weapon(Entity owner, Point mountOffset, Vector2 direction)
            : base(IvyGame.Get())
        {
            m_owner = owner;
            m_mountOffset = mountOffset;
            m_direction = direction;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            m_weaponMap = Game.Content.Load<Texture2D>("Sprites\\samusMap");
            m_fireEffect = Game.Content.Load<SoundEffect>("Audio\\weapon_fire");

            m_projectileAnim = new AnimatedSprite(m_weaponMap, m_projectileRect, 2, 10f);
            m_projectileAnim.Initialize();

            m_explosionAnim = new AnimatedSprite(m_weaponMap, m_explosionRect, 6, 18f);
            m_explosionAnim.Initialize();
            m_explosionAnim.Loop = false;

            base.Initialize();
        }

        public Point Position
        {
            get
            {
                return new Point(m_owner.Position.X + m_mountOffset.X,
                                 m_owner.Position.Y + m_mountOffset.Y);
            }
        }

        public Vector2 Direction
        {
            get { return new Vector2(m_direction.X, 0); }
            set { m_direction = value; }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            m_direction = m_owner.Direction;
            
            base.Update(gameTime);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            
        }

        public void Fire()
        {
            Projectile p = new Projectile(this, m_owner, m_projectileAnim, m_explosionAnim);
            p.Initialize();
            p.Direction = new Vector2(m_direction.X, 0);

            ChangeZoneMsg msg = new ChangeZoneMsg(this, IvyGame.Get(), p, m_owner.WorldZone.ZoneName, Position, 1);
            MessageDispatcher.Get().SendMessage(msg);

            m_fireEffect.Play();
        }

    }
}