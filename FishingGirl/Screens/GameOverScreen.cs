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
    public class GameOverScreen : Screen
    {
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
            _screenDesc = content.Load<SpriteDescriptorTemplate>("Sprites/GameOverScreen").Create();
            _screenDesc.Sprite.Color = Color.TransparentWhite;
            _screenDesc.GetSprite<TextSprite>("Ending").Text = Resources.EndingText;
            _screenDesc.GetSprite<TextSprite>("Prompt1").Text = Resources.EndingPrompt1;
            _screenDesc.GetSprite<TextSprite>("Prompt2").Text = Resources.EndingPrompt2;

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

        protected override void UpdateTransitionOn(float time, float progress, bool pushed)
        {
            if (pushed)
            {
                // coming from game, so fade in this screen
                _screenDesc.Sprite.Color = new Color(_screenDesc.Sprite.Color, progress);
            }
            else
            {
                // coming from transition to new game, so remove this screen
                Stack.Pop();
                _screenDesc.Sprite.Color = Color.TransparentWhite; // reset
            }
        }

        protected override void UpdateActive(float time)
        {
            if (_context.Input.Action.Pressed)
            {
                Stack.Push(_transitionScreen);
            }
        }

        private SpriteDescriptor _screenDesc;
        private TransitionScreen _transitionScreen;

        private FishingGameContext _context;
    }
}
