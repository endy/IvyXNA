using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OgmoXNA;
using OgmoXNA.Layers;
using OgmoXNA.Values;

namespace Ivy
{
    public class WorldZone : IMessageSender, IMessageReceiver
    {
        
        List<Entity> m_entities;
        Texture2D m_background;

        public Rectangle Bounds { get; private set; }
        public string ZoneName { get; private set; }
        public Vector2 GravityConstant { get; private set; }

        // refactor these properties to a derived class
        Texture2D m_escapeBackground;
        public bool EscapeMode { get; private set; }
        
        public WorldZone(string zoneName)
        {
            EscapeMode = false;

            ZoneName = zoneName;

            m_entities = new List<Entity>();
            GravityConstant = new Vector2(0, 1);

            OgmoLevel level = IvyGame.Get().Content.Load<OgmoLevel>(ZoneName);
            string workingDir = level.Project.Settings.WorkingDirectory + "\\";

            Bounds = new Rectangle(0, 0, level.Width, level.Height);

            m_background = level.GetLayer<OgmoObjectLayer>("bg").Objects[0].Texture;

            OgmoObjectLayer escapeBgLayer = level.GetLayer<OgmoObjectLayer>("escape_bg");
            if (escapeBgLayer != null)
            {
                m_escapeBackground = escapeBgLayer.Objects[0].Texture;
            }

            foreach (Rectangle rect in level.GetLayer<OgmoGridLayer>("platforms").RectangleData)
            {
                Platform p = new Platform();
                p.Initialize();
                p.SetSize(rect.Width, rect.Height);
                AddEntity(p, rect.Location);
            }

            foreach (OgmoObject portalObj in level.GetLayer<OgmoObjectLayer>("portals").Objects)
            {
                OgmoStringValue destZone = portalObj.GetValue<OgmoStringValue>("destZone");
                OgmoIntegerValue destX = portalObj.GetValue<OgmoIntegerValue>("destX");
                OgmoIntegerValue destY = portalObj.GetValue<OgmoIntegerValue>("destY");

                Point portalPosition = new Point((int)portalObj.Position.X, (int)portalObj.Position.Y);

                Rectangle portalBounds = 
                    new Rectangle(0, 0, portalObj.Width, portalObj.Height);

                ///@todo figure out how to prepend "levels/"
                ZonePortal portal = new ZonePortal(
                    @"levels/" + destZone.Value,
                    new Point(destX.Value, destY.Value),
                    portalBounds);

                portal.Initialize();

                AddEntity(portal, portalPosition);
            }

            OgmoObjectLayer baddieLayer =  level.GetLayer<OgmoObjectLayer>("baddies");

            if (baddieLayer != null)
            {
                foreach (OgmoObject baddie in baddieLayer.Objects)
                {
                    Point position = new Point((int)baddie.Position.X, (int)baddie.Position.Y);

                    Entity entityBaddie = null;

                    if (baddie.Name == "ridley")
                    {
                        entityBaddie = new Ridley(baddie.Texture);
                    }
                    else if (baddie.Name == "fireflea")
                    {
                        entityBaddie = new Fireflea(baddie.Texture);
                    }

                    entityBaddie.Initialize();
                    AddEntity(entityBaddie, position);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            IvyGame.Get().ConsoleStr += "Entities: " + m_entities.Count + "\n";

            // Update misc zone resources (background positions, etc)

            List<Entity> deadEntities = new List<Entity>();

            // Update alive entities, remove dead ones
            foreach (Entity e in m_entities)
            {
                if (e.Alive)
                {
                    e.Update(gameTime);
                }

                if (e.Alive == false)
                {
                    deadEntities.Add(e);
                }
            }

            foreach (Entity e in deadEntities)
            {
                RemoveEntity(e);
            }

            ///@todo Migrate this back into Entity
            // update entity positions
            foreach (Entity e in m_entities)
            {
                if (e.Movable)
                {
                    e.LastPosition = e.Position;

                    int elapsedTimeMS = gameTime.ElapsedGameTime.Milliseconds;
                    Vector2 newPos = e.GetPositionAtTime(elapsedTimeMS);

                    ///@todo Determine Vector2 vs Point
                    e.Position = new Point((int)newPos.X, (int)newPos.Y);
                }
            }

            List<Entity> entityListA = new List<Entity>(m_entities);
            List<Entity> entityListB = new List<Entity>(m_entities);

            foreach (Entity a in entityListA)
            {
                
                if (a.Collidable == false)
                {
                    // Not collidable, ignore in list A and B
                    entityListB.Remove(a);
                    continue;
                }

                if (a.Movable == false)
                {
                    continue;
                }

                entityListB.Remove(a);

                foreach (Entity b in entityListB)
                {
                    if (b.Collidable)
                    {
                        Rectangle aRect = a.CollisionRect();
                        Rectangle bRect = b.CollisionRect();

                        if (aRect.Intersects(bRect))
                        {
                            MessageDispatcher.Get().SendMessage(new EntityCollisionMsg(this, a, b));
                            MessageDispatcher.Get().SendMessage(new EntityCollisionMsg(this, b, a));
                        }
                    }
                }
            }            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw Background
            Rectangle cameraRect = IvyGame.Get().Camera.CameraRect;     // camera rect in world space

            Rectangle srcRect = new Rectangle(Bounds.Left + cameraRect.X, Bounds.Top + cameraRect.Y, cameraRect.Width, cameraRect.Height);
            Rectangle dstRect = new Rectangle(0, 0, 800, 600);

            if (EscapeMode == false || m_escapeBackground == null)
            {
                spriteBatch.Draw(m_background, dstRect, srcRect, Color.White);
            }
            else
            {
                spriteBatch.Draw(m_escapeBackground, dstRect, srcRect, Color.White);
            }

            foreach (Entity e in m_entities)
            {
                e.Draw(spriteBatch);
            }
        }

        public void Draw3D()
        {
            foreach (Entity e in m_entities)
            {
                e.Draw3D();
            }
        }

        private void AddEntity(Entity entity, Point position)
        {
            if (m_entities.Contains(entity) == false)
            {
                m_entities.Add(entity);
                entity.ChangeZone(this, position);
            }
        }

        private void RemoveEntity(Entity entity)
        {
            if (m_entities.Contains(entity) == true)
            {
                m_entities.Remove(entity);
            }
        }

        public virtual void ReceiveMessage(Message msg)
        {
            if (msg.Type == MessageType.ChangeZone)
            {
                ChangeZoneMsg czMsg = (ChangeZoneMsg)msg;
                Entity entity = czMsg.Entity;
                if (czMsg.DestZone == ZoneName)
                {
                    AddEntity(entity, czMsg.DestPosition);
                }
                else
                {
                    RemoveEntity(entity);
                }
            }
        }

        public void SetEscapeMode(bool escapeModeOn)
        {
            EscapeMode = escapeModeOn;
        }
    }
}
