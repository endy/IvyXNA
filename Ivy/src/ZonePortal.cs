using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Ivy
{
    public class ZonePortal : Entity
    {
        public string DestZone { get; private set; }
        public Point DestPosition { get; private set; }
        public Rectangle Bounds { get; private set; }

        public ZonePortal(string destZone, Point destPosition, Rectangle bounds) :
            base(IvyGame.Get())
        {
            DestZone = destZone;
            DestPosition = destPosition;
            Bounds = bounds;
        }

        public override void Initialize()
        {
            base.Initialize();

            m_StaticCollisionRect = Bounds;
        }

        public override void ReceiveMessage(Message msg)
        {
            if (msg.Type == MessageType.CollideWithEntity)
            {
                EntityCollisionMsg collisionMsg = (EntityCollisionMsg) msg;

                ChangeZoneMsg changeRoomMsg = 
                    new ChangeZoneMsg(this, IvyGame.Get(), collisionMsg.EntityHit, DestZone, DestPosition, 0);

                MessageDispatcher.Get().SendMessage(changeRoomMsg);
            }
        }
    }
}
