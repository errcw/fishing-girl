using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;
using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    /// <summary>
    /// Displays the end-of-game narrative.
    /// </summary>
    public class StoreScreen : Screen
    {
        /// <summary>
        /// The store this screen is displaying.
        /// </summary>
        public Store Store { get; set; }

        public StoreScreen(FishingGameContext context)
        {
            _context = context;
            ShowBeneath = true;
            TransitionOnTime = 0.5f;
            TransitionOffTime = 0.25f;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            _screenDesc = content.Load<SpriteDescriptorTemplate>("Sprites/StoreScreen").Create();
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
        }

        protected override void UpdateTransitionOn(float time, float progress, bool pushed)
        {
            _screenDesc.Sprite.Color = new Color(_screenDesc.Sprite.Color, progress);
        }

        protected override void UpdateTransitionOff(float time, float progress, bool pushed)
        {
            _screenDesc.Sprite.Color = new Color(_screenDesc.Sprite.Color, 1 - progress);
        }

        protected override void UpdateActive(float time)
        {
            if (_context.Input.Cancel.Pressed)
            {
                Stack.Pop();
            }
        }

        private SpriteDescriptor _screenDesc;
        private FishingGameContext _context;
    }
}
