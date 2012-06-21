using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Quicksilver
{
    class Block
    {
        public Vector2 Position { get; set; }
        public Rectangle CollisionRect { get; private set; }

        public Texture2D Sprite { get; private set; }

        public Block(Rectangle location, Texture2D sprite)
        {
            Sprite = sprite;
            CollisionRect = location;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, CollisionRect, Color.White);
        }
    }
}
