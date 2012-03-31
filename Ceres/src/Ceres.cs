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

namespace Ceres
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Ceres : Ivy.IvyGame, IMessageSender
    {
        ///@todo add player management
        protected Player m_playerOne;

        private Timer EscapeTimer;
        
        public Ceres()
        {

        }

        protected override void Initialize()
        {
            base.Initialize();

            WorldZone shaftZone = new WorldZone(@"levels/shaft");

            // Add player

            // TODO: Find out if components need to be individually initialized here,
            //       XNA may provide a way of doing this.
            m_playerOne = new Player(this);
            m_playerOne.Initialize();
            ChangeZoneMsg addEntityMsg = new ChangeZoneMsg(this, shaftZone, m_playerOne, shaftZone.ZoneName, new Point(112, 16), 0);
            MessageDispatcher.Get().SendMessage(addEntityMsg);

            SetCurrentZone(shaftZone);

            SetCameraTarget(m_playerOne);

            // Create Camera
            Rectangle screenRect = new Rectangle(0, 0, 800, 600);

            int cameraHeight = 192;
            int cameraWidth = 256;
            Rectangle cameraBounds = new Rectangle(0, 0, cameraWidth, cameraHeight);
            Camera.Initialize(shaftZone.Bounds, cameraBounds, screenRect);

            CameraGel = Content.Load<Texture2D>(@"art/bg_layer_blueaura");
            GelTint = new Color(0.0f, 0.0f, 1.0f, 0.0f);

            EscapeTimer = new Timer(1000 * 60);  // One minute to escape
            EscapeTimer.OnTimeExpired += OnEscapeTimeExpired;

            #region Register Input Handlers
            // Movement
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickLeft, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickRight, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickUp, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.LeftThumbstickDown, OnGamePadDirectionEvent);

            InputMgr.Get().RegisterGamePadButton(Buttons.DPadLeft, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.DPadRight, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.DPadUp, OnGamePadDirectionEvent);
            InputMgr.Get().RegisterGamePadButton(Buttons.DPadDown, OnGamePadDirectionEvent);

            InputMgr.Get().RegisterKey(Keys.Up, OnKeyboardDirectionEvent);
            InputMgr.Get().RegisterKey(Keys.Down, OnKeyboardDirectionEvent);
            InputMgr.Get().RegisterKey(Keys.Left, OnKeyboardDirectionEvent);
            InputMgr.Get().RegisterKey(Keys.Right, OnKeyboardDirectionEvent);

            // Actions
            InputMgr.Get().RegisterGamePadButton(Buttons.A, OnGamePadButtonEvent); // Run/Alt
            InputMgr.Get().RegisterGamePadButton(Buttons.B, OnGamePadButtonEvent); // Jump
            InputMgr.Get().RegisterGamePadButton(Buttons.Y, OnGamePadButtonEvent); // Fire Weapon

            // Keyboard Actions
            InputMgr.Get().RegisterKey(Keys.F, OnKeyboardEvent);                    // Fire
            InputMgr.Get().RegisterKey(Keys.Space, OnKeyboardEvent);                // Jump

            // Debug Actions
            InputMgr.Get().RegisterGamePadButton(Buttons.X, DebugOnGamePadButtonEvent);
            #endregion
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (EscapeTimer != null)
            {
                EscapeTimer.Update(gameTime);
            }

            base.Update(gameTime);

            ConsoleStr += "Energy: " + m_playerOne.Energy + "\n";

            if ((EscapeTimer != null) && (EscapeTimer.Paused == false))
            {
                ConsoleStr += "Self Destruct in " + (EscapeTimer.TimeRemaining / 1000) + " seconds";
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void ReceiveMessage(Message msg)
        {
            if (msg.Type == MessageType.RidleyDead)
            {
                EscapeTimer.Paused = false;

                GetCurrentZone().SetEscapeMode(true);
                
                CameraGel = Content.Load<Texture2D>(@"art/bg_layer_redaura");
                GelTint = new Color(1.0f, 0.0f, 0.0f, 0.0f);
            }
            else
            {
                base.ReceiveMessage(msg);
            }
        }

        private void OnEscapeTimeExpired()
        {
            MessageDispatcher.Get().SendMessage(new Message(MessageType.EndGame, this, this));
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
                    Message msg = new Message(MessageType.MoveLeft, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
                else if ((e.EventType == InputEventType.Released) &&
                         (InputMgr.Get().GamePadState.IsButtonUp(Buttons.LeftThumbstickRight)) &&
                         (InputMgr.Get().GamePadState.IsButtonUp(Buttons.DPadRight)))
                {
                    // TODO: need a better way of handling mutually exclusive button inputs

                    Message msg = new Message(MessageType.Stand, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
            }
            else if ((e.Button == Buttons.LeftThumbstickRight) ||
                     (e.Button == Buttons.DPadRight))
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    Message msg = new Message(MessageType.MoveRight, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
                else if ((e.EventType == InputEventType.Released) &&
                         (InputMgr.Get().GamePadState.IsButtonUp(Buttons.LeftThumbstickLeft)) &&
                         (InputMgr.Get().GamePadState.IsButtonUp(Buttons.DPadLeft)))
                {
                    // TODO: need a better way of handling mutually exclusive button inputs

                    Message msg = new Message(MessageType.Stand, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
            }

            return true;
        }

        bool OnKeyboardDirectionEvent(KeyboardEvent e)
        {
            // TODO: design some way to map events to messages to avoid switch code like this

            // TODO:  add some intelligence here to make sure that left and right keys are mutually exclusive
            if (e.Key == Keys.Left)
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    Message msg = new Message(MessageType.MoveLeft, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
                else if (e.EventType == InputEventType.Released)
                {
                    Message msg = new Message(MessageType.Stand, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
            }
            else if (e.Key == Keys.Right)
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    Message msg = new Message(MessageType.MoveRight, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
                else if (e.EventType == InputEventType.Released)
                {
                    Message msg = new Message(MessageType.Stand, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
            }

            return true;
        }

        protected override bool OnGamePadButtonEvent(GamePadButtonEvent e)
        {
            bool ret = true;

            if (e.Button == Buttons.B)
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    // start jump
                    Message msg = new Message(MessageType.Jump, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
                else if (e.EventType == InputEventType.Released)
                {
                    // end jump!
                    Message msg = new Message(MessageType.Fall, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
            }
            else if ((e.Button == Buttons.Y) && (e.EventType == InputEventType.Pressed))
            {
                // Fire
                Message msg = new Message(MessageType.FireWeapon, this, m_playerOne);
                MessageDispatcher.Get().SendMessage(msg);
            }
            else
            {
                ret = base.OnGamePadButtonEvent(e);
            }

            return ret;
        }

        bool DebugOnGamePadButtonEvent(GamePadButtonEvent e)
        {
            return true;
        }

        protected override bool OnKeyboardEvent(KeyboardEvent e)
        {
            bool ret = true;

            // Jump!
            if (e.Key == Keys.Space)
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    // start jump
                    Message msg = new Message(MessageType.Jump, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
                else if (e.EventType == InputEventType.Released)
                {
                    // end jump
                    Message msg = new Message(MessageType.Fall, this, m_playerOne);
                    MessageDispatcher.Get().SendMessage(msg);
                }
            }
            else if ((e.Key == Keys.F) && (e.EventType == InputEventType.Pressed))
            {
                // Fire
                Message msg = new Message(MessageType.FireWeapon, this, m_playerOne);
                MessageDispatcher.Get().SendMessage(msg);
            }
            else
            {
                ret = base.OnKeyboardEvent(e);
            }

            return ret;
        }
        #endregion
    }
}
