using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ivy
{
    class EntityStateJump : State
    {
        private static EntityStateJump m_instance = null;

        private EntityStateJump()
        {

        }

        static public EntityStateJump Get()
        {
            if (m_instance == null)
            {
                m_instance = new EntityStateJump();
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
                //case MessageType.CollideWithEnv:
                //    Message newMsg = new Message(MessageType.Fall, entity, entity);
                //    MessageDispatcher.Get().SendMessage(newMsg);
                    //entity.ChangeState(EntityStateFall.Get());
                //    break;
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
