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

namespace Quicksilver
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class Quicksilver : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics { get; private set; }
        SpriteBatch spriteBatch;

        public static Entity Player { get; private set; }
        public static bool IsPaused { get; set; }

        List<IUserInterfaceElement> uiElementList;

        List<Entity> liveEntities;
        GamePadState lastGamePadState;
        MouseState lastMouseState;
        int remainingSpawnTime;

        Slider playerSpeed;
        Slider fireRate;

        Texture2D playerSprite;
        Texture2D bulletSprite;
        Texture2D enemySprite;
        Texture2D upSprite;
        Texture2D downSprite;

        SpriteFont sliderFont;

        public Quicksilver()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsPaused = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            liveEntities = new List<Entity>();

            Player = new Entity(playerSprite);
            Player.Position = new Vector2(200, 200);
            liveEntities.Add(Player);

            uiElementList = new List<IUserInterfaceElement>();
            playerSpeed = CreateSlider();
            playerSpeed.Position = new Point(600, 400);
            playerSpeed.Initialize();
            uiElementList.Add(playerSpeed);
            fireRate = CreateSlider();
            fireRate.Position = new Point(600, 432);
            fireRate.Initialize();
            uiElementList.Add(fireRate);

            remainingSpawnTime = 5000;

            IsMouseVisible = true;
            IsPaused = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            playerSprite = Content.Load<Texture2D>("art/fireflea");
            enemySprite = Content.Load<Texture2D>("art/fireflea");
            bulletSprite = Content.Load<Texture2D>("art/fireball");
            upSprite = Content.Load<Texture2D>("art/up");
            downSprite = Content.Load<Texture2D>("art/down");

            sliderFont = Content.Load<SpriteFont>("fonts/Slider");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            ///@todo is this correct?
            playerSprite.Dispose();
            enemySprite.Dispose();
            bulletSprite.Dispose();
            upSprite.Dispose();
            downSprite.Dispose();
        }

        protected void CreateBullet(Vector2 position, Vector2 direction)
        {
            Entity bullet = new Entity(bulletSprite);
            bullet.Position = position;
            bullet.Direction = direction;
            bullet.Speed = new Vector2(2, 2);
            bullet.Movement = new SimpleEntityMovement();
            bullet.handleCollision = Collisions.DamageEntity;

            liveEntities.Add(bullet);
        }

        int enemyCount = 0;
        protected void CreateEnemy(Vector2 position)
        {
            Entity enemy = new Entity(enemySprite);
            enemy.Position = position;
            enemy.Direction = new Vector2(-1, 1);
            enemy.Mask = Color.Red;

            if (enemyCount % 5 == 0)
            {
                enemy.Movement = new ZigZagMovement();
            }
            else
            {
                enemy.Movement = new AttackMovement();
                enemy.Speed = new Vector2(0.01f, 0.01f);
            }
            enemyCount++;
            liveEntities.Add(enemy);
        }

        protected Slider CreateSlider()
        {
            Slider s = new Slider();
            s.UpSprite = upSprite;
            s.DownSprite = downSprite;
            s.SliderFont = sliderFont;

            return s;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            GamePadState currentState = GamePad.GetState(PlayerIndex.One);
            MouseState currentMouseState = Mouse.GetState();
            
            // Allows the game to exit
            if (currentState.Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            if ((IsPaused == false) && (Player.isAlive == false))
            {
                IsPaused = true;
                ///@todo OnPause
            }

            if (IsPaused == false)
            {
                // Remove dead entities
                List<Entity> deadEntities = new List<Entity>();
                foreach (Entity e in liveEntities)
                {
                    if (e.isAlive == false)
                    {
                        deadEntities.Add(e);
                    }
                }
                foreach (Entity e in deadEntities)
                {
                    liveEntities.Remove(e);
                }

                // Update game properties based on UI input
                Player.Speed = new Vector2(1 + playerSpeed.Value, 0);

                // Update game state based on player input
                if ((currentState.Buttons.A != lastGamePadState.Buttons.A) &&
                    (currentState.Buttons.A == ButtonState.Pressed))
                {
                    Vector2 direction = Player.Direction;
                    if (Player.Direction.X == 0.0)
                    {
                        direction = new Vector2(1, 0);
                    }
                    CreateBullet(Player.Position, direction);
                }

                // Update game state based on input

                if ((currentState.DPad.Left != lastGamePadState.DPad.Left) ||
                    (currentState.DPad.Right != lastGamePadState.DPad.Right))
                {
                    if (currentState.DPad.Left == ButtonState.Pressed)
                    {
                        Player.Direction = new Vector2(-1, 0);
                    }
                    else if (currentState.DPad.Right == ButtonState.Pressed)
                    {
                        Player.Direction = new Vector2(1, 0);
                    }
                    else
                    {
                        Player.Direction = new Vector2(0, 0);
                    }
                }

                // Update game state that is altered by collisions
                for (int e1 = 0; e1 < liveEntities.Count; ++e1)
                {
                    for (int e2 = e1 + 1; e2 < liveEntities.Count; ++e2)
                    {
                        if (liveEntities[e1].CollisionRect.Intersects(liveEntities[e2].CollisionRect))
                        {
                            liveEntities[e1].OnCollision(liveEntities[e2]);
                            liveEntities[e2].OnCollision(liveEntities[e1]);
                        }
                    }
                }

                // Apply these updates to all entities
                foreach (Entity e in liveEntities)
                {
                    e.Update();
                }

                // Entity spawning takes place after all other entities have been updated
                // (one side effect of spawning before entity update is existing entities may be affected)
                remainingSpawnTime -= gameTime.ElapsedGameTime.Milliseconds;
                if (remainingSpawnTime <= 0)
                {
                    Random r = new Random();

                    Vector2 position = new Vector2(r.Next(GraphicsDevice.Viewport.Bounds.Width),
                                                   r.Next(GraphicsDevice.Viewport.Bounds.Height));
                    CreateEnemy(position);

                    remainingSpawnTime = 500;
                }

                // Finally Update UI
                foreach (IUserInterfaceElement uiElement in uiElementList)
                {
                    uiElement.Update(currentMouseState, lastMouseState);
                }
            }

            lastGamePadState = currentState;
            lastMouseState = currentMouseState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            foreach (Entity e in liveEntities)
            {
                e.Draw(spriteBatch);
            }

            foreach (IUserInterfaceElement uiElement in uiElementList)
            {
                uiElement.Draw(spriteBatch);
            }

            // Draw Mouse Position
            Point mousePos = new Point(lastMouseState.X, lastMouseState.Y);
            spriteBatch.DrawString(sliderFont, "Mouse Pos:" + mousePos.ToString(), Vector2.Zero, Color.White);
            spriteBatch.DrawString(sliderFont, "Player Energy: " + Player.Energy.ToString(), new Vector2(0, 16), Color.White);

            if (IsPaused)
            {
                if (Player.isAlive)
                {
                    spriteBatch.DrawString(sliderFont, "Paused", new Vector2(380, 280), Color.Blue);
                }
                else
                {
                    spriteBatch.DrawString(sliderFont, "GAME OVER", new Vector2(380, 280), Color.Red);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
