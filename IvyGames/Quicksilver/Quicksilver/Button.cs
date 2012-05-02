using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Quicksilver
{
    interface IUserInterfaceElement
    {
        void Update(MouseState currentMouseState, MouseState lastMouseState);
        void Draw(SpriteBatch spriteBatch);
    }


    class Button : IUserInterfaceElement
    {
        public Texture2D ButtonSpritePressed { get; set; }
        public Texture2D ButtonSpriteReleased { get; set; }
        public Rectangle ButtonBounds { get; set; }

        public ButtonState CurrentState { get; private set; }

        public Button()
        {
            ButtonBounds = new Rectangle(0, 0, 32, 32);
            CurrentState = ButtonState.Released;
        }

        public void Update(MouseState currentMouseState, MouseState lastMouseState)
        {
            if (currentMouseState.LeftButton != lastMouseState.LeftButton)
            {
                Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);
                if (ButtonBounds.Contains(mousePos))
                {
                    CurrentState = ButtonState.Pressed;
                }
                else
                {
                    CurrentState = ButtonState.Released;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentState == ButtonState.Pressed)
            {
                spriteBatch.Draw(ButtonSpritePressed, ButtonBounds, Color.White);
            }
            else
            {
                spriteBatch.Draw(ButtonSpriteReleased, ButtonBounds, Color.White);
            }
        }
    }
}
