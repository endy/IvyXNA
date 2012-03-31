using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ivy
{
    public class Camera2D
    {
        private Rectangle m_zoneBounds;
        public Rectangle CameraBounds { get; private set; } // in room coordinates
        public Rectangle ScreenRect { get; private set; }   // in screen coordinates

        // calculated camera rect
        public Rectangle CameraRect { get; private set; }   // in room coordinates

        private Vector2 m_maxSpeed;

        private Entity m_targetEntity;

        public Camera2D()
        {
            Rectangle defaultRect = new Rectangle(0, 0, 800, 600);
            m_zoneBounds = defaultRect;
            CameraBounds = defaultRect;
            ScreenRect = defaultRect;
            CameraRect = defaultRect;
        }

        public void Initialize(Rectangle roomBounds, Rectangle cameraBounds, Rectangle screenRect)
        {
            m_zoneBounds = roomBounds;
            CameraBounds = cameraBounds;
            ScreenRect = screenRect;

            CameraRect = new Rectangle(0, 0, cameraBounds.Width, cameraBounds.Height);

            m_maxSpeed = new Vector2(1.3f, 1.3f);
        }

        public void SetZoneBounds(Rectangle zoneBounds)
        {
            m_zoneBounds = zoneBounds;
        }

        public void SetTarget(Entity target)
        {
            m_targetEntity = target;


            Vector2 idealCameraCenter = new Vector2(m_targetEntity.Position.X + (1 / 5f * ((float)CameraBounds.Width * m_targetEntity.Direction.X)),
                                        m_targetEntity.Position.Y + (1 / 10f * CameraBounds.Height * m_targetEntity.Direction.Y));

            int x = (int)idealCameraCenter.X - CameraBounds.Center.X;
            int y = (int)idealCameraCenter.Y - CameraBounds.Center.Y;
            int width = CameraBounds.Width;
            int height = CameraBounds.Height;

            x = Math.Max(x, m_zoneBounds.X);
            x = Math.Min(x + width, m_zoneBounds.Right) - width;
            y = Math.Max(y, m_zoneBounds.Y);
            y = Math.Min(y + height, m_zoneBounds.Bottom) - height;

            CameraRect = new Rectangle(x, y, width, height);
        }

        public Vector2 GetRoomPointForScreenPoint(Vector2 screenPoint)
        {
            Vector2 roomPoint = Vector2.Zero;

            if ((ScreenRect.Left <= screenPoint.X) &&
                (screenPoint.X < ScreenRect.Right) &&
                (ScreenRect.Top <= screenPoint.Y) &&
                (screenPoint.Y < ScreenRect.Bottom))
            {
                roomPoint.X = CameraRect.X + ((screenPoint.X / (float)ScreenRect.Width) * CameraRect.Width);
                roomPoint.Y = CameraRect.Y + ((screenPoint.Y / (float)ScreenRect.Height) * CameraRect.Height);
            }
            else
            {
                // TODO: probably an error
            }

            return roomPoint;
        }

        public Vector2 GetScreenPointForRoomPoint(Vector2 roomPoint)
        {
            Vector2 screenPoint = Vector2.Zero;

            if ((m_zoneBounds.Left <= roomPoint.X) &&
                (roomPoint.X < m_zoneBounds.Right) &&
                (m_zoneBounds.Top <= roomPoint.Y) &&
                (roomPoint.Y < m_zoneBounds.Bottom))
            {
                screenPoint.X = ((roomPoint.X - CameraRect.X) / (float)CameraRect.Width) * ScreenRect.Width;
                screenPoint.Y = ((roomPoint.Y - CameraRect.Y) / (float)CameraRect.Height) * ScreenRect.Height;
            }
            else
            {
                // TODO: probably an error
            }

            return screenPoint;
        }

        private Vector2 GetMaxTravelDist(GameTime gameTime)
        {
            return new Vector2((m_maxSpeed.X * gameTime.ElapsedGameTime.Milliseconds),
                               (m_maxSpeed.Y * gameTime.ElapsedGameTime.Milliseconds));
        }

        public void Update(GameTime gameTime)
        {
            if (m_targetEntity != null)
            {
                Vector2 actualCameraCenter = new Vector2(CameraRect.Center.X, CameraRect.Center.Y);

                Vector2 idealCameraCenter = new Vector2(m_targetEntity.Position.X + (1 / 5f * ((float)CameraBounds.Width * m_targetEntity.Direction.X)),
                                            m_targetEntity.Position.Y + (1 / 10f * CameraBounds.Height * m_targetEntity.Direction.Y));

                //IvyGame.Get().ConsoleStr += "Ideal Center: " + idealCameraCenter + "\n";
                //IvyGame.Get().ConsoleStr += "Actual Center: " + actualCameraCenter + "\n";

                if (m_targetEntity.Moving)
                {
                    Vector2 maxTravelDist = GetMaxTravelDist(gameTime);

                    Vector2 newCameraCenter = idealCameraCenter;

                    if (Math.Abs(idealCameraCenter.X - actualCameraCenter.X) > maxTravelDist.X)
                    {
                        // transition!
                        if ((idealCameraCenter.X - actualCameraCenter.X) < 0)
                        {
                            newCameraCenter.X = actualCameraCenter.X - maxTravelDist.X;
                        }
                        else if ((idealCameraCenter.X - actualCameraCenter.X) > 0)
                        {
                            newCameraCenter.X = actualCameraCenter.X + maxTravelDist.X;
                        }
                    }

                    if (Math.Abs(idealCameraCenter.Y - actualCameraCenter.Y) > maxTravelDist.Y)
                    {
                        // transition!
                        if ((idealCameraCenter.Y - actualCameraCenter.Y) < 0)
                        {
                            newCameraCenter.Y = actualCameraCenter.Y - maxTravelDist.Y;
                        }
                        else if ((idealCameraCenter.Y - actualCameraCenter.Y) > 0)
                        {
                            newCameraCenter.Y = actualCameraCenter.Y + maxTravelDist.Y;
                        }
                    }


                    //IvyGame.Get().ConsoleStr += "new camera center: " + newCameraCenter + "\n";

                    Rectangle newCameraRect = new Rectangle(
                        (int)(newCameraCenter.X - (CameraBounds.Width / 2f)),
                        (int)(newCameraCenter.Y - (CameraBounds.Height / 2f)),
                        CameraBounds.Width,
                        CameraBounds.Height);


                    if (newCameraRect.X < 0)
                    {
                        newCameraRect.X = 0;
                    }
                    if ((newCameraRect.X + newCameraRect.Width) >= (m_zoneBounds.X + m_zoneBounds.Width))
                    {
                        newCameraRect.X = (m_zoneBounds.X + m_zoneBounds.Width) - CameraRect.Width;
                    }
                    if (newCameraRect.Y < 0)
                    {
                        newCameraRect.Y = 0;
                    }
                    if ((newCameraRect.Y + newCameraRect.Height) >= (m_zoneBounds.Y + m_zoneBounds.Height))
                    {
                        newCameraRect.Y = (m_zoneBounds.Y + m_zoneBounds.Height) - CameraRect.Height;
                    }
                    CameraRect = newCameraRect;
                }
            }
        }
    }
}
