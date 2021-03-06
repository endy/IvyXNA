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

        // Game State
        public static Entity Player { get; private set; }
        public static bool IsPaused { get; set; }

        // UI
        List<IUserInterfaceElement> uiElementList;
        Slider playerSpeed;
        Slider jumpSpeed;

        // Game Entities
        List<Entity> liveEntities;
        GamePadState lastGamePadState;
        MouseState lastMouseState;
        int remainingSpawnTime;

        // Game World
        List<Block> worldElements;

        // Resources
        Texture2D playerSprite;
        Texture2D bulletSprite;
        Texture2D enemySprite;
        Texture2D upSprite;
        Texture2D downSprite;
        Texture2D blockSprite;

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


            // Setup the World
            worldElements = new List<Block>();

            CreateWorld();

            // Setup Entities
            liveEntities = new List<Entity>();

            Player = new Entity(playerSprite);
            Player.Position = new Vector2(200, 200);
            Player.MovementState = Entity.EntityMovementState.Falling;
            liveEntities.Add(Player);

            uiElementList = new List<IUserInterfaceElement>();
            playerSpeed = CreateSlider();
            playerSpeed.Position = new Point(600, 400);
            playerSpeed.Initialize();
            playerSpeed.Value = (int)Player.Speed.X;
            uiElementList.Add(playerSpeed);
            jumpSpeed = CreateSlider();
            jumpSpeed.Position = new Point(600, 432);
            jumpSpeed.Initialize();
            uiElementList.Add(jumpSpeed);

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
            blockSprite = Content.Load<Texture2D>("art/32x32block");

#if WINDOWS || XBOX
            sliderFont = Content.Load<SpriteFont>("fonts/Slider");
#endif  // WINDOWS || XBOX
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
            blockSprite.Dispose();
        }

        protected void CreateWorld()
        {
            Block b = new Block(new Rectangle(0, 400, 180, 32), blockSprite);
            worldElements.Add(b);

            b = new Block(new Rectangle(200, 400, 80, 32), blockSprite);
            worldElements.Add(b);

            b = new Block(new Rectangle(320, 400, 80, 32), blockSprite);
            worldElements.Add(b);

            b = new Block(new Rectangle(460, 400, 300, 32), blockSprite);
            worldElements.Add(b);
        }

        protected void CreateBullet(Vector2 position, Vector2 direction)
        {
            Entity bullet = new Entity(bulletSprite);
            bullet.Position = position;
            bullet.Direction = direction;
            bullet.Speed = new Vector2(2, 2);
            bullet.MovementState = Entity.EntityMovementState.Flying;
            bullet.Movement = new SimpleEntityMovement();
            bullet.handleCollision = Collisions.DamageEntity;
            bullet.Parent = Player; ///@todo Temporary.  Refactor

            liveEntities.Add(bullet);
        }

        int enemyCount = 0;
        protected void CreateEnemy(Vector2 position)
        {
            Entity enemy = new Entity(enemySprite);
            enemy.Position = position;
            enemy.Direction = new Vector2(-1, 1);
            enemy.Mask = Color.Red;
            enemy.MovementState = Entity.EntityMovementState.Flying;

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

#if WINDOWS || XBOX
            s.SliderFont = sliderFont;
#endif // WINDOWS || XBOX

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

                if (currentState.Buttons.B != lastGamePadState.Buttons.B)
                {
                    if ((Player.MovementState == Entity.EntityMovementState.Walking) &&
                        (currentState.Buttons.B == ButtonState.Pressed))
                    {
                        Player.MovementState = Entity.EntityMovementState.Jumping;
                    }

                    if ((Player.MovementState == Entity.EntityMovementState.Jumping) &&
                        (currentState.Buttons.B == ButtonState.Released))
                    {
                        Player.MovementState = Entity.EntityMovementState.Falling;
                    }
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

                // Update entity states based on collisions
                for (int eIdx = 0; eIdx < liveEntities.Count; ++eIdx)
                {
                    for (int bIdx = 0; bIdx < worldElements.Count; ++bIdx)
                    {
                        if (liveEntities[eIdx].CollisionRect.Intersects(worldElements[bIdx].CollisionRect))
                        {
                            liveEntities[eIdx].OnWorldCollision(worldElements[bIdx]);
                        }
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

            foreach (Block b in worldElements)
            {
                b.Draw(spriteBatch);
            }

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

#if WINDOWS || XBOX
            spriteBatch.DrawString(sliderFont, "Mouse Pos:" + mousePos.ToString(), Vector2.Zero, Color.White);
            spriteBatch.DrawString(sliderFont, "Player Energy: " + Player.Energy.ToString(), new Vector2(0, 16), Color.White);
            spriteBatch.DrawString(sliderFont, "Entity Count: " + liveEntities.Count.ToString(), new Vector2(0, 32), Color.White);
#endif // WINDOWS || XBOX

            if (IsPaused)
            {
#if WINDOWS || XBOX
                if (Player.isAlive)
                {
                    spriteBatch.DrawString(sliderFont, "Paused", new Vector2(380, 280), Color.Blue);
                }
                else
                {
                    spriteBatch.DrawString(sliderFont, "GAME OVER", new Vector2(380, 280), Color.Red);
                }
#endif // WINDOWS || XBOX
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
