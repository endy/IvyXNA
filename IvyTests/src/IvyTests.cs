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

using Ivy;

namespace IvyTests
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class IvyTests : IvyGame
    {
        ParticleSystem m_particleSystem;

        Vector2 emitterPosition;



        public IvyTests()
        {

        }
    
        protected override void Initialize()
        {
            m_particleSystem = new ParticleSystem(this);
            Components.Add(m_particleSystem);

            emitterPosition = Vector2.Zero;

            // Movement
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickLeft, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickRight, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickUp, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickDown, OnGamePadDirectionEvent);

            InputMgr.Get().RegisterGamePadButton(Buttons.DPadLeft, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.DPadRight, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.DPadUp, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.DPadDown, OnGamePadDirectionEvent);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            base.LoadContent();
        }


        protected override void UnloadContent()
        {
            base.UnloadContent();
        }
        
        static float elapsedTime = 0;
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
            if (elapsedTime > 1000)
            {
                m_particleSystem.AddParticleEffect(emitterPosition, new Rectangle(0, 0, 800, 600), 10, gameTime);
                elapsedTime = 0;
            }
        }


        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        #region Input Handlers
        bool OnGamePadDirectionEvent(GamePadButtonEvent e)
        {
            // TODO: design some way to map events to messages to avoid switch code like this
            if ((e.Button == Buttons.LeftThumbstickLeft) ||
                (e.Button == Buttons.DPadLeft))
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    emitterPosition.X -= 15;
                }
            }
            else if ((e.Button == Buttons.LeftThumbstickRight) ||
                     (e.Button == Buttons.DPadRight))
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    emitterPosition.X += 15;
                }
            }
            else if ((e.Button == Buttons.LeftThumbstickUp) ||
                     (e.Button == Buttons.DPadUp))
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    emitterPosition.Y -= 15;
                }
            }
            else if ((e.Button == Buttons.LeftThumbstickDown) ||
                     (e.Button == Buttons.DPadDown))
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    emitterPosition.Y += 15;
                }
            }

            return true;
        }
        #endregion
    }
}
