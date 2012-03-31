using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Ivy
{
    public class Platform : Entity
    {
        public Platform()
            : base(IvyGame.Get())
        {
            Movable = false;
        }

        public override void Initialize()
        {
            base.Initialize();

            Moving = false;
        }

        public void SetSize(int width, int height)
        {
            m_StaticCollisionRect = new Rectangle(0, 0, width, height);
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
