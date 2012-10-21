using System;
using System.Collections.Generic;

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
    /// This is the main type for your game
    /// </summary>
    public abstract class IvyGame : Microsoft.Xna.Framework.Game, IMessageSender, IMessageReceiver
    {
        private static IvyGame m_instance = null;

        public enum GameState
        {
            Pause,
            Play,
            GameOver,
        };

        public GameState State { get; protected set; }

        GraphicsDeviceManager IvyGraphics;
        SpriteBatch IvySpriteBatch;

        // World Data
        private WorldZone m_currentZone;

        public Camera2D Camera { get; private set; }

        public Entity m_cameraTarget;

        // refactor into Console class
        public string ConsoleStr { get; set; }
        private SpriteFont consoleFont;
        private Vector2 consolePos;

        private float m_fpsValue;
        private string m_fpsStr;

        protected Texture2D CameraGel { get; set; }
        protected Color GelTint { get; set; }

        // Debug Options
        private bool DrawCollisionRects { get; set; }

        protected IvyGame()
        {
            State = GameState.Play;

            DrawCollisionRects = false;

            m_fpsValue = 0.0f;

            IvyGraphics = new GraphicsDeviceManager(this);
            IvyGraphics.PreferredBackBufferWidth = 800;
            IvyGraphics.PreferredBackBufferHeight = 600;
            IvyGraphics.ApplyChanges(); ///@todo is this needed?

            Content.RootDirectory = "Content";

            if (m_instance != null)
            {
                // @todo error! exception?
            }

            m_instance = this;
        }
        
        public static IvyGame Get()
        {
            if (m_instance == null)
            {
                //@TODO exception!
                // uh oh!  shouldn't even happen!
            }

            return m_instance;
        }
       
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize components
            base.Initialize();

            Camera = new Camera2D();

            GelTint = Color.White;
    
            ConsoleStr = "\n";
            m_fpsStr = "\n";

            InputMgr.Get().RegisterKey(Keys.P, OnKeyboardEvent);    // Pause/Play
            InputMgr.Get().RegisterKey(Keys.Q, OnKeyboardEvent);    // Quit
            InputMgr.Get().RegisterKey(Keys.F1, OnKeyboardEvent);   // Debug - Collision Rects

            InputMgr.Get().RegisterGamePadButton(Buttons.Back, OnGamePadButtonEvent);            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            IvySpriteBatch = new SpriteBatch(GraphicsDevice);
        
            ///@todo move to console class
            consoleFont = Content.Load<SpriteFont>(@"Fonts\\Console");
            consolePos = new Vector2(40, 50);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ConsoleStr = "\n\n";
            
            // Dispatch queued messages
            MessageDispatcher.Get().Update(gameTime);

            // Input
            InputMgr.Get().Update();

            if (State == GameState.Play)
            {
                if (m_currentZone != null)
                {
                    m_currentZone.Update(gameTime);
                }

                // if multiple rooms are active in a game, they should all be updated here
            }
            else if (State == GameState.GameOver)
            {
                this.Exit();
            }

            // Update Camera based on Player Position
            Camera.Update(gameTime);

            // Update Sound FX?

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            m_fpsValue = (m_fpsValue * 3) + ((1 / (float)gameTime.ElapsedGameTime.Milliseconds) * 1000);
            m_fpsValue /= 4.0f;
            m_fpsStr = "FPS: " + (int)m_fpsValue + "\n";

            GraphicsDevice.Clear(Color.Black);
   
            IvySpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            SamplerState pointSampler = new SamplerState();
            pointSampler.Filter = TextureFilter.Point;
            IvyGraphics.GraphicsDevice.SamplerStates[0] = pointSampler;

            // if multiple rooms are active, only the current one is drawn
            if (m_currentZone != null)
            {
                m_currentZone.Draw(IvySpriteBatch);
            }

            // Draw 'Camera Gel' to tint screen
            if (CameraGel != null)
            {
                IvySpriteBatch.Draw(CameraGel, Camera.ScreenRect, GelTint);
            }

            // Draw Console           
            // Find the center of the string
            Vector2 FontCenter = consoleFont.MeasureString(ConsoleStr) / 2;

            Vector2 drawConsolePos = new Vector2(consolePos.X + FontCenter.X, consolePos.Y);
                        
            // Draw the string
            IvySpriteBatch.DrawString(consoleFont, ConsoleStr, drawConsolePos, Color.LimeGreen,
                                   0, FontCenter, 1.2f, SpriteEffects.None, 0.5f);

            string dataString = m_fpsStr;

            Vector2 FpsDims = consoleFont.MeasureString(dataString);
            Vector2 drawFpsPos = new Vector2((Camera.ScreenRect.Right - (FpsDims.X * 2)), FpsDims.Y);
            IvySpriteBatch.DrawString(consoleFont, dataString, drawFpsPos, Color.LimeGreen, 0, FpsDims / 2, 2.0f, SpriteEffects.None, 0.5f);

            IvySpriteBatch.End();

            if ((m_currentZone != null) && DrawCollisionRects)
            {
                m_currentZone.Draw3D();
            }

            base.Draw(gameTime);
        }

        protected void SetCameraTarget(Entity target)
        {
            m_cameraTarget = target;
            Camera.SetTarget(target);
        }

        public WorldZone GetCurrentZone()
        {
            return m_currentZone;
        }

        public void SetCurrentZone(WorldZone room)
        {
            m_currentZone = room;
        }

        public virtual void ReceiveMessage(Message msg)
        {
            if (msg.Type == MessageType.ChangeZone)
            {
                HandleChangeZoneMsg((ChangeZoneMsg)msg);                
            }
            else if (msg.Type == MessageType.PauseGame)
            {
                State = GameState.Pause;
            }
            else if (msg.Type == MessageType.PlayGame)
            {
                State = GameState.Play;
            }
            else if (msg.Type == MessageType.EndGame)
            {
                HandleGameEndMsg(msg);;
            }
        }

        private void HandleChangeZoneMsg(ChangeZoneMsg msg)
        {
            // Pause, Transition Room, Then Pass Message onto both rooms

            if (m_currentZone != null)
            {
                MessageDispatcher.Get().SendMessage(
                    new ChangeZoneMsg(this, m_currentZone, msg.Entity, msg.DestZone, msg.DestPosition, 1));
            }

            if (msg.Entity == m_cameraTarget)
            {
                WorldZone destZone = new WorldZone(msg.DestZone);

                destZone.SetEscapeMode(GetCurrentZone().EscapeMode);

                SetCurrentZone(destZone);
                Camera.SetZoneBounds(destZone.Bounds);

                Camera.SetTarget(msg.Entity);

                if (msg.DestZone != null)
                {
                    MessageDispatcher.Get().SendMessage(
                        new ChangeZoneMsg(this, destZone, msg.Entity, msg.DestZone, msg.DestPosition, 1));
                }
            }
        }

        protected virtual void HandleGameEndMsg(Message msg)
        {
            if (msg.Type == MessageType.EndGame)
            {
                State = GameState.GameOver;
            }
        }

        protected virtual bool OnKeyboardEvent(KeyboardEvent e)
        {
            if (e.Key == Keys.P)
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    if (State == GameState.Pause)
                    {
                        MessageDispatcher.Get().SendMessage(new Message(MessageType.PlayGame, this, this));
                    }
                    else if (State == GameState.Play)
                    {
                        MessageDispatcher.Get().SendMessage(new Message(MessageType.PauseGame, this, this));
                    }
                }
            }
            else if (e.Key == Keys.Q)
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    MessageDispatcher.Get().SendMessage(new Message(MessageType.EndGame, this, this));
                }
            }
            else if (e.Key == Keys.F1)
            {
                if (e.EventType == InputEventType.Pressed)
                {
                    DrawCollisionRects = !DrawCollisionRects;
                }
            }

            return true;
        }

        protected virtual bool OnGamePadButtonEvent(GamePadButtonEvent e)
        {
            // Allows the game to exit
            if (e.Button == Buttons.Back && e.EventType == InputEventType.Pressed)
            {
                MessageDispatcher.Get().SendMessage(new Message(MessageType.EndGame, this, this));
            }

            return true;
        }
    }
}
