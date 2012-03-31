using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ivy
{
    class Fireflea : Entity
    {
        private Texture2D m_sprite;

        private float m_degree;
        private int m_elapsedTime;

        public Fireflea(Texture2D sprite) :
            base(IvyGame.Get())
        {
            m_sprite = sprite;
        }

        public override void Initialize()
        {
            base.Initialize();

            Movable = true;
            Damagable = true;

            m_StaticCollisionRect = m_sprite.Bounds;

            Random r = new Random();
            Vector2 pos = GetPositionAtTime(r.Next(0, 1000));

            Position = new Point((int)pos.X, (int)pos.Y);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override Vector2 GetPositionAtTime(int elapsedTimeMS)
        {
            float dx = 0;
            float dy = 0;

            m_elapsedTime += elapsedTimeMS;

            if (m_elapsedTime > 50)
            {
                m_degree += elapsedTimeMS / 60.0f;

                dx = (Math.Sin(m_degree) > 0) ? 2.0f : -2.0f;
                dy = (Math.Cos(m_degree) > 0) ? 2.0f : -2.0f;

                m_elapsedTime = 0;
            }


            Vector2 newPos = new Vector2(Position.X + dx, Position.Y + dy);

            return newPos;

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

            Color tint = new Color(Energy / 100.0f, Energy / 100.0f, Energy / 100.0f);

            spriteBatch.Draw(m_sprite, dstRect, srcRect, tint);            
        }

        public override void ReceiveMessage(Message msg)
        {
            if (msg.Type == MessageType.CollideWithEntity)
            {
                // Ignore this message!
            }
            else
            {
                base.ReceiveMessage(msg);
            }
        }

    }
}
