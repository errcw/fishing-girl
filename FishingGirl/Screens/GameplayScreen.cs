using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Screen;
using Library.Sprite;

using FishingGirl.Gameplay;
using FishingGirl.Interface;

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
            _camera = new CameraSprite(_context.Game.GraphicsDevice);
            _cameraController = new CameraController(_camera);
            _scene = new Scene(_camera);
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _pauseScreen = new PauseMenuScreenFactory().Create(_context, content);
            _scene.LoadContent(content);
        }

        /// <summary>
        /// Draws this game.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, _camera.Transform);
            _scene.Draw(spriteBatch);
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
                Stack.Push(_pauseScreen);
                return;
            }

            _cameraController.Update(time);
            _scene.Update(time);
        }

        private CameraSprite _camera;
        private CameraController _cameraController;

        private Scene _scene;

        private MenuScreen _pauseScreen;

        private FishingGameContext _context;
    }
}
