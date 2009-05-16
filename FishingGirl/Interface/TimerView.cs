using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Sprite.Pipeline;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Draws timers.
    /// </summary>
    public class TimerView
    {
        /// <summary>
        /// Creates a new timer view.
        /// </summary>
        /// <param name="timer">The timer to display.</param>
        public TimerView(Timer timer)
        {
            _timer = timer;
        }

        /// <summary>
        /// Loads the content for the timer display.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _timerDescriptor = content.Load<SpriteDescriptorTemplate>("Sprites/TimerView").Create();
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            if (_prevTime != _timer.Time)
            {
                TextSprite timeSprite = _timerDescriptor.GetSprite<TextSprite>("Text");
                timeSprite.Text = FormatTime(_timer.Time);
                timeSprite.Position = new Vector2(_timerDescriptor.Sprite.Size.X - timeSprite.Size.X, timeSprite.Position.Y);

                if (Math.Abs(_prevTime - _timer.Time) > AnimationThreshold)
                {
                    _changeAnimation = _timerDescriptor.GetAnimation("ChangeTime");
                    _changeAnimation.Start();
                }

                _prevTime = _timer.Time;
            }
            if (_changeAnimation != null)
            {
                if (!_changeAnimation.Update(time))
                {
                    _changeAnimation = null;
                }
            }
        }

        /// <summary>
        /// Draws the timer.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _timerDescriptor.Sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Returns a time formated as MM:SS.
        /// </summary>
        /// <param name="time">The total time in seconds.</param>
        private string FormatTime(float time)
        {
            TimeSpan span = TimeSpan.FromSeconds(time);
            return String.Format("{0,2}:{1,2:00}", span.Minutes, span.Seconds);
        }

        private Timer _timer;
        private float _prevTime;

        private SpriteDescriptor _timerDescriptor;
        private IAnimation _changeAnimation;

        private const float AnimationThreshold = 1f;
    }
}
