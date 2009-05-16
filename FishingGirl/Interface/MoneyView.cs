using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Displays the amount of money earned by the player.
    /// </summary>
    public class MoneyView
    {
        /// <summary>
        /// Creates a new money view.
        /// </summary>
        /// <param name="money">The money to display.</param>
        public MoneyView(Money money)
        {
            _money = money;
        }

        /// <summary>
        /// Loads the money sprite.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _sprite = content.Load<SpriteDescriptorTemplate>("Sprites/MoneyView").Create();
        }

        /// <summary>
        /// Updates the money animation
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            if (_money.Amount != _lastAmount)
            {
                _lastAmount = _money.Amount;

                float width = _sprite.Sprite.Size.X;
                TextSprite moneySprite = _sprite.GetSprite<TextSprite>("Text");
                moneySprite.Text = _money.Amount.ToString();
                moneySprite.Position = new Vector2(width - moneySprite.Size.X, moneySprite.Position.Y);

                _animation = _sprite.GetAnimation("ChangeAmount");
                _animation.Start();
            }

            if (_animation != null)
            {
                if (!_animation.Update(time))
                {
                    _animation = null;
                }
            }
        }

        /// <summary>
        /// Draws the money.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Sprite.Draw(spriteBatch);
        }

        private Money _money;
        private int _lastAmount;

        private SpriteDescriptor _sprite;
        private IAnimation _animation;
    }
}
