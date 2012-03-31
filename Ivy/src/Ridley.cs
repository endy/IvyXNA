using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ivy
{
    class Ridley : Entity
    {
        private Texture2D m_sprite;

        private int Damage { get; set; }

        private int m_elapsedTime;
        private int m_update;

        public Ridley(Texture2D sprite) : 
            base(IvyGame.Get()) 
        {
            m_sprite = sprite;
        }

        public override void Initialize()
        {
            base.Initialize();

            Movable = true;
            m_StaticCollisionRect = m_sprite.Bounds;
            Damage = 10;
            Damagable = true;

            Energy = 1000;

            m_elapsedTime = 0;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);

            if (Energy <= 0)
            {
                MessageDispatcher.Get().SendMessage(new Message(MessageType.RidleyDead, this, IvyGame.Get()));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 roomPos = new Vector2(Position.X, Position.Y);
            Vector2 screenPos = IvyGame.Get().Camera.GetScreenPointForRoomPoint(roomPos);

            Point screenPosPoint = new Point((int)screenPos.X, (int)screenPos.Y);

            Rectangle srcRect = m_sprite.Bounds;

            // TODO: Refactor scaling to camera 
            Rectangle dstRect = new Rectangle(screenPosPoint.X,
                                              screenPosPoint.Y,
                                              (int)(srcRect.Width / 256f * 800f * 1),
                                              (int)(srcRect.Height / 192f * 600f * 1));

            Color tint = new Color(1.0f, (float)Energy / 1000.0f, (float)Energy / 1000.0f);

            spriteBatch.Draw(m_sprite, dstRect, srcRect, tint);

        }

        public override Vector2 GetPositionAtTime(int elapsedTimeMS)
        {
            float dx = 0;
            float dy = 0;

            m_elapsedTime += elapsedTimeMS;
            m_update++;

            if (m_update > 5)
            {
                dy = (Math.Cos(m_elapsedTime / 2.0) > 0) ? 2.0f : -2.0f;
                m_update = 0;
            }

            Vector2 newPos = new Vector2(Position.X + dx, Position.Y + dy);

            return newPos;

        }

        public override void ReceiveMessage(Message msg)
        {
            if (msg.Type == MessageType.CollideWithEntity)
            {
                EntityCollisionMsg entMsg = (EntityCollisionMsg) msg;

                TakeDamageMsg takeDamageMsg = new TakeDamageMsg(this, entMsg.EntityHit, Damage);
                MessageDispatcher.Get().SendMessage(takeDamageMsg);
            }
            else
            {
                base.ReceiveMessage(msg);
            }
        }
    }
}
