using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Ivy
{
    public enum InputEventType
    {
        Pressed,
        Released,
        ChangedValue
    };

    public enum VariableInputType
    {
        Mouse, 
        RightThumbStick,
        LeftThumbStick,
        RightTrigger,
        LeftTrigger,
    };

    public class VariableInputEvent
    {
        public InputEventType EventType { get; private set; }
        public VariableInputType InputType { get; private set; }
        public Vector2 Value { get; private set; }

        // TODO:  put DX/DY value in to show change relative to last position

        public VariableInputEvent(InputEventType eventType, VariableInputType inputType, Vector2 value)
        {
            EventType = eventType;
            InputType = inputType;
            Value = value;
        }
    }

    public class GamePadButtonEvent
    {
        public InputEventType EventType { get; private set; }
        public Buttons Button { get; private set; }

        public GamePadButtonEvent(InputEventType type, Buttons button)
        {
            EventType = type;
            Button = button;
        }
    }

    public class KeyboardEvent
    {
        public InputEventType EventType { get; private set; }
        public Keys Key { get; private set; }

        public KeyboardEvent(InputEventType type, Keys key)
        {
            EventType = type;
            Key = key;
        }
    }
}
