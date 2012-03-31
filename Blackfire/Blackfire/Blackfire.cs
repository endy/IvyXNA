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

namespace Blackfire
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Blackfire : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model ship;

        protected float AspectRatio {get; private set; }

        // Set the position of the model in world space, and set the rotation.
        Vector3 shipPosition = Vector3.Zero;

        struct ShipOrientation
        {
            public float Yaw;
            public float Pitch;
            public float Roll;

            public ShipOrientation(float yaw = 0.0f, float pitch = 0.0f, float roll = 0.0f)
            {
                Yaw = yaw;
                Pitch = pitch;
                Roll = roll;
            }
        };

        ShipOrientation shipOrientation;

        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 5000.0f);

        

        public Blackfire()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            LoadContent();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ship = Content.Load<Model>("Models\\p1_wedge");

            AspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                ButtonState.Pressed)
                this.Exit();

            // Pitch +/-
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.W))
            {
                shipOrientation.Pitch += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                        MathHelper.ToRadians(0.1f);
            }

            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S))
            {
                shipOrientation.Pitch -= (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                        MathHelper.ToRadians(0.1f);
            }

            // Yaw +/-
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.A))
            {
                shipOrientation.Yaw += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                        MathHelper.ToRadians(0.1f);
            }
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.D))
            {
                shipOrientation.Yaw -= (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                        MathHelper.ToRadians(0.1f);
            }
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[ship.Bones.Count];
            ship.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in ship.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    
                    effect.World = transforms[mesh.ParentBone.Index] *
                                   Matrix.CreateRotationX(shipOrientation.Pitch) *
                                   Matrix.CreateRotationY(shipOrientation.Yaw) * 
                                   Matrix.CreateTranslation(shipPosition);

                    effect.View = Matrix.CreateLookAt(cameraPosition,
                                                      Vector3.Zero, 
                                                      Vector3.Up);
                    
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                                            MathHelper.ToRadians(45.0f), 
                                            AspectRatio,
                                            1.0f, 
                                            10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
