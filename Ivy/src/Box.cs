using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Ivy
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Box
    {
        private static Box m_instance;

        Matrix scaleMatrix;
        Matrix transMatrix;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        BasicEffect basicEffect;
        VertexDeclaration vertexDeclaration;

        VertexPositionColor[] vertexArray;
        short[] lineIndicies;

        Rectangle m_rect;

        private Box()
        {
            m_rect = new Rectangle(0, 0, 1, 1);
        }

        public static Box Get()
        {
            if (m_instance == null)
            {
                m_instance = new Box();
                m_instance.Initialize();
            }

            return m_instance;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        private void Initialize()
        {
            InitializeVertexes();
            InitializeTransform();
            InitializeEffect();
        }

        private void InitializeVertexes()
        {
            vertexArray = new VertexPositionColor[4];

            vertexArray[0].Position = Vector3.Zero;
            vertexArray[0].Color = Color.LawnGreen;
            vertexArray[1].Position = new Vector3(m_rect.Width, 0, 0);
            vertexArray[1].Color = Color.LawnGreen;
            vertexArray[2].Position = new Vector3(m_rect.Width, m_rect.Height, 0);
            vertexArray[2].Color = Color.LawnGreen;
            vertexArray[3].Position = new Vector3(0, m_rect.Height, 0);
            vertexArray[3].Color = Color.LawnGreen;


            lineIndicies = new short[5] { 0, 1, 2, 3, 0 };
        }

        private void InitializeTransform()
        {
            viewMatrix = Matrix.CreateLookAt(
                new Vector3(0.0f, 0.0f, 1.0f),
                Vector3.Zero,
                Vector3.Up
                );

            projectionMatrix = Matrix.CreateOrthographicOffCenter(
                0,
                (float)IvyGame.Get().GraphicsDevice.Viewport.Width,
                (float)IvyGame.Get().GraphicsDevice.Viewport.Height,
                0,
                1.0f, 1000.0f);
        }

        private void InitializeEffect()
        {

            vertexDeclaration = VertexPositionColor.VertexDeclaration;

            basicEffect = new BasicEffect(IvyGame.Get().GraphicsDevice);
            basicEffect.VertexColorEnabled = true;

            scaleMatrix = Matrix.CreateScale(1f);
            transMatrix = Matrix.CreateTranslation(0, 0, 0);

            basicEffect.World      = scaleMatrix * transMatrix;
            basicEffect.View       = viewMatrix;
            basicEffect.Projection = projectionMatrix;
        }

        public void Draw(Point Position, Rectangle rect)
        {
            Rectangle cameraRect = IvyGame.Get().Camera.CameraRect;

            scaleMatrix = Matrix.CreateScale(rect.Width, rect.Height, 1);

            Vector2 screenPos = IvyGame.Get().Camera.GetScreenPointForRoomPoint(new Vector2(Position.X, Position.Y));
            transMatrix = Matrix.CreateTranslation(screenPos.X, screenPos.Y, 0);
            basicEffect.World = scaleMatrix * transMatrix;       

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                IvyGame.Get().GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineStrip,
                    vertexArray,
                    0, 
                    4,
                    lineIndicies,  // index of the first vertex to draw
                    0, 
                    4);   // number of primitives
            }
        }
        
    }
}