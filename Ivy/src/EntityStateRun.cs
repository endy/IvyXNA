using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Ivy
{
    class EntityStateRun : State
    {
        private static EntityStateRun m_instance = null;

        private EntityStateRun()
        {

        }

        public static EntityStateRun Get()
        {
            if (m_instance == null)
            {
                m_instance = new EntityStateRun();
            }
            return m_instance;
        }

        public override void Enter(Entity entity)
        {
            entity.Moving = true;
        }

        public override void HandleMessage(Entity entity, Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.Stand:
                    entity.ChangeState(EntityStateStand.Get());
                    break;
                case MessageType.Jump:
                    entity.ChangeState(EntityStateJump.Get());
                    break;
                case MessageType.Fall:
                    entity.ChangeState(EntityStateFall.Get());
                    break;
                default:
                    // TODO: error!
                    break;
            }
        }
    }
}
