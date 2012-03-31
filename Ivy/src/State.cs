using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ivy
{
    abstract public class State
    {
        protected State()
        {

        }

        public virtual void Enter(Entity entity)
        { 
            // do nothing 
        }

        public virtual void Execute(Entity entity)
        {
            // do nothing 
        }

        public virtual void Exit(Entity entity)
        {
            // do nothing 
        }

        public abstract void HandleMessage(Entity entity, Message msg);
    }
}
