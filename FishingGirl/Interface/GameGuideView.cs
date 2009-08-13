using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Animation;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Displays a text label to guide the player's actions.
    /// </summary>
    public class GameGuideView
    {
        /// <summary>
        /// Creates the text sprites.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _bubble = content.Load<SpriteDescriptorTemplate>("Sprites/GuideView").Create();

            _font = content.Load<SpriteFont>("Fonts/Text");
            _buttonA = content.Load<ImageSpriteTemplate>("ButtonA").Create();
            _buttonB = content.Load<ImageSpriteTemplate>("ButtonB").Create();
        }

        /// <summary>
        /// Updates the position of the text.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            if (_bubbleAnimation != null)
            {
                if (!_bubbleAnimation.Update(time))
                {
                    _bubbleAnimation = null;
                }
            }
        }

        /// <summary>
        /// Draws this guide text near the top of the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _bubble.Sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Shows this guide with the given text.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public void Show(string text)
        {
            CompositeSprite textSprite = _bubble.GetSprite<CompositeSprite>("Text");
            textSprite.Clear();
            textSprite.Add(CreateTextSprite(text));

            _bubble.GetSprite("Money").Color = Color.TransparentWhite;

            _bubbleAnimation = _bubble.GetAnimation("Show");
            _bubbleAnimation.Start();
        }

        /// <summary>
        /// Shows this guide with the given text and money.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="money">The amount of money to display.</param>
        public void Show(string text, int money)
        {
            Show(text);
            _bubble.GetSprite<TextSprite>("MoneyText").Text = money.ToString();
            _bubble.GetSprite("Money").Color = Color.White;
        }

        /// <summary>
        /// Hides this text.
        /// </summary>
        public void Hide()
        {
            _bubbleAnimation = _bubble.GetAnimation("Hide");
            _bubbleAnimation.Start();
        }

        /// <summary>
        /// Returns a sprite containing the given text, with substitutions
        /// for special character sequences.
        /// </summary>
        private CompositeSprite CreateTextSprite(string text)
        {
            CompositeSprite sprite = new CompositeSprite();

            float x = 0f;
            string[] parts = text.Split('_');
            foreach (string part in parts)
            {
                Sprite partSprite;
                switch (part)
                {
                    case "A": partSprite = _buttonA; break;
                    case "B": partSprite = _buttonB; break;
                    default: partSprite = new TextSprite(_font, part); break;
                }
                partSprite.Position = new Vector2(x, 0f);

                x += partSprite.Size.X;
                sprite.Add(partSprite);
            }

            return sprite;
        }

        private SpriteDescriptor _bubble;
        private IAnimation _bubbleAnimation;

        private SpriteFont _font;
        private Sprite _buttonA;
        private Sprite _buttonB;

        private readonly Vector2 TitleAreaOffset;
    }
}