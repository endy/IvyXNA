using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ivy
{
    public class StateMgr
    {
        Entity m_owner;     /// Entity that owns this state machine
                             
        public State CurrentState { get; private set; }
        State m_previousState;

        public StateMgr(Entity owner)
        {
            m_owner = owner;
        }

        public void Initialize()
        {
            CurrentState = EntityStateStand.Get();
        }

        public void ChangeState(State nextState)
        {
            if (CurrentState != null)
            {
                CurrentState.Exit(m_owner);
                m_previousState = CurrentState;
                CurrentState = nextState;
                CurrentState.Enter(m_owner);
            }
        }

        public void Update()
        {
            CurrentState.Execute(m_owner);
        }

        public void HandleMessage(Message msg)
        {
            CurrentState.HandleMessage(m_owner, msg);
        }
    }
}
