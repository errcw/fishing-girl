using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using Library.Sprite;
using Library.Extensions;

using FishingGirl.Properties;

namespace FishingGirl
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FishingGame : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Creats a new game.
        /// </summary>
        public FishingGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Window.Title = Resources.FishingGirl;

            //Components.Add(new GamerServicesComponent(this));
            //Microsoft.Xna.Framework.GamerServices.Guide.SimulateTrialMode = true;
        }

        /// <summary>
        /// Initialises this game.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Loads the content for all the game components.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            Content.RootDirectory = "Content";

            _testSprite = Content.Load<ImageSprite>("FishSmall1Body");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Updates the game state.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float time = gameTime.GetElapsedSeconds();
            if (time <= 0 || !IsActive)
            {
                return; // discard "empty" updates
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            _testSprite.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Sprite _testSprite;
    }
}
