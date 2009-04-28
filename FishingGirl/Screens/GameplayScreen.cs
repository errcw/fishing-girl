using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Screen;
using Library.Sprite;

namespace FishingGirl.Screens
{
    /// <summary>
    /// A screen that shows the game.
    /// </summary>
    public class GameplayScreen : Screen
    {
        /// <summary>
        /// Creates a new gameplay screen.
        /// </summary>
        public GameplayScreen(FishingGameContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _pauseScreen = new PauseMenuScreenFactory().Create(_context, content);
        }

        /// <summary>
        /// Draws this game.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None/*, _camera.Transform*/);
            spriteBatch.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            spriteBatch.End();
        }

        /// <summary>
        /// Updates the gameplay for this frame.
        /// </summary>
        protected override void UpdateActive(float time)
        {
            if (_context.Input.Start.Pressed)
            {
                PauseGame();
                return;
            }
        }

        /// <summary>
        /// Pauses this game and shows the pause menu.
        /// </summary>
        private void PauseGame()
        {
            Stack.Push(_pauseScreen);
        }

        private MenuScreen _pauseScreen;

        private FishingGameContext _context;
    }
}
