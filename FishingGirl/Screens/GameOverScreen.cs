using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    /// <summary>
    /// Displays the end-of-game narrative.
    /// </summary>
    public class GameOverScreen : Screen
    {
        /// <summary>
        /// If the game should display the winning or losing screen.
        /// </summary>
        public bool IsWon
        {
            set
            {
                _screenDesc = value ? _screenDescWon : _screenDescLost;
            }
        }

        public GameOverScreen(FishingGameContext context)
        {
            _context = context;

            ShowBeneath = true;
            TransitionOnTime = 1f;
            TransitionOffTime = 0f;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            _screenDescLost = content.Load<SpriteDescriptorTemplate>("Sprites/EndingLoseScreen").Create();
            _screenDescLost.GetSprite<TextSprite>("Ending").Text = Resources.EndingLostText;
            _screenDescLost.GetSprite<TextSprite>("Prompt1").Text = Resources.EndingPrompt1;
            _screenDescLost.GetSprite<TextSprite>("Prompt2").Text = Resources.EndingPrompt2;

            _screenDescWon = content.Load<SpriteDescriptorTemplate>("Sprites/EndingWinScreen").Create();
            _screenDescWon.GetSprite<TextSprite>("Ending").Text = Resources.EndingWinText;
            _screenDescWon.GetSprite<TextSprite>("Prompt1").Text = Resources.EndingPrompt1;
            _screenDescWon.GetSprite<TextSprite>("Prompt2").Text = Resources.EndingPrompt2;

            _transitionScreen = new TransitionScreen();
            _transitionScreen.FadeColor = Color.Black;
            _transitionScreen.LoadContent(content);
        }

        /// <summary>
        /// Draws this screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            _screenDesc.Sprite.Draw(spriteBatch);
            spriteBatch.End();
        }

        protected override void Show(bool pushed)
        {
            base.Show(pushed);
            if (pushed)
            {
                _screenDesc.GetAnimation("Show").Start();
            }
        }

        protected override void UpdateTransitionOn(float time, float progress, bool pushed)
        {
            if (pushed)
            {
                // coming from game, so show this screen
                _screenDesc.GetAnimation("Show").Update(time);
            }
            else
            {
                // coming from transition to new game, so remove this screen
                Stack.Pop();
            }
        }

        protected override void UpdateActive(float time)
        {
            if (_context.Input.Action.Pressed)
            {
                Stack.Push(_transitionScreen);
            }
        }

        private SpriteDescriptor _screenDescLost;
        private SpriteDescriptor _screenDescWon;
        private SpriteDescriptor _screenDesc;

        private TransitionScreen _transitionScreen;

        private FishingGameContext _context;
    }
}
