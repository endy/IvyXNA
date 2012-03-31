using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Ivy
{
    public class InputMgr
    {
        public delegate bool VariableInputEventHandler(VariableInputEvent e);
        public delegate bool GamePadButtonEventHandler(GamePadButtonEvent e);
        public delegate bool KeyboardEventHandler(KeyboardEvent e);

        private class RegisteredVariableInput
        {
            public VariableInputType InputType { get; private set; }

            private event VariableInputEventHandler OnVariableInputEvent;

            public RegisteredVariableInput(VariableInputType inputType)
            {
                InputType = inputType;
            }

            public void AddHandler(VariableInputEventHandler handler)
            {
                OnVariableInputEvent += handler;
            }

            public void RemoveHandler(VariableInputEventHandler handler)
            {
                OnVariableInputEvent -= handler;
            }

            public void FireEvent(VariableInputEvent e)
            {
                OnVariableInputEvent(e);
            }

            public bool Empty
            {
                get { return (OnVariableInputEvent == null); }
            }
        }

        private class RegisteredGamePadButton
        {
            public Buttons Button { get; private set; }

            private event GamePadButtonEventHandler OnGamePadButtonEvent;

            public RegisteredGamePadButton(Buttons button)
            {
                Button = button;
            }

            public void AddHandler(GamePadButtonEventHandler handler)
            {
                OnGamePadButtonEvent += handler;
            }

            public void RemoveHandler(GamePadButtonEventHandler handler)
            {
                OnGamePadButtonEvent -= handler;
            }

            public void FireEvent(GamePadButtonEvent e)
            {
                OnGamePadButtonEvent(e);
            }

            public bool Empty
            {
                get { return (OnGamePadButtonEvent == null); }
            }
        }

        private class RegisteredKey
        {
            public Keys Key { get; private set; }

            private event KeyboardEventHandler OnKeyboardEvent;

            public RegisteredKey(Keys key)
            {
                Key = key;
            }

            public void AddHandler(KeyboardEventHandler handler)
            {
                OnKeyboardEvent += handler;
            }

            public void RemoveHandler(KeyboardEventHandler handler)
            {
                OnKeyboardEvent -= handler;
            }

            public void FireEvent(KeyboardEvent e)
            {
                OnKeyboardEvent(e);
            }

            public bool Empty
            {
                get { return (OnKeyboardEvent == null); }
            }
        }

        private KeyboardState m_keyboardState;
        private KeyboardState m_prevKeyboardState;
        
        private MouseState m_mouseState;
        private MouseState m_prevMouseState;

        // Only support a single player
        private GamePadState m_gamePadStateP1;
        private GamePadState m_prevGamePadStateP1;

        static InputMgr m_instance = null;

        List<RegisteredVariableInput> m_registeredVariableInput;
        List<RegisteredGamePadButton> m_registeredButtons;
        List<RegisteredKey> m_registeredKeys;

        private InputMgr()
        {

        }

        static public InputMgr Get()
        {
            if (m_instance == null)
            {
                m_instance = new InputMgr();
                m_instance.Initialize();
            }

            return m_instance;
        }

        private void Initialize()
        {
            m_registeredVariableInput = new List<RegisteredVariableInput>();
            m_registeredButtons = new List<RegisteredGamePadButton>();
            m_registeredKeys = new List<RegisteredKey>();

            Update();   // Initialize current states
            Update();   // Initialize previous states
        }

        #region Register Input Methods
        public void RegisterVariableInput(VariableInputType inputType, VariableInputEventHandler eventHandler)
        {
            int viIdx = 0;
            foreach (RegisteredVariableInput rvi in m_registeredVariableInput)
            {
                if (rvi.InputType != inputType)
                {
                    viIdx++;
                }
                else
                {
                    break;
                }
            }

            if (viIdx == m_registeredVariableInput.Count)
            {
                // Register new input type
                RegisteredVariableInput rvi = new RegisteredVariableInput(inputType);
                rvi.AddHandler(eventHandler);

                m_registeredVariableInput.Add(rvi);
            }
            else
            {
                // Add handler to previously registered input type
                m_registeredVariableInput[viIdx].AddHandler(eventHandler);
            }
        }

        public void UnregisterVariableInput(VariableInputType inputType, VariableInputEventHandler eventHandler)
        {
            for (int idx = 0; idx < m_registeredVariableInput.Count; ++idx)
            {
                if (m_registeredVariableInput[idx].InputType == inputType)
                {
                    m_registeredVariableInput[idx].RemoveHandler(eventHandler);

                    if (m_registeredVariableInput[idx].Empty)
                    {
                        m_registeredVariableInput.RemoveAt(idx);
                    }

                    break;
                }
            }
        }

        public void RegisterGamePadButton(Buttons button, GamePadButtonEventHandler eventHandler)
        {
            int buttonIdx = -1;
            for (int idx = 0; idx < m_registeredButtons.Count; ++idx)
            {
                if (m_registeredButtons[idx].Button == button)
                {
                    buttonIdx = idx;
                    break;
                }
            }

            if (buttonIdx != -1)
            {
                // Button already registered, add event handler
                m_registeredButtons[buttonIdx].AddHandler(eventHandler);
            }
            else
            {
                // Register new button event handler
                RegisteredGamePadButton rb = new RegisteredGamePadButton(button);
                rb.AddHandler(eventHandler);

                m_registeredButtons.Add(rb);
            }
        }

        public void UnregisterGamePadButton(Buttons button, GamePadButtonEventHandler eventHandler)
        {
            for (int idx = 0; idx < m_registeredButtons.Count; ++idx)
            {
                if (m_registeredButtons[idx].Button == button)
                {
                    m_registeredButtons[idx].RemoveHandler(eventHandler);

                    if (m_registeredButtons[idx].Empty)
                    {
                        m_registeredButtons.RemoveAt(idx);
                    }

                    break;
                }
            }
        }

        public void RegisterKey(Keys key, KeyboardEventHandler eventHandler)
        {
            int keyIdx = -1;
            for (int idx = 0; idx < m_registeredKeys.Count; ++idx)
            {
                if (m_registeredKeys[idx].Key == key)
                {
                    keyIdx = idx;
                    break;
                }
            }

            if (keyIdx != -1)
            {
                m_registeredKeys[keyIdx].AddHandler(eventHandler);
            }
            else
            {
                RegisteredKey rk = new RegisteredKey(key);
                rk.AddHandler(eventHandler);

                m_registeredKeys.Add(rk);
            }
        }

        public void UnregisterKey(Keys key, KeyboardEventHandler eventHandler)
        {
            for (int idx = 0; idx < m_registeredKeys.Count; ++idx)
            {
                if (m_registeredKeys[idx].Key == key)
                {
                    m_registeredKeys[idx].RemoveHandler(eventHandler);

                    if (m_registeredKeys[idx].Empty)
                    {
                        m_registeredKeys.RemoveAt(idx);
                    }

                    break;
                }
            }
        }
        #endregion

        public void Update()
        {
            m_prevKeyboardState = m_keyboardState;
            m_keyboardState = Keyboard.GetState();

            m_prevMouseState = m_mouseState;
            m_mouseState = Mouse.GetState();

            // Only support single player
            m_prevGamePadStateP1 = m_gamePadStateP1;
            m_gamePadStateP1 = GamePad.GetState(PlayerIndex.One);

            QueryInputsAndFireEvents();
        }

        private void QueryInputsAndFireEvents()
        {
            // Check Variable Inputs
            foreach (RegisteredVariableInput rvi in m_registeredVariableInput)
            {
                Vector2 value = Vector2.Zero;
                bool haveInput = false;
                switch (rvi.InputType)
                {
                    case VariableInputType.Mouse:
                        if ((MouseState.X != PrevMouseState.X) ||
                            (MouseState.Y != PrevMouseState.Y))
                        {
                            value = new Vector2(MouseState.X, MouseState.Y);
                            haveInput = true;
                        }
                        break;
                    case VariableInputType.LeftThumbStick:
                        if (GamePadState.ThumbSticks.Left != PrevGamePadState.ThumbSticks.Left)
                        {
                            value = GamePadState.ThumbSticks.Left;
                            haveInput = true;
                        }
                        break;
                    case VariableInputType.RightThumbStick:
                        if (GamePadState.ThumbSticks.Right != PrevGamePadState.ThumbSticks.Right)
                        {
                            value = GamePadState.ThumbSticks.Right;
                            haveInput = true;
                        }
                        break;
                    case VariableInputType.LeftTrigger:
                        if (GamePadState.Triggers.Left != PrevGamePadState.Triggers.Left)
                        {
                            value = new Vector2(GamePadState.Triggers.Left, 0);
                            haveInput = true;
                        }
                        break;
                    case VariableInputType.RightTrigger:
                        if (GamePadState.Triggers.Right != PrevGamePadState.Triggers.Right)
                        {
                            value = new Vector2(GamePadState.Triggers.Right, 0);
                            haveInput = true;
                        }
                        break;
                    default:
                        // TODO: error!
                        break;
                };

                if (haveInput == true)
                {
                    VariableInputEvent e = new VariableInputEvent(InputEventType.ChangedValue, rvi.InputType, value);
                    rvi.FireEvent(e);
                }
            }

            // Check Buttons
            foreach (RegisteredGamePadButton rb in m_registeredButtons)
            {
                if (PrevGamePadState.IsButtonDown(rb.Button) != GamePadState.IsButtonDown(rb.Button))
                {
                    GamePadButtonEvent e;
                    if (GamePadState.IsButtonDown(rb.Button))
                    {
                        e = new GamePadButtonEvent(InputEventType.Pressed, rb.Button);
                    }
                    else
                    {
                        e = new GamePadButtonEvent(InputEventType.Released, rb.Button);
                    }

                    rb.FireEvent(e);
                }
            }

            // Check Keys
            foreach (RegisteredKey rkey in m_registeredKeys)
            {
                if (PrevKeyboardState.IsKeyDown(rkey.Key) != KeyboardState.IsKeyDown(rkey.Key))
                {
                    KeyboardEvent e;
                    if (KeyboardState.IsKeyDown(rkey.Key))
                    {
                        e = new KeyboardEvent(InputEventType.Pressed, rkey.Key);
                    }
                    else
                    {
                        e = new KeyboardEvent(InputEventType.Released, rkey.Key);
                    }

                    rkey.FireEvent(e);
                }
            }
        }

        #region Properties
        public KeyboardState PrevKeyboardState
        {
            get { return m_prevKeyboardState; }
        }

        public KeyboardState KeyboardState
        {
            get { return m_keyboardState; }
        }

        public MouseState PrevMouseState
        {
            get { return m_prevMouseState; }
        }

        public MouseState MouseState
        {
            get { return m_mouseState; }
        }

        public GamePadState PrevGamePadState
        {
            get { return m_prevGamePadStateP1; }
        }

        public GamePadState GamePadState
        {
            get { return m_gamePadStateP1; }
        }
        #endregion
    }
}
