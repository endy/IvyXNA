using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Ivy
{
    public class Timer
    {
        public delegate void TimeExpiredEvent();

        public TimeExpiredEvent OnTimeExpired;

        public int TimeRemaining { get; private set; }
        public bool Paused { get; set; }

        public Timer(int totalTime)
        {
            TimeRemaining = totalTime;
            Paused = true;
        }

        public void Start()
        {
            Paused = false;
        }

        public void Update(GameTime gameTime)
        {
            if (Paused == false)
            {
                TimeRemaining -= gameTime.ElapsedGameTime.Milliseconds;
            }

            if ((TimeRemaining <= 0) && (OnTimeExpired != null))
            {
                OnTimeExpired();
            }
        }
    }
}
