using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ivy
{
    class EntityStateStand : State
    {
        private static EntityStateStand m_instance = null;

        private EntityStateStand()
        {

        }

        public static EntityStateStand Get()
        {
            if (m_instance == null)
            {
                m_instance = new EntityStateStand();
            }
            return m_instance;
        }

        public override void Enter(Entity entity)
        {
            entity.Moving = false;
        }

        public override void HandleMessage(Entity entity, Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.MoveLeft:
                case MessageType.MoveRight:
                    entity.ChangeState(EntityStateRun.Get());
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
