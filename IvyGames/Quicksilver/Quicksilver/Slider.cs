using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Quicksilver
{
    public class Slider : IUserInterfaceElement
    {
        public int Value { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }

        public Point Position { get; set; }

        public Texture2D UpSprite { get; set; }
        public Texture2D DownSprite { get; set; }
        public SpriteFont SliderFont { get; set; }

        // cached positions
        private Rectangle UpRectPosition;
        private Rectangle DownRectPosition;
        private Rectangle ValueDisplayPosition;

        public Slider()
        {
            Value = 0;
            Min = 0;
            Max = 100;

            UpRectPosition = new Rectangle(32, 0, 16, 16);
            DownRectPosition = new Rectangle(0, 0, 16, 16);
            ValueDisplayPosition = new Rectangle(16, 0, 16, 16);
        }

        public void Initialize()
        {
            UpRectPosition = new Rectangle(32, 0, 16, 16);
            UpRectPosition.Offset(Position);
            DownRectPosition = new Rectangle(0, 0, 16, 16);
            DownRectPosition.Offset(Position);
            ValueDisplayPosition = new Rectangle(16, 0, 16, 16);
            ValueDisplayPosition.Offset(Position);
        }

        public void Update(MouseState currentMouseState, MouseState lastMouseState)
        {
            if ((currentMouseState.LeftButton != lastMouseState.LeftButton) &&
                (currentMouseState.LeftButton == ButtonState.Pressed))
            {
                Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);
                if (DownRectPosition.Contains(mousePos))
                {
                    Value = Math.Max(Min, Value - 1);
                }
                else if (UpRectPosition.Contains(mousePos))
                {
                    Value = Math.Min(Max, Value + 1);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(DownSprite, DownRectPosition, Color.White);
            spriteBatch.Draw(UpSprite, UpRectPosition, Color.White);

            Vector2 valueStringPos = new Vector2(ValueDisplayPosition.Left, ValueDisplayPosition.Top);
            spriteBatch.DrawString(SliderFont, Value.ToString(), valueStringPos, Color.White);
        }
    }
}
