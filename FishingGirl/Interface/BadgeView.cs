using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{

    /// <summary>
    /// Displays the badges earned by the player.
    /// </summary>
    public class BadgeView
    {
        /// <summary>
        /// Creates a new badge view.
        /// </summary>
        /// <param name="badges">The badges to display.</param>
        public BadgeView(Badges badges)
        {
            _badges = badges;
            _badges.BadgeEarned += OnBadgeEarned;
        }

        /// <summary>
        /// Loads the badges sprite.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _sprite = content.Load<SpriteDescriptorTemplate>("Sprites/Badge").Create();
        }

        /// <summary>
        /// Updates the badge animation.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            if (_animation != null)
            {
                if (!_animation.Update(time))
                {
                    if (_sprite.Sprite.Color.A == 255)
                    {
                        // the badge is visible, now hide it
                        _animation = _sprite.GetAnimation("Hide");
                    }
                    else
                    {
                        // the badge is hidden; the show/hide cycle is complete
                        _animation = null;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the badge.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Notifies us that a badge has been earned.
        /// </summary>
        private void OnBadgeEarned(object badgesObj, BadgeEventArgs args)
        {
            _sprite.GetSprite<TextSprite>("Name").Text = args.Badge.Name;
            _sprite.GetSprite<TextSprite>("Description").Text = args.Badge.Description;
            _animation = _sprite.GetAnimation("Show");
        }

        private Badges _badges;

        private SpriteDescriptor _sprite;
        private IAnimation _animation;
    }
}
