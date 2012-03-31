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


namespace IvyTests
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ParticleSystem : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private struct ParticleData
        {
            public float birthTime;
            public float maxAge;
            public Vector2 position;
            public Vector2 direction;
            public Color color;

        };

        private List<ParticleData> m_particleList;
        private Random m_generator;

        private Texture2D m_particleSprite;

        public ParticleSystem(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public override void Initialize()
        {
            base.Initialize();

            m_generator = new Random(0);

            m_particleList = new List<ParticleData>();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            m_particleSprite = Game.Content.Load<Texture2D>("particle");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            List<ParticleData> deadParticles = new List<ParticleData>();

            float speed = 0.5f;

            for (int idx = 0; idx < m_particleList.Count; ++idx)
            {
                ParticleData p = m_particleList[idx];

                if ((gameTime.TotalGameTime.Milliseconds + gameTime.TotalGameTime.Seconds * 1000) > (p.birthTime + p.maxAge))
                {
                    deadParticles.Add(p);
                }
                else
                {
                    p.position += speed * m_particleList[idx].direction;
                    m_particleList[idx] = p;
                }
            }

            foreach (ParticleData p in deadParticles)
            {
                m_particleList.Remove(p);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch batch = new SpriteBatch(Game.GraphicsDevice);

            batch.Begin();

            Vector2 spriteCenter = new Vector2(m_particleSprite.Width / 2, m_particleSprite.Height / 2);

            foreach (ParticleData p in m_particleList)
            {
                batch.Draw(m_particleSprite, p.position + spriteCenter, p.color);
            }

            batch.End();
        }

        public void AddParticleEffect(Vector2 position, Rectangle bounds, int particleCount, GameTime gameTime)
        {
            float maxAge = 20000f;

            for (int i = 0; i < particleCount; ++i)
            {
                ParticleData particle = new ParticleData();

                particle.position = position;
                particle.birthTime = gameTime.TotalGameTime.Milliseconds + gameTime.TotalGameTime.Seconds * 1000;
                particle.color = new Color((float)m_generator.NextDouble(), (float)m_generator.NextDouble(), (float)m_generator.NextDouble());
                particle.direction = new Vector2((float)(m_generator.NextDouble() * 2f) - 1f, (float)(m_generator.NextDouble() * 2f) - 1f);
                particle.maxAge = maxAge;

                m_particleList.Add(particle);
            }
        }
    }
}