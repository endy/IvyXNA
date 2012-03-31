using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Ivy
{
    public enum MessageType
    {
        // Player Message
        MoveLeft,
        MoveRight,
        Stand,
        Jump,
        Fall,
        Land,
        FireWeapon,

        // Entity Message
        CollideWithEntity,
        ChangeZone,
        TakeDamage,
        RidleyDead,

        // Game Message
        PauseGame,
        PlayGame,
        EndGame,

        // Debug Messages
        ActivateSkree,
    }

    public class Message
    {
        public MessageType Type { get; private set; }
        public IMessageReceiver Receiver { get; private set; }
        public IMessageSender Sender { get; private set; }

        /// when to deliever the message, in milliseconds from time enqueued
        public int SendTime { get; set; }

        public Message(MessageType type, IMessageSender sender, IMessageReceiver receiver)
        {
            Type = type;
            Receiver = receiver;
            Sender = sender;
            SendTime = 0;
        }

        public Message(MessageType type, IMessageSender sender, IMessageReceiver receiver, int sendTime)
        {
            Type = type;
            Receiver = receiver;
            Sender = sender;
            SendTime = sendTime;
        }
    }

    public class EntityCollisionMsg : Message
    {
        public Entity EntityHit { get; private set; }

        public EntityCollisionMsg(IMessageSender sender, IMessageReceiver receiver, Entity entityHit)
            : base(MessageType.CollideWithEntity, sender, receiver)
        {
            EntityHit = entityHit;
        }
    }

    public class ChangeZoneMsg : Message
    {
        public string DestZone { get; private set; }
        public Point DestPosition { get; private set; }
        public Entity Entity { get; private set; }

        public ChangeZoneMsg(
            IMessageSender sender, 
            IMessageReceiver receiver,
            Entity entity,
            string destZone,
            Point destPosition,
            int sendTime)
          : base(MessageType.ChangeZone, sender, receiver, sendTime)
        {
            Entity = entity;
            DestZone = destZone;
            DestPosition = destPosition;
        }
    }

    public class TakeDamageMsg : Message
    {
        public int Damage { get; private set; }

        public TakeDamageMsg(
            IMessageSender sender,
            IMessageReceiver receiver,
            int damage)
          : base(MessageType.TakeDamage, sender, receiver)
        {
            Damage = damage;
        }
    }

}
