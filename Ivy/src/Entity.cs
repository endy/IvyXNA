using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ivy
{
    public class Entity : Microsoft.Xna.Framework.GameComponent, IMessageReceiver, IMessageSender
    {
        // @todo Add 'flying'/'floating' property... ? avoids gravity?

        public WorldZone WorldZone { get; private set; }

        ///@todo Consider private set
        public Point Position { get; set; }
        public Point LastPosition { get; set; }
        public Vector2 Direction { get; set; }
        public bool Moving { get; set; }



        protected AnimGraph m_animGraph;
        protected Vector2 m_speed;
        public Vector2 CurrentSpeed { get; set; }
        protected Rectangle m_StaticCollisionRect;
        protected StateMgr m_entityStateMgr;

        protected Entity m_platform;

        public bool Alive { get; protected set; }
        public bool Collidable { get; protected set; }
        public bool Movable { get; protected set; }
        public bool Damagable { get; protected set; }
        public int Energy { get; protected set; }

        public State CurrentState
        {
            get
            {
                return m_entityStateMgr.CurrentState;
            }
        }

        public Entity(Game game) :
            base(game)
        {
            Movable = false;
            Collidable = true;
            Alive = true;
            Damagable = false;
        }

        public override void Initialize()
        {
            Position = new Point(0, 0);
            LastPosition = Position;

            Direction = new Vector2(1f, 0f);

            Moving = false;

            Energy = 100;

            m_speed = new Vector2(0.2f, 0.3f);
            CurrentSpeed = Vector2.Zero;

            m_entityStateMgr = new StateMgr(this);
            m_entityStateMgr.Initialize();

            // use static collision rect for now
            if (m_animGraph != null)
            {
                m_StaticCollisionRect = m_animGraph.GetCurrentNode().Anim.GetFrameBounds();
            }
            else
            {
                m_StaticCollisionRect = new Rectangle(0, 0, 1, 1);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Energy > 0)
            {
                if (m_platform != null)
                {
                    Vector2 p = GetPositionAtTime(gameTime.ElapsedGameTime.Milliseconds);

                    Rectangle cRect = CollisionRect();
                    cRect.X = (int)p.X;
                    cRect.Y = (int)(p.Y + WorldZone.GravityConstant.Y);

                    if (m_platform.CollisionRect().Intersects(cRect) == false)
                    {
                        MessageDispatcher.Get().SendMessage(new Message(MessageType.Fall, this, this));
                    }
                }

                m_entityStateMgr.Update();
            }

            if (Energy <= 0)
            {
                Alive = false;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }

        public virtual void Draw3D()
        {
            Rectangle r = CollisionRect();
            Rectangle newRect = new Rectangle(0, 0, (int)(r.Width / 256f * 800f), (int)(r.Height / 192f * 600f));

            Box.Get().Draw(Position, newRect);
        }

        public Rectangle CollisionRect()
        {
            Rectangle rect = m_StaticCollisionRect;

            return new Rectangle(Position.X, Position.Y, rect.Width, rect.Height);
        }

        public virtual void ReceiveMessage(Message msg)
        {
            m_entityStateMgr.HandleMessage(msg);


            if (msg.Type == MessageType.CollideWithEntity)
            {
                HandleCollisionWithEntity((EntityCollisionMsg)msg);
            }
            else if ((msg.Type == MessageType.TakeDamage) && Damagable)
            {
                Energy = Math.Max(Energy - ((TakeDamageMsg)msg).Damage, 0);
            }

            if (msg.Type == MessageType.MoveLeft)
            {
                Direction = new Vector2(-1f, Direction.Y);
                CurrentSpeed = new Vector2(m_speed.X, CurrentSpeed.Y);
            }
            else if (msg.Type == MessageType.MoveRight)
            {
                Direction = new Vector2(1f, Direction.Y);
                CurrentSpeed = new Vector2(m_speed.X, CurrentSpeed.Y);
            }
            else if (msg.Type == MessageType.Stand)
            {
                CurrentSpeed = new Vector2(0f, CurrentSpeed.Y);
            }
            else if (msg.Type == MessageType.Jump)
            {
                Direction = new Vector2(Direction.X, -1f);
                CurrentSpeed = new Vector2(CurrentSpeed.X, m_speed.Y);
                m_platform = null;
            }
            else if (msg.Type == MessageType.Fall)
            {
                Direction = new Vector2(Direction.X, 1f);
                CurrentSpeed = new Vector2(CurrentSpeed.X, m_speed.Y);
                m_platform = null;
            }
            else if (msg.Type == MessageType.Land)
            {
                CurrentSpeed = new Vector2(CurrentSpeed.X, 0f);
            }

            // if moving message...update direction?
            // or should that be done on state change??
            // and do we query input mgr or do we do it some other way

        }
        public void ChangeState(State nextState)
        {
            m_entityStateMgr.ChangeState(nextState);

            // TODO: update entity properties based on new state
            //       or should this be in a seperate entity method that the state calls?
            //       does it matter?  -- maybe it does -- so the state has more 'control' over the entity
        }

        public virtual Vector2 GetPositionAtTime(int elapsedTimeMS)
        {
            int dx = (int)(CurrentSpeed.X * Direction.X * elapsedTimeMS);
            int dy = (int)(CurrentSpeed.Y * Direction.Y * elapsedTimeMS);

            // restrict movement to within world zone
            int x = Math.Max(WorldZone.Bounds.Left, Math.Min(WorldZone.Bounds.Right - CollisionRect().Width, Position.X + dx));
            int y = Math.Max(WorldZone.Bounds.Top, Math.Min(WorldZone.Bounds.Bottom - CollisionRect().Height, Position.Y + dy));

            return new Vector2(x, y);
        }

        public void ChangeZone(WorldZone zone, Point position)
        {
            ///@todo need 'ExitZone' method?
            WorldZone = zone;

            LastPosition = Position;
            Position = position;
        }

        private void HandleCollisionWithEntity(EntityCollisionMsg msg)
        {
            Rectangle collisionRect = CollisionRect();
            Rectangle footRect =
                new Rectangle(collisionRect.X, collisionRect.Y + collisionRect.Height - 1, collisionRect.Width, 3);
            Rectangle headRect = new Rectangle(footRect.X, collisionRect.Y, footRect.Width, 3);

            if (msg.EntityHit.Movable == false)
            {
                int dx = Position.X;
                int dy = Position.Y;

                Rectangle entRect = msg.EntityHit.CollisionRect();

                bool footHit = footRect.Intersects(entRect);
                bool headHit = headRect.Intersects(entRect);

                if (footHit != true && headHit != true)
                {
                    if (Position.X - LastPosition.X > 0)
                    {
                        dx = entRect.X - collisionRect.Width;
                    }
                    else if (Position.X - LastPosition.X < 0)
                    {
                        dx = entRect.X + entRect.Width;
                    }
                }
                else if (footHit == true)
                {
                    int snapY = msg.EntityHit.Position.Y - CollisionRect().Height;

                    if (Math.Abs(dy - snapY) < 10)
                    {
                        dy = snapY;
                        MessageDispatcher.Get().SendMessage(new Message(MessageType.Land, this, this));

                        // standing on this platform
                        m_platform = msg.EntityHit;
                    }
                    else
                    {
                        if (Position.X - LastPosition.X > 0)
                        {
                            dx = entRect.X - collisionRect.Width;
                        }
                        else if (Position.X - LastPosition.X < 0)
                        {
                            dx = entRect.X + entRect.Width;
                        }
                    }
                }
                else if (headHit == true)
                {
                    int snapY = msg.EntityHit.Position.Y + entRect.Height;

                    if (Math.Abs(dy - snapY) < 10)
                    {
                        dy = snapY;
                    }
                    else
                    {
                        if (Position.X - LastPosition.X > 0)
                        {
                            dx = entRect.X - collisionRect.Width;
                        }
                        else if (Position.X - LastPosition.X < 0)
                        {
                            dx = entRect.X + entRect.Width;
                        }
                    }
                }

                Position = new Point(dx, dy);
            }
        }

    }
}
