using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Ivy
{
    public interface IMessageReceiver
    {
        void ReceiveMessage(Message msg);
    }

    public interface IMessageSender
    {
        // Empty
    }

    public class MessageDispatcher
    {
        private static MessageDispatcher m_instance;

        private List<Message> m_messageList;

        private MessageDispatcher()
        {

        }

        public static MessageDispatcher Get()
        {
            if (m_instance == null)
            {
                m_instance = new MessageDispatcher();
                m_instance.Initialize();
            }

            return m_instance;
        }

        private void Initialize()
        {
            m_messageList = new List<Message>();
        }

        public void SendMessage(Message msg)
        {
            if (msg.SendTime == 0)
            {
                msg.Receiver.ReceiveMessage(msg);
            }
            else
            {
                m_messageList.Add(msg);
            }
        }

        public void Update(GameTime gameTime)
        {
            List<Message> sendList = new List<Message>();

            // send messages when the time is right!
            foreach (Message msg in m_messageList)
            {
                msg.SendTime = Math.Max(0, (msg.SendTime - gameTime.ElapsedGameTime.Milliseconds));

                if (msg.SendTime == 0)
                {
                    sendList.Add(msg);
                }
            }

            foreach (Message msg in sendList)
            {
                m_messageList.Remove(msg);
                msg.Receiver.ReceiveMessage(msg);
            }
        }
    }
}
